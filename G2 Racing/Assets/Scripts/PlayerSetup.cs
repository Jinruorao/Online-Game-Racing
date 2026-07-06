using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera PlayerCamera;

    /// <summary>本地玩家实例（静态引用，方便其他组件访问）</summary>
    public static GameObject LocalPlayerInstance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;

            // 启用本地玩家控制脚本（CarMovement + MovementController 兼容）
            CarMovement carMove = GetComponent<CarMovement>();
            if (carMove != null) carMove.enabled = true;

            MovementController movementCtrl = GetComponent<MovementController>();
            if (movementCtrl != null) movementCtrl.enabled = true;

            // 启用本地摄像机
            PlayerCamera.enabled = true;

            // 设置 PhotonView 为 UnreliableOnChange 同步模式
            PhotonView pv = GetComponent<PhotonView>();
            if (pv != null)
            {
                pv.Synchronization = ViewSynchronization.UnreliableOnChange;
            }

            // 确保 CarSync 已挂载（由 Prefab 设计时添加，此处只做检查）
            if (GetComponent<CarSync>() == null)
            {
                Debug.LogWarning("[PlayerSetup] 建议在 Player Prefab 上挂载 CarSync 组件以获得最佳同步效果。");
            }
        }
        else
        {
            // Remote player: 禁用所有本地控制脚本
            CarMovement carMove = GetComponent<CarMovement>();
            if (carMove != null) carMove.enabled = false;

            MovementController movementCtrl = GetComponent<MovementController>();
            if (movementCtrl != null) movementCtrl.enabled = false;

            PlayerCamera.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (photonView != null && photonView.IsMine)
        {
            LocalPlayerInstance = null;
        }
    }
}
