<<<<<<< HEAD
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MovementController : MonoBehaviourPun, IPunObservable
{
    [Header("飞行参数")]
    public float forwardSpeed = 50f;
    public float turnRate = 120f;

    [Header("转向平滑")]
    public float turnSmoothTime = 0.08f;

    [Header("上下浮动")]
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2f;

    [Header("倾斜效果")]
    public float maxRollAngle = 25f;
    public float rollSmoothTime = 0.1f;
    public float rollReturnSpeed = 3f;

    [Header("网络插值速度")]
    public float networkLerpSpeed = 12f; // 位置跟随速度
    public float networkRotLerpSpeed = 12f; // 旋转跟随速度
=======
﻿using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("=== 飞行参数 ===")]
    public float forwardSpeed = 50f;
    public float turnRate = 120f;

    [Header("=== 转向平滑 ===")]
    public float turnSmoothTime = 0.08f;

    [Header("=== 贴地参数 ===")]
    public float heightAboveGround = 1.5f;      // 飞船离地高度
    public float heightSmoothTime = 0.15f;      // 高度跟随平滑度
    public float groundCheckDistance = 10f;     // 地面检测距离
    public LayerMask groundLayer;               // 地面层
    public float gravityForce = 20f;            // 没有地面时的重力

    [Header("=== 倾斜效果 ===")]
    public float maxRollAngle = 25f;
    public float rollSmoothTime = 0.1f;

    [Header("=== 翻滚特技 ===")]
    public float rollDuration = 0.8f;
    public int tapsForRoll = 4;
    public float groundLostDelay = 0.5f;

    [Header("=== 加速系统 ===")]
    public float baseForwardSpeed = 50f;        // 基础速度
    public float currentSpeedMultiplier = 1f;   // 当前速度倍率
>>>>>>> origin/JIN10086

    private Rigidbody rb;
    private float currentYaw = 0f;
    private float currentYawVelocity = 0f;
<<<<<<< HEAD
    private Vector3 startPosition;
    private float timeOffset;
=======
>>>>>>> origin/JIN10086

    private float currentRoll = 0f;
    private float targetRoll = 0f;
    private float rollVelocity = 0f;

<<<<<<< HEAD
    // ---------- 网络同步专用变量 ----------
    private Vector3 networkTargetPosition;
    private float networkTargetYaw;
    private float networkTargetRoll;

    // 用于SmoothDamp的临时速度（引用传递）
    private Vector3 networkPosVelocity = Vector3.zero;
    private float networkYawVelocity = 0f;
    private float networkRollVelocity = 0f;
=======
    // 高度相关
    private float currentHeightVelocity = 0f;
    private bool hasGround = false;
    private float lastGroundTime = 0f;

    // 翻滚相关
    private bool isRolling = false;
    private float rollStartTime = 0f;
    private float rollProgress = 0f;
    private int turnCounter = 0;
    private float lastInputTime = 0f;
    private float inputCooldown = 0.2f;
    private bool hasTriggeredGroundRoll = false;
>>>>>>> origin/JIN10086

    void Start()
    {
        rb = GetComponent<Rigidbody>();
<<<<<<< HEAD

        // 如果不是本地玩家，冻结物理模拟以减少性能开销（但仍保留碰撞）
        if (!photonView.IsMine)
        {
            rb.isKinematic = false; // 保持非Kinematic以触发碰撞，但由网络驱动位置
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
        }

        startPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        // 初始化网络目标为自身位置
        networkTargetPosition = transform.position;
        networkTargetYaw = transform.eulerAngles.y;
        networkTargetRoll = 0f;
=======
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
>>>>>>> origin/JIN10086
    }

    void Update()
    {
<<<<<<< HEAD
        // ---------- 只有本地玩家才处理输入 ----------
        if (!photonView.IsMine) return;

        float horizontalInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            float direction = Mathf.Sign(horizontalInput);

            float deltaAngle = turnRate * Time.deltaTime * direction;
            currentYaw += deltaAngle;
            currentYaw = Mathf.Clamp(currentYaw, -180f, 180f);
=======
        // === 地面检测 ===
        CheckGround();

        // === 转向输入 ===
        float horizontalInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontalInput) > 0.01f && !isRolling)
        {
            if (Input.GetButtonDown("Horizontal") && Time.time - lastInputTime > inputCooldown)
            {
                turnCounter++;
                lastInputTime = Time.time;

                if (turnCounter >= tapsForRoll)
                {
                    StartRoll("转向触发");
                    turnCounter = 0;
                }
            }

            float direction = Mathf.Sign(horizontalInput);
            float deltaAngle = turnRate * Time.deltaTime * direction;
            currentYaw += deltaAngle;
>>>>>>> origin/JIN10086

            targetRoll = -direction * maxRollAngle;
        }
        else
        {
<<<<<<< HEAD
            targetRoll = 0f;
=======
            if (!isRolling)
            {
                targetRoll = 0f;
            }
>>>>>>> origin/JIN10086
        }
    }

    void FixedUpdate()
    {
<<<<<<< HEAD
        float smoothYaw = 0f;
        Quaternion targetRotation = Quaternion.Euler(0f, currentYaw, 0f);
        // ---------- 处理远端玩家的平滑插值 ----------
        if (!photonView.IsMine)
        {
            // 位置插值 (使用SmoothDamp避免瞬移和抖动)
            Vector3 smoothPos = Vector3.SmoothDamp(
                rb.position,
                networkTargetPosition,
                ref networkPosVelocity,
                1f / networkLerpSpeed // 时间常数
            );
            rb.MovePosition(smoothPos);

            // 偏航 + 倾斜 分别插值，再组合旋转
            smoothYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                networkTargetYaw,
                ref networkYawVelocity,
                1f / networkRotLerpSpeed
            );

            float smoothRoll = Mathf.SmoothDamp(
                currentRoll, // 这里用currentRoll作为当前值
                networkTargetRoll,
                ref networkRollVelocity,
                1f / networkRotLerpSpeed
            );
            currentRoll = smoothRoll; // 更新当前值以便下一帧使用

            targetRotation = Quaternion.Euler(0, smoothYaw, 0) *
                                        Quaternion.Euler(0, 0, smoothRoll);
            rb.MoveRotation(targetRotation);
            return; // 远端处理完毕，不再执行下面的物理移动
        }

        // ---------- 本地玩家的物理移动 (原有逻辑) ----------
        float floatOffset = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmplitude;
        float targetY = startPosition.y + floatOffset;

        Vector3 horizontalMovement = transform.forward * forwardSpeed * Time.fixedDeltaTime;
        Vector3 newPosition = rb.position + horizontalMovement;
        newPosition.y = targetY;
        rb.MovePosition(newPosition);

        smoothYaw = Mathf.SmoothDampAngle(
=======
        // === 1. 高度控制（贴地或下落） ===
        Vector3 newPosition = rb.position;

        if (hasGround)
        {
            // 有地面：贴地飞行
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
            {
                float targetHeight = hit.point.y + heightAboveGround;

                // 平滑移动到目标高度
                float smoothY = Mathf.SmoothDamp(
                    rb.position.y,
                    targetHeight,
                    ref currentHeightVelocity,
                    heightSmoothTime
                );
                newPosition.y = smoothY;
            }
        }
        else
        {
            // 没有地面：自由落体
            float fallVelocity = rb.velocity.y - gravityForce * Time.fixedDeltaTime;
            newPosition.y = rb.position.y + fallVelocity * Time.fixedDeltaTime;

            // 限制最大下落速度
            if (fallVelocity < -50f) fallVelocity = -50f;
        }

        // === 2. 前进移动 ===
        Vector3 horizontalMovement = transform.forward * forwardSpeed * Time.fixedDeltaTime;
        newPosition.x += horizontalMovement.x;
        newPosition.z += horizontalMovement.z;

        rb.MovePosition(newPosition);

        // === 3. 转向（Yaw） ===
        float smoothYaw = Mathf.SmoothDampAngle(
>>>>>>> origin/JIN10086
            transform.eulerAngles.y,
            currentYaw,
            ref currentYawVelocity,
            turnSmoothTime
        );

<<<<<<< HEAD
        currentRoll = Mathf.SmoothDamp(
            currentRoll,
            targetRoll,
            ref rollVelocity,
            rollSmoothTime
        );

        targetRotation = Quaternion.Euler(0, smoothYaw, 0) *
                                    Quaternion.Euler(0, 0, currentRoll);
        rb.MoveRotation(targetRotation);
    }

    // ---------- PUN2 网络序列化 (每帧调用，建议设为 Unreliable) ----------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 发送方（本地玩家）：发送当前位置、偏航角、倾斜角
            stream.SendNext(rb.position);
            stream.SendNext(currentYaw);
            stream.SendNext(currentRoll);
        }
        else
        {
            // 接收方（其他玩家）：接收并存储为目标值
            networkTargetPosition = (Vector3)stream.ReceiveNext();
            networkTargetYaw = (float)stream.ReceiveNext();
            networkTargetRoll = (float)stream.ReceiveNext();
        }
    }

    // ---------- 重置功能（带RPC，确保所有客户端同步重置） ----------
    [PunRPC]
    public void RPC_ResetOrientation()
    {
        // 如果是本地玩家，重置输入状态
        if (photonView.IsMine)
        {
            currentYaw = 0f;
            currentYawVelocity = 0f;
            currentRoll = 0f;
            targetRoll = 0f;
            rollVelocity = 0f;
        }

        // 无论本地还是远端，重置位置和旋转（由调用者决定重置到哪，这里仅示例）
        rb.MovePosition(startPosition);
        rb.MoveRotation(Quaternion.identity);

        // 同步重置网络目标值，避免插值回弹
        networkTargetPosition = startPosition;
        networkTargetYaw = 0f;
        networkTargetRoll = 0f;
        networkPosVelocity = Vector3.zero;
        networkYawVelocity = 0f;
        networkRollVelocity = 0f;
    }

    // 外部调用重置的包装方法（例如UI按钮）
    public void ResetOrientation()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_ResetOrientation", RpcTarget.All);
        }
    }
=======
        // === 4. 翻滚或倾斜 ===
        float finalRoll;

        if (isRolling)
        {
            rollProgress = (Time.time - rollStartTime) / rollDuration;

            if (rollProgress >= 1f)
            {
                isRolling = false;
                finalRoll = 0f;
                currentRoll = 0f;
                targetRoll = 0f;
                rollVelocity = 0f;
                hasTriggeredGroundRoll = false;
            }
            else
            {
                float rollAngle = Mathf.Lerp(0f, 360f, rollProgress);
                finalRoll = rollAngle;
                currentRoll = rollAngle;
            }
        }
        else
        {
            currentRoll = Mathf.SmoothDamp(
                currentRoll,
                targetRoll,
                ref rollVelocity,
                rollSmoothTime
            );
            finalRoll = currentRoll;
        }

        // === 5. 应用旋转 ===
        Quaternion targetRotation = Quaternion.Euler(0, smoothYaw, 0) *
                                    Quaternion.Euler(0, 0, finalRoll);
        rb.MoveRotation(targetRotation);
    }

    // === 地面检测 ===
    void CheckGround()
    {
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer);

        // 可视化射线
        if (isGrounded)
        {
            Debug.DrawLine(transform.position, hit.point, Color.green);
            hasGround = true;
            lastGroundTime = Time.time;
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
            hasGround = false;
        }

        // === 检测地面丢失触发翻滚 ===
        if (!isGrounded && !isRolling && !hasTriggeredGroundRoll && Time.time - lastGroundTime > groundLostDelay)
        {
            StartRoll("地面丢失触发");
            hasTriggeredGroundRoll = true;
            
        }

        // 如果回到地面，重置标记
        if (isGrounded && hasTriggeredGroundRoll)
        {
            hasTriggeredGroundRoll = false;
        }
    }

    // === 触发翻滚 ===
    void StartRoll(string reason)
    {
        if (isRolling) return;

        isRolling = true;
        rollStartTime = Time.time;
        rollProgress = 0f;

        Debug.Log($"🔥 翻滚触发！原因: {reason}");
    }

    // === 重置飞船 ===
    public void ResetOrientation()
    {
        currentYaw = 0f;
        currentYawVelocity = 0f;
        currentRoll = 0f;
        targetRoll = 0f;
        rollVelocity = 0f;
        isRolling = false;
        turnCounter = 0;
        hasTriggeredGroundRoll = false;
        rb.MoveRotation(Quaternion.identity);
    }

    // === 调试UI ===
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string status = isRolling ? "🔥 翻滚中！" : $"转向计数: {turnCounter}/{tapsForRoll}";
        GUI.Label(new Rect(20, 20, 200, 30), status, style);

        string groundStatus = hasGround ? $"✅ 有地面 高度: {transform.position.y:F1}" : "❌ 无地面 - 坠落中！";
        GUI.Label(new Rect(20, 55, 300, 30), groundStatus, style);
    }

    // === Scene视图可视化 ===
    void OnDrawGizmosSelected()
    {
        if (transform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, 0.2f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        }
    }

    public void ResetSpeedMultiplier()
    {
        currentSpeedMultiplier = 1f;
    }
>>>>>>> origin/JIN10086
}