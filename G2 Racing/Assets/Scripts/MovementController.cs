using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("=== 飞行参数 ===")]
    public float forwardSpeed = 50f;
    public float turnRate = 120f;

    [Header("=== 转向平滑 ===")]
    public float turnSmoothTime = 0.08f;

    [Header("=== 上下浮动 ===")]
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2f;

    [Header("=== 倾斜效果 ===")]
    public float maxRollAngle = 25f;        // 最大倾斜角度
    public float rollSmoothTime = 0.1f;     // 倾斜平滑速度
    public float rollReturnSpeed = 3f;      // 回正速度

    private Rigidbody rb;
    private float currentYaw = 0f;
    private float currentYawVelocity = 0f;
    private Vector3 startPosition;
    private float timeOffset;

    private float currentRoll = 0f;          // 当前倾斜角度
    private float targetRoll = 0f;           // 目标倾斜角度
    private float rollVelocity = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;

        startPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // === 转向输入 ===
        float horizontalInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            float direction = Mathf.Sign(horizontalInput);

            // 累积转向角度
            float deltaAngle = turnRate * Time.deltaTime * direction;
            currentYaw += deltaAngle;
            currentYaw = Mathf.Clamp(currentYaw, -180f, 180f);

            // 设置目标倾斜角度（左转左倾，右转右倾）
            targetRoll = -direction * maxRollAngle;
        }
        else
        {
            // 没有输入时，回正
            targetRoll = 0f;
        }
    }

    void FixedUpdate()
    {
        // === 1. 计算浮动偏移 ===
        float floatOffset = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmplitude;
        float targetY = startPosition.y + floatOffset;

        // === 2. 计算水平移动 ===
        Vector3 horizontalMovement = transform.forward * forwardSpeed * Time.fixedDeltaTime;
        Vector3 newPosition = rb.position + horizontalMovement;
        newPosition.y = targetY;
        rb.MovePosition(newPosition);

        // === 3. 转向 ===
        float smoothYaw = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            currentYaw,
            ref currentYawVelocity,
            turnSmoothTime
        );

        // === 4. 倾斜（Roll）平滑 ===
        currentRoll = Mathf.SmoothDamp(
            currentRoll,
            targetRoll,
            ref rollVelocity,
            rollSmoothTime
        );

        // === 5. 组合旋转：Yaw（偏航）+ Roll（倾斜）===
        Quaternion targetRotation = Quaternion.Euler(0, smoothYaw, 0) *
                                    Quaternion.Euler(0, 0, currentRoll);
        rb.MoveRotation(targetRotation);
    }

    public void ResetOrientation()
    {
        currentYaw = 0f;
        currentYawVelocity = 0f;
        currentRoll = 0f;
        targetRoll = 0f;
        rollVelocity = 0f;
        rb.MoveRotation(Quaternion.identity);
    }
}