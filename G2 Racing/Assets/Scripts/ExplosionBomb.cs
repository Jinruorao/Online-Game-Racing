using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ExplosionBomb : MonoBehaviourPunCallbacks
{
    [Header("=== Attack Parameters ===")]
    public float attackRange = 50f;
    public float attackAngle = 60f;
    public float stunDuration = 2f;

    [Header("=== Effect ===")]
    public GameObject flameEffectPrefab;

    [Header("=== Cooldown ===")]
    public float cooldownDuration = 5f;

    private float currentCooldown = 0f;
    private bool isOnCooldown = false;

    private PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!pv.IsMine) return;

        // 更新冷却计时
        if (isOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                isOnCooldown = false;
                currentCooldown = 0f;
                Debug.Log("✅ 技能冷却结束！");
            }
        }

        // 按 Q 攻击
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isOnCooldown)
            {
                Debug.Log($"⏳ 技能冷却中... 剩余 {currentCooldown:F1} 秒");
                return;
            }
            AttackNearestEnemy();
        }
    }

    void AttackNearestEnemy()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        Transform nearestEnemy = null;
        float nearestAngle = Mathf.Infinity;

        foreach (GameObject player in allPlayers)
        {
            if (player.transform == transform) continue;

            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (angle > attackAngle) continue;
            if (distance > attackRange) continue;

            if (angle < nearestAngle)
            {
                nearestAngle = angle;
                nearestEnemy = player.transform;
            }
        }

        if (nearestEnemy != null)
        {
            StartCooldown();

            PhotonView targetPV = nearestEnemy.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                // 通过网络 RPC 在所有客户端上执行攻击效果
                pv.RPC("RPC_AttackTarget", RpcTarget.All, targetPV.ViewID);
            }
        }
        else
        {
            Debug.Log("❌ No enemy in front!");
        }
    }

    [PunRPC]
    void RPC_AttackTarget(int targetViewID)
    {
        PhotonView targetPV = PhotonView.Find(targetViewID);
        if (targetPV == null) return;

        GameObject target = targetPV.gameObject;

        // 显示火焰特效（每个客户端本地生成，不需要网络同步）
        if (flameEffectPrefab != null)
        {
            Vector3 spawnPos = target.transform.position + Vector3.up * 2f;
            GameObject flame = Instantiate(flameEffectPrefab, spawnPos, Quaternion.identity);
            flame.transform.SetParent(target.transform);
            flame.SetActive(true);
            Destroy(flame, 3f);
        }

        // 禁用移动
        MovementController movement = target.GetComponent<MovementController>();
        if (movement != null) movement.enabled = false;

        CarMovement carMove = target.GetComponent<CarMovement>();
        if (carMove != null) carMove.enabled = false;

        // 停止物理
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 触发重生
        RespawnSystem respawn = target.GetComponent<RespawnSystem>();
        if (respawn != null && !respawn.isRespawning)
        {
            respawn.TriggerRespawn();
        }

        Debug.Log($"💥 Attacked {target.name} via network!");
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = cooldownDuration;
        Debug.Log($"⏳ 技能进入冷却... {cooldownDuration} 秒");
    }

    public float GetCooldownRemaining()
    {
        return currentCooldown;
    }

    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Vector3 forward = transform.forward * attackRange;
        Vector3 left = Quaternion.Euler(0, -attackAngle, 0) * forward;
        Vector3 right = Quaternion.Euler(0, attackAngle, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
    }
}
