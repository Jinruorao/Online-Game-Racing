using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("=== 护盾参数 ===")]
    public float shieldDuration = 5f;           // 护盾持续秒数（过期自动消失）

    [Header("=== 引用 ===")]
    public RespawnSystem respawnSystem;

    // 护盾状态
    private bool isShieldActive = false;
    private float shieldTimer = 0f;

    // 视觉引用
    private Renderer shieldRenderer;
    private Color shieldColor;

    void Start()
    {
        shieldRenderer = GetComponent<Renderer>();
        if (shieldRenderer != null)
        {
            shieldColor = shieldRenderer.material.color;
        }

        if (respawnSystem == null)
        {
            respawnSystem = GetComponentInParent<RespawnSystem>();
        }

        // 默认隐藏护盾
        SetShieldVisible(false);
    }

    void Update()
    {
        // 护盾计时（过期自动消失）
        if (isShieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
                Debug.Log("⏰ 护盾过期自动消失");
            }
        }

        // 按 E 激活护盾
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivateShield();
        }
    }

    // === 激活护盾 ===
    public void ActivateShield()
    {
        if (isShieldActive)
        {
            Debug.Log("🛡️ 护盾已激活，无需重复激活");
            return;
        }

        isShieldActive = true;
        shieldTimer = shieldDuration;
        SetShieldVisible(true);
        Debug.Log($"🛡️ 护盾激活！持续 {shieldDuration} 秒");
    }

    // === 停用护盾 ===
    public void DeactivateShield()
    {
        isShieldActive = false;
        SetShieldVisible(false);
        Debug.Log("🛡️ 护盾已消失");
    }

    // === 护盾阻挡撞击（由 RespawnSystem 调用） ===
    public bool TryBlockHit()
    {
        if (!isShieldActive)
        {
            return false;
        }

        // 挡住撞击，护盾立即消失
        Debug.Log("💥🛡️ 护盾挡住了撞击！");
        DeactivateShield();
        return true;
    }

    // === 显示/隐藏护盾 ===
    void SetShieldVisible(bool visible)
    {
        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = visible;
        }
    }

    // === 获取护盾状态 ===
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
}