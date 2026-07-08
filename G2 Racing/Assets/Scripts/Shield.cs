using UnityEngine;
using Photon.Pun;

public class Shield : MonoBehaviourPunCallbacks
{
    [Header("=== 护盾参数 ===")]
    public float shieldDuration = 5f;

    [Header("=== 引用 ===")]
    public RespawnSystem respawnSystem;

    private bool isShieldActive = false;
    private float shieldTimer = 0f;
    private Renderer shieldRenderer;
    private PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        // 兜底：往父级找
        if (pv == null) pv = GetComponentInParent<PhotonView>();

        shieldRenderer = GetComponent<Renderer>();

        if (respawnSystem == null)
        {
            respawnSystem = GetComponentInParent<RespawnSystem>();
        }

        SetShieldVisible(false);
    }

    void Update()
    {
        // ★ 关键修复：pv 可能为 null
        if (pv == null || !pv.IsMine) return;

        // 护盾计时（过期自动消失）
        if (isShieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                pv.RPC("RPC_DeactivateShield", RpcTarget.All);
                Debug.Log("⏰ 护盾过期自动消失");
            }
        }

        // 按 E 激活护盾
        if (Input.GetKeyDown(KeyCode.E))
        {
            pv.RPC("RPC_ActivateShield", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_ActivateShield()
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

    [PunRPC]
    void RPC_DeactivateShield()
    {
        isShieldActive = false;
        SetShieldVisible(false);
        Debug.Log("🛡️ 护盾已消失");
    }

    /// <summary>
    /// 护盾阻挡撞击（由 RespawnSystem 调用）
    /// </summary>
    public bool TryBlockHit()
    {
        if (!isShieldActive)
        {
            return false;
        }

        Debug.Log("💥🛡️ 护盾挡住了撞击！");
        if (pv != null && pv.IsMine)
        {
            pv.RPC("RPC_DeactivateShield", RpcTarget.All);
        }
        else
        {
            // 本地直接关闭视觉
            isShieldActive = false;
            SetShieldVisible(false);
        }
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
