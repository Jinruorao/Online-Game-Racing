using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// 比赛结束管理器（挂载在 GameManager 上）。
/// 统计所有玩家的完赛顺序，全部完赛后显示排名面板，
/// 10 秒后自动退出房间回到 LobbyScene。
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class RaceFinishManager : MonoBehaviourPunCallbacks
{
    [Header("UI 引用")]
    public GameObject resultPanel;          // 排名面板（默认隐藏）
    public Text[] rankTexts;                // 3 个 Text：1st/2nd/3rd
    public Text countdownText;              // "Returning to lobby in 10..."

    private List<string> finishOrder = new List<string>();
    private int totalPlayers = 3;
    private float exitTimer = 10f;

    public static RaceFinishManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    // ================================================================
    //  公开接口：由 NetworkedRaceManager 在玩家完赛时调用
    // ================================================================
    public void ReportFinish(string playerName)
    {
        // 通过 RPC 发送给 MasterClient 登记
        photonView.RPC("RPC_RegisterFinish", RpcTarget.MasterClient, playerName);
    }

    [PunRPC]
    void RPC_RegisterFinish(string playerName, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 去重：防止同一玩家多次报告
        if (!finishOrder.Contains(playerName))
            finishOrder.Add(playerName);

        Debug.Log($"[RaceFinish] {playerName} 完赛！当前 {finishOrder.Count}/{totalPlayers}");

        if (finishOrder.Count >= totalPlayers)
        {
            // 全部完赛 → 广播排名结果
            string[] result = finishOrder.ToArray();
            photonView.RPC("RPC_ShowResults", RpcTarget.All, result);
        }
    }

    [PunRPC]
    void RPC_ShowResults(string[] orderedNames)
    {
        finishOrder = new List<string>(orderedNames);
        ShowResultPanel();
        StartCoroutine(ExitCountdown());
    }

    // ================================================================
    //  UI 显示
    // ================================================================
    void ShowResultPanel()
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);

        string[] suffix = { "st", "nd", "rd" };
        for (int i = 0; i < rankTexts.Length; i++)
        {
            if (i < finishOrder.Count)
            {
                string ord = i < 3 ? suffix[i] : "th";
                rankTexts[i].text = $"{i + 1}{ord}  {finishOrder[i]}";
                rankTexts[i].gameObject.SetActive(true);
            }
            else
            {
                rankTexts[i].gameObject.SetActive(false);
            }
        }
    }

    // ================================================================
    //  10 秒倒计时退出
    // ================================================================
    IEnumerator ExitCountdown()
    {
        float remaining = exitTimer;
        while (remaining > 0)
        {
            if (countdownText != null)
                countdownText.text = $"Returning to lobby in {Mathf.CeilToInt(remaining)}...";
            remaining -= Time.deltaTime;
            yield return null;
        }

        // 离开房间并返回大厅
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    // ================================================================
    //  Photon 回调：离开房间后加载大厅
    // ================================================================
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }
}
