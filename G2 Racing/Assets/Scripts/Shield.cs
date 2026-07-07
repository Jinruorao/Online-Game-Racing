using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("=== 护盾参数 ===")]
    public float shieldDuration = 5f;

    [Header("=== 引用 ===")]
    public RespawnSystem respawnSystem;

    private bool isShieldActive = false;
    private float shieldTimer = 0f;

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

        SetShieldVisible(false);
    }

    void Update()
    {
        if (isShieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
                Debug.Log("⏰ 护盾过期自动消失");
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivateShield();
        }
    }

    public void ActivateShield()
    {
        if (isShieldActive)
        {
            Debug.Log("🛡️ 护盾已激活，无需重复激活");
            return;
        }

        // ★ 重置计时器
        isShieldActive = true;
        shieldTimer = shieldDuration;
        SetShieldVisible(true);
        Debug.Log($"🛡️ 护盾激活！持续 {shieldDuration} 秒");
    }

    public void DeactivateShield()
    {
        // ★ 彻底清理状态
        isShieldActive = false;
        shieldTimer = 0f;           
        SetShieldVisible(false);
        Debug.Log("🛡️ 护盾已消失");
    }

    public bool TryBlockHit()
    {
        if (!isShieldActive)
        {
            return false;
        }

        Debug.Log("💥🛡️ 护盾挡住了撞击！");
        DeactivateShield();         
        return true;
    }

    void SetShieldVisible(bool visible)
    {
        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = visible;
        }
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }
}