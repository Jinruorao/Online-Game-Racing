using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TimeCountDownManager : MonoBehaviourPunCallbacks
{
    private Text TimeUIText;
    private float timeToStartRace = 5.0f;
    private float lastSentTime = -1f; // 去重：避免每帧发 RPC
    private bool raceStarted = false;

    private void Awake()
    {
        if (RacingModeGameManager.instance != null)
        {
            TimeUIText = RacingModeGameManager.instance.TimeUIText;
        }
        else
        {
            Debug.LogWarning("[TimeCountDownManager] RacingModeGameManager.instance 为空");
        }
    }

    void Start()
    {
        // 确保玩家生成后 CarMovement 先禁用，等倒计时结束才启用
    }

    void Update()
    {
        if (raceStarted) return;

        if (PhotonNetwork.IsMasterClient)
        {
            if (timeToStartRace >= 0.0f)
            {
                timeToStartRace -= Time.deltaTime;

                // 只在数值变化时发送 RPC（避免每帧发送）
                int currentInt = Mathf.FloorToInt(timeToStartRace);
                int lastInt = Mathf.FloorToInt(lastSentTime);
                if (currentInt != lastInt || timeToStartRace <= 0f)
                {
                    photonView.RPC("RPC_SetTime", RpcTarget.All, timeToStartRace);
                    lastSentTime = timeToStartRace;
                }
            }
            else if (!raceStarted)
            {
                photonView.RPC("RPC_StartTheRace", RpcTarget.AllBuffered);
                raceStarted = true;
            }
        }
    }

    [PunRPC]
    public void RPC_SetTime(float time)
    {
        if (TimeUIText == null)
        {
            if (RacingModeGameManager.instance != null)
                TimeUIText = RacingModeGameManager.instance.TimeUIText;
            else
                return;
        }

        if (time > 0.0f)
        {
            TimeUIText.text = Mathf.CeilToInt(time).ToString();
        }
        else
        {
            TimeUIText.text = "GO!";
        }
    }

    [PunRPC]
    public void RPC_StartTheRace()
    {
        raceStarted = true;

        // 同时启用 CarMovement 和 MovementController
        CarMovement carMove = GetComponent<CarMovement>();
        if (carMove != null) carMove.controlsEnabled = true;

        MovementController movementCtrl = GetComponent<MovementController>();
        if (movementCtrl != null) movementCtrl.enabled = true;

        // 清除 UI
        if (TimeUIText != null)
        {
            TimeUIText.text = "";
        }

        this.enabled = false;
    }

    /// <summary>获取当前倒计时剩余时间</summary>
    public float GetRemainingTime()
    {
        return timeToStartRace;
    }
}
