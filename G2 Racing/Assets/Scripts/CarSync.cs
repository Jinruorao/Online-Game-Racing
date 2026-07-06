using UnityEngine;
using Photon.Pun;

/// <summary>
/// 赛车网络同步组件。
/// 使用 OnPhotonSerializeView 在 Owner 和 Observer 间同步位置、旋转和速度。
/// Owner 端每帧写入，Observer 端接收并平滑插值。
/// </summary>
[RequireComponent(typeof(PhotonView), typeof(Rigidbody))]
public class CarSync : MonoBehaviour, IPunObservable
{
    [Header("同步设置")]
    public bool syncVelocity = true;
    public bool syncAngularVelocity = true;
    public bool syncTeleport = true;

    [Header("插值平滑")]
    public float positionSmoothSpeed = 10f;
    public float rotationSmoothSpeed = 8f;

    // 同步数据
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private Vector3 networkAngularVelocity;

    private Rigidbody rb;
    private PhotonView pv;

    // 瞬移检测
    private Vector3 lastSyncedPosition;
    private float teleportDistanceThreshold = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (pv.IsMine)
        {
            // Owner: 初始化同步缓存为本地值
            networkPosition = transform.position;
            networkRotation = transform.rotation;
            networkVelocity = rb.velocity;
            networkAngularVelocity = rb.angularVelocity;
            lastSyncedPosition = transform.position;
        }
        else
        {
            // Observer: 确保碰撞检测模式合理
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine)
        {
            // Observer: 平滑插值到网络位置
            rb.position = Vector3.Lerp(rb.position, networkPosition, positionSmoothSpeed * Time.fixedDeltaTime);
            rb.rotation = Quaternion.Slerp(rb.rotation, networkRotation, rotationSmoothSpeed * Time.fixedDeltaTime);

            if (syncVelocity)
            {
                rb.velocity = networkVelocity;
            }
            if (syncAngularVelocity)
            {
                rb.angularVelocity = networkAngularVelocity;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Owner 写入
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            if (syncVelocity)
            {
                stream.SendNext(rb.velocity);
            }
            if (syncAngularVelocity)
            {
                stream.SendNext(rb.angularVelocity);
            }

            // 瞬移检测标记
            float dist = Vector3.Distance(transform.position, lastSyncedPosition);
            stream.SendNext(dist > teleportDistanceThreshold);
            lastSyncedPosition = transform.position;
        }
        else
        {
            // Observer 读取
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            if (syncVelocity)
            {
                networkVelocity = (Vector3)stream.ReceiveNext();
            }
            if (syncAngularVelocity)
            {
                networkAngularVelocity = (Vector3)stream.ReceiveNext();
            }

            bool isTeleport = (bool)stream.ReceiveNext();
            if (isTeleport)
            {
                // 瞬移：直接跳转不插值
                rb.position = networkPosition;
                rb.rotation = networkRotation;
            }
        }
    }

    /// <summary>
    /// 强制将网络目标瞬间对齐到当前网络数据（Observer 端跳帧时用）
    /// </summary>
    public void TeleportToNetworkPosition()
    {
        if (!pv.IsMine)
        {
            rb.position = networkPosition;
            rb.rotation = networkRotation;
        }
    }
}
