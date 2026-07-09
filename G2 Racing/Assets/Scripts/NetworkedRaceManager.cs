using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// 全局比赛状态管理器。
/// 每辆赛车各有一个实例，由 Owner 向 MasterClient 报告圈数/完成状态，
/// MasterClient 负责广播最终比赛结果。
/// </summary>
public class NetworkedRaceManager : MonoBehaviourPunCallbacks
{
    [Header("比赛配置")]
    public int totalLaps = 3;

    [Header("UI 引用")]
    public Text lapCounterText;       // 如 "2/3"
    public Text positionText;         // 如 "1st / 4"
    public Text raceResultText;       // 比赛结束后显示

    // 本地状态
    private int currentLap = 0;
    private int currentCheckpoint = 0;
    private float raceStartTime;
    private bool raceFinished = false;

    private PhotonView pv;

    // 圈数触发器按顺序存储
    private List<GameObject> orderedTriggers = new List<GameObject>();

    void Start()
    {
        pv = GetComponent<PhotonView>();

        // 从 RacingModeGameManager 获取圈数触发器
        if (RacingModeGameManager.instance != null && RacingModeGameManager.instance.lapTriggers != null)
        {
            foreach (GameObject trigger in RacingModeGameManager.instance.lapTriggers)
            {
                if (trigger != null)
                    orderedTriggers.Add(trigger);
            }
        }

        // 初始化 UI
        if (lapCounterText != null)
            lapCounterText.text = $"0/{totalLaps}";

        raceStartTime = Time.time;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!pv.IsMine) return;
        if (raceFinished) return;

        if (orderedTriggers.Contains(other.gameObject))
        {
            int triggerIndex = orderedTriggers.IndexOf(other.gameObject);

            // 圈数逻辑：通过圈数触发器的顺序来判定
            // 假设触发器顺序: 0, 1, 2, ..., N-1 (start/finish)
            // 每经过一个比当前 checkpoint 大的触发器，更新 checkpoint
            // 到达 finish trigger (index 0) 且 tick 完所有 checkpoint 算一圈

            if (triggerIndex == 0) // 经过起/终点线
            {
                if (currentCheckpoint >= orderedTriggers.Count - 1 || orderedTriggers.Count <= 1)
                {
                    // 完成了所有中间检查点，进入下一圈
                    currentLap++;
                    currentCheckpoint = 0;

                    // 网络广播圈数更新
                    pv.RPC("RPC_UpdateLap", RpcTarget.All, currentLap);

                    if (currentLap >= totalLaps)
                    {
                        // 比赛完成
                        FinishRace();
                    }
                }
            }
            else if (triggerIndex > currentCheckpoint)
            {
                // 按顺序通过检查点
                currentCheckpoint = triggerIndex;
            }
        }
    }

    public void FinishRace()
    {
        if (raceFinished) return;
        raceFinished = true;

        // 通知 RaceFinishManager
        if (RaceFinishManager.Instance != null)
            RaceFinishManager.Instance.ReportFinish(PhotonNetwork.LocalPlayer.NickName);

        float raceTime = Time.time - raceStartTime;

        // 通过 RPC 通知所有客户端该玩家完成比赛
        pv.RPC("RPC_PlayerFinished", RpcTarget.MasterClient, 
               PhotonNetwork.LocalPlayer.NickName, raceTime);
    }

    [PunRPC]
    void RPC_UpdateLap(int lap)
    {
        if (lapCounterText != null)
            lapCounterText.text = $"{lap}/{totalLaps}";
    }

    [PunRPC]
    void RPC_PlayerFinished(string playerName, float raceTime, PhotonMessageInfo info)
    {
        // 只有 MasterClient 处理完成事件
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"🏁 {playerName} 完成比赛！用时: {raceTime:F2}s");

        // MasterClient 广播完成消息给所有玩家
        photonView.RPC("RPC_DisplayResult", RpcTarget.All, playerName, raceTime);
    }

    [PunRPC]
    void RPC_DisplayResult(string playerName, float raceTime)
    {
        if (raceResultText != null)
        {
            float minutes = Mathf.FloorToInt(raceTime / 60);
            float seconds = raceTime % 60;
            raceResultText.text = $"🏁 {playerName}\n{minutes:00}:{seconds:00}";
            raceResultText.gameObject.SetActive(true);
        }

        // 禁用本地控制
        if (pv.IsMine)
        {
            CarMovement carMove = GetComponent<CarMovement>();
            if (carMove != null) carMove.enabled = false;

            MovementController movementCtrl = GetComponent<MovementController>();
            if (movementCtrl != null) movementCtrl.enabled = false;
        }
    }

    /// <summary>
    /// 按赛道顺序注册圈数触发器（在编辑器中调用）
    /// </summary>
    public void RegisterLapTriggers(List<GameObject> orderedTriggersList)
    {
        orderedTriggers = orderedTriggersList;
    }
}

