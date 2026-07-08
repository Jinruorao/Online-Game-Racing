using UnityEngine;
using System.Collections;

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
    private float currentYawVelocity;
    private float currentRoll;
    private float targetRoll;
    private float rollVelocity;
    private float currentHeightVelocity;
    private bool hasGround;
    private RaycastHit _groundHit;          // 缓存的地面检测结果，避免 FixedUpdate 重复射线

    // 加速倍率
    private float _currentBoostMultiplier = 1f;
    private Coroutine _boostCoroutine;

    void Start()
    {
        currentYaw = transform.eulerAngles.y;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        if (groundLayer.value == 0)
            groundLayer = ~0;
    }

    void Update()
    {
        CheckGround();

        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
        {
            float dir = Mathf.Sign(h);
            currentYaw += turnRate * Time.deltaTime * dir;
            targetRoll = -dir * maxRollAngle;
        }
        else
        {
            targetRoll = 0f;
        }
    }

    void FixedUpdate()
    {
        Vector3 newPos = rb.position;

        if (hasGround)
        {
            float targetY = _groundHit.point.y + heightAboveGround;
            float smoothY = heightSmoothTime > 0.001f
                ? Mathf.SmoothDamp(rb.position.y, targetY, ref currentHeightVelocity, heightSmoothTime)
                : targetY;
            smoothY = Mathf.Max(smoothY, _groundHit.point.y + 0.1f);
            newPos.y = smoothY;
        }
        else
        {
            float fallVy = rb.velocity.y - gravityForce * Time.fixedDeltaTime;
            newPos.y = rb.position.y + fallVy * Time.fixedDeltaTime;

            // 坠落时仍然尝试找回地面
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down,
                                out RaycastHit hit, groundCheckDistance * 3))
            {
                hasGround = true;
                _groundHit = hit;
                newPos.y = hit.point.y + heightAboveGround;
            }
        }

        // 水平移动（含加速倍率）
        Vector3 move = transform.forward * forwardSpeed * _currentBoostMultiplier * Time.fixedDeltaTime;
        newPos.x += move.x;
        newPos.z += move.z;
        rb.MovePosition(newPos);

        // 转向 + 倾斜
        float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, currentYaw,
                                                ref currentYawVelocity, turnSmoothTime);
        currentRoll = Mathf.SmoothDamp(currentRoll, targetRoll, ref rollVelocity, rollSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0, smoothYaw, 0) * Quaternion.Euler(0, 0, currentRoll));
    }

    /// <summary>检测地面并缓存碰撞信息，供 FixedUpdate 复用。</summary>
    void CheckGround()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, out _groundHit,
                                         groundCheckDistance, groundLayer)
                     || Physics.Raycast(transform.position, Vector3.down, out _groundHit,
                                         groundCheckDistance * 2);

        if (grounded)
        {
            Debug.DrawLine(transform.position, _groundHit.point, Color.green);
            hasGround = true;
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

#if UNITY_EDITOR
    void OnGUI()
    {
        GUIStyle style = new GUIStyle { fontSize = 20, normal = { textColor = Color.white } };
        string s = hasGround ? $"\u2705 贴地 {transform.position.y:F1}" : "\u274c 坠落中";
        GUI.Label(new Rect(20, 55, 300, 30), s, style);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
#endif

    // ====================================================================
    //  加速 / 减速 接口
    // ====================================================================

    /// <summary>
    /// 施加一个持续 duration 秒的速度倍率（由 SpeedBoost 等外部脚本调用）。
    /// multiplier > 1 为加速，&lt; 1 为减速，= 1 为正常速度。
    /// 多人模式下仅在本地客户端调用，加速后的移动通过 CarSync 自动同步。
    /// </summary>
    public void ApplyBoost(float multiplier, float duration)
    {
        if (_boostCoroutine != null)
            StopCoroutine(_boostCoroutine);
        _boostCoroutine = StartCoroutine(BoostRoutine(multiplier, duration));
    }

    private IEnumerator BoostRoutine(float multiplier, float duration)
    {
        _currentBoostMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        _currentBoostMultiplier = 1f;
        _boostCoroutine = null;
    }

    /// <summary>立即取消所有加速效果，恢复原始速度。</summary>
    public void ResetSpeedMultiplier()
    {
        if (_boostCoroutine != null)
            StopCoroutine(_boostCoroutine);
        _currentBoostMultiplier = 1f;
        _boostCoroutine = null;
    }
}
