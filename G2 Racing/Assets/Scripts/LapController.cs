using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LapController : MonoBehaviourPunCallbacks
{
    private List<GameObject> LapTriggers = new List<GameObject>();
    private PhotonView pv;
    public bool raceFinished = false;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (RacingModeGameManager.instance != null && RacingModeGameManager.instance.lapTriggers != null)
        {
            foreach (GameObject lapTrigger in RacingModeGameManager.instance.lapTriggers)
            {
                if (lapTrigger != null)
                {
                    LapTriggers.Add(lapTrigger);
                }
            }
        }
        else
        {
            Debug.LogWarning("[LapController] RacingModeGameManager.instance.lapTriggers 为空，请在场景中设置圈数触发器。");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pv.IsMine) return; // 只有本地玩家触发圈数检测
        if (raceFinished) return;

        if (LapTriggers.Contains(other.gameObject))
        {
            int indexOfTrigger = LapTriggers.IndexOf(other.gameObject);
            LapTriggers[indexOfTrigger].SetActive(false);

            if (other.name == "FinishTrigger" || other.gameObject.name.Contains("Finish"))
            {
                // 游戏结束 — 通过网络 RPC 同步
                pv.RPC("RPC_GameFinished", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void RPC_GameFinished()
    {
        if (raceFinished) return;
        raceFinished = true;

        PlayerSetup setup = GetComponent<PlayerSetup>();
        if (setup != null && setup.PlayerCamera != null)
        {
            setup.PlayerCamera.transform.parent = null;
        }

        // 禁用两种移动控制
        CarMovement carMove = GetComponent<CarMovement>();
        if (carMove != null) carMove.enabled = false;

        MovementController movementCtrl = GetComponent<MovementController>();
        if (movementCtrl != null) movementCtrl.enabled = false;

        Debug.Log($"🏁 {gameObject.name} 完成比赛！");
    }
}
