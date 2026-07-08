using UnityEngine;
using Photon.Pun;

/// <summary>
/// 加速带触发器。
/// 挂载在 BoostPad 预制体上，当赛车进入触发区域时，
/// 对 MovementController 施加一个持续一定时间的速度倍率。
///
/// === 多人游戏兼容 ===
/// 仅对本地玩家（PhotonView.IsMine）施加加速效果，
/// 加速后的位移通过 CarSync 自动同步到其他客户端。
/// 非 Photon 模式（单机测试）下对所有赛车生效。
/// </summary>
[RequireComponent(typeof(Collider))]
public class SpeedBoost : MonoBehaviour
{
    [Header("加速配置")]
    public float boostMultiplier = 1.5f;
    public float boostDuration = 2f;

    [Header("视觉反馈（可选）")]
    public ParticleSystem boostParticle;
    public Color gizmoColor = new Color(0f, 0.8f, 1f, 0.4f);

    private void OnTriggerEnter(Collider other)
    {
        // 检测是否为赛车（通过 MovementController 判定，不依赖 Tag）
        MovementController mc = other.GetComponentInParent<MovementController>();
        if (mc == null)
            return;

        // 多人模式：只给本地玩家的车加速；非 Photon 模式全部生效
        PhotonView pv = other.GetComponentInParent<PhotonView>();
        if (pv != null && PhotonNetwork.IsConnected && !pv.IsMine)
            return;

        mc.ApplyBoost(boostMultiplier, boostDuration);

        if (boostParticle != null)
            boostParticle.Play();

        Debug.Log($"[SpeedBoost] 加速！乘={boostMultiplier}，时长={boostDuration}s — {other.name}");
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = gizmoColor;

        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        else if (col is MeshCollider meshCol && meshCol.sharedMesh != null)
        {
            Gizmos.DrawMesh(meshCol.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
        }
    }
}
