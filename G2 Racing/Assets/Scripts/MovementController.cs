using UnityEngine;

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

    private Rigidbody rb;
    private float currentYaw;
    private float currentYawVelocity = 0f;

    private float currentRoll = 0f;
    private float targetRoll = 0f;
    private float rollVelocity = 0f;

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

    void Start()
    {
      
        currentYaw = transform.eulerAngles.y;
        rb = GetComponent<Rigidbody>();
        //rb.constraints = RigidbodyConstraints.FreezeRotationX |
         //                RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false;
    }

    void Update()
    {
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
                    //StartRoll("转向触发");
                    turnCounter = 0;
                }
            }

            float direction = Mathf.Sign(horizontalInput);
            float deltaAngle = turnRate * Time.deltaTime * direction;
            currentYaw += deltaAngle;

            targetRoll = -direction * maxRollAngle;
        }
        else
        {
            if (!isRolling)
            {
                targetRoll = 0f;
            }
        }
    }

    void FixedUpdate()
    {
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
            transform.eulerAngles.y,
            currentYaw,
            ref currentYawVelocity,
            turnSmoothTime
        );

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
            //StartRoll("地面丢失触发");
            hasTriggeredGroundRoll = true;
            
        }

        // 如果回到地面，重置标记
        if (isGrounded && hasTriggeredGroundRoll)
        {
            hasTriggeredGroundRoll = false;
        }
    }

    // === 触发翻滚 ===
    //void StartRoll(string reason)
    //{
     //   if (isRolling) return;
//
      //  isRolling = true;
     //   rollStartTime = Time.time;
     //   rollProgress = 0f;

      //  Debug.Log($"🔥 翻滚触发！原因: {reason}");
  //  }

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
}