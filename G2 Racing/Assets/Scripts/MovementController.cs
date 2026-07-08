using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("=== 飞行参数 ===")]
    public float forwardSpeed = 50f;
    public float turnRate = 120f;

    [Header("=== 转向平滑 ===")]
    public float turnSmoothTime = 0.08f;

    [Header("=== 贴地参数 ===")]
    public float heightAboveGround = 1.5f;
    public float heightSmoothTime = 0.15f;
    public float groundCheckDistance = 10f;
    public LayerMask groundLayer;
    public float gravityForce = 20f;

    [Header("=== 倾斜效果 ===")]
    public float maxRollAngle = 25f;
    public float rollSmoothTime = 0.1f;

    private Rigidbody rb;
    private float currentYaw;
    private float currentYawVelocity = 0f;
    private float currentRoll = 0f;
    private float targetRoll = 0f;
    private float rollVelocity = 0f;
    private float currentHeightVelocity = 0f;
    private bool hasGround = false;
    private float lastGroundTime = 0f;

    void Start()
    {
        currentYaw = transform.eulerAngles.y;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // ★ 自动修复：如果 groundLayer 没设置，回退到所有层
        if (groundLayer.value == 0)
        {
            groundLayer = ~0;
        }
    }

    void Update()
    {
        CheckGround();
        float horizontalInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            float direction = Mathf.Sign(horizontalInput);
            currentYaw += turnRate * Time.deltaTime * direction;
            targetRoll = -direction * maxRollAngle;
        }
        else
        {
            targetRoll = 0f;
        }
    }

    void FixedUpdate()
    {
        Vector3 newPosition = rb.position;
        if (hasGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer)
                || Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance * 2))
            {
                float targetHeight = hit.point.y + heightAboveGround;
                float smoothY;
                if (heightSmoothTime > 0.001f)
                    smoothY = Mathf.SmoothDamp(rb.position.y, targetHeight, ref currentHeightVelocity, heightSmoothTime);
                else
                    smoothY = targetHeight;
                smoothY = Mathf.Max(smoothY, hit.point.y + 0.1f);
                newPosition.y = smoothY;
            }
        }
        else
        {
            float fallVelocity = rb.velocity.y - gravityForce * Time.fixedDeltaTime;
            newPosition.y = rb.position.y + fallVelocity * Time.fixedDeltaTime;
            if (fallVelocity < -30f) fallVelocity = -30f;
            RaycastHit fallbackHit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out fallbackHit, groundCheckDistance * 3))
            {
                hasGround = true;
                newPosition.y = fallbackHit.point.y + heightAboveGround;
            }
        }
        Vector3 horizontalMovement = transform.forward * forwardSpeed * Time.fixedDeltaTime;
        newPosition.x += horizontalMovement.x;
        newPosition.z += horizontalMovement.z;
        rb.MovePosition(newPosition);

        float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, currentYaw, ref currentYawVelocity, turnSmoothTime);
        currentRoll = Mathf.SmoothDamp(currentRoll, targetRoll, ref rollVelocity, rollSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0, smoothYaw, 0) * Quaternion.Euler(0, 0, currentRoll));
    }

    void CheckGround()
    {
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer)
                       || Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance * 2);
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
    }

    public void ResetOrientation()
    {
        currentYaw = 0f; currentYawVelocity = 0f;
        currentRoll = 0f; targetRoll = 0f; rollVelocity = 0f;
        rb.MoveRotation(Quaternion.identity);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle { fontSize = 20, normal = { textColor = Color.white } };
        string s = hasGround ? $"✅ 贴地 {transform.position.y:F1}" : "❌ 坠落中";
        GUI.Label(new Rect(20, 55, 300, 30), s, style);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

    public void ResetSpeedMultiplier() { }
}
