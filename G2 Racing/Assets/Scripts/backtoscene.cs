using UnityEngine;
using Photon.Pun;

/// <summary>
/// 兜底碰撞箱：玩家飞出赛道边界时传送回安全位置。
/// 多人兼容：仅对本地玩家生效，通过 CarSync 自动同步给其他客户端。
/// </summary>
public class backtoscene : MonoBehaviour
{
    [Header("传送目标位置")]
    public Transform targetPos;

    void OnTriggerEnter(Collider other)
    {
        // 检测是否为赛车（支持 Tag 或组件两种方式）
        bool isPlayer = other.CompareTag("Player")
                     || other.GetComponentInParent<MovementController>() != null;

        if (!isPlayer) return;

        PhotonView pv = other.GetComponentInParent<PhotonView>();

        // 多人模式：只处理本地玩家的车
        // 非 Photon 模式（单机测试）下全部处理
        if (pv != null && PhotonNetwork.IsConnected && !pv.IsMine)
            return;

        // 确定要移动的根物体（防止只移动了子碰撞体）
        Transform root = (pv != null) ? pv.transform : other.transform;
        Rigidbody rb = root.GetComponent<Rigidbody>();

        // 传送位置 + 归零速度
        root.position = targetPos.position;
        root.rotation = targetPos.rotation;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 通知 CarSync 强制对齐（避免 Observer 端插值延迟）
        CarSync carSync = root.GetComponent<CarSync>();
        if (carSync != null)
        {
            carSync.TeleportToNetworkPosition();
        }

        // 重置 MovementController 的加速状态
        MovementController mc = root.GetComponent<MovementController>();
        if (mc != null)
        {
            mc.ResetSpeedMultiplier();
        }

        Debug.Log($"[backtoscene] 传送 {root.name} → {targetPos.position}");
    }
}
