using UnityEngine;

public class ExplosionBomb: MonoBehaviour
{
    [Header("=== Attack Parameters ===")]
    public float attackRange = 50f;
    public float attackAngle = 60f;
    public float stunDuration = 2f;

    [Header("=== Effect ===")]
    public GameObject flameEffectPrefab;

    [Header("=== Cooldown ===")]
    public float cooldownDuration = 5f;         // 冷却时间（秒）

    private float currentCooldown = 0f;          // 当前冷却剩余时间
    private bool isOnCooldown = false;           // 是否冷却中

    void Update()
    {
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

        // 按 Q 攻击（只有不在冷却中才能攻击）
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

        // ★ 只有打中才显示特效
        if (nearestEnemy != null)
        {
            // ★ 进入冷却
            StartCooldown();

            // 显示火焰特效
            if (flameEffectPrefab != null)
            {
                Vector3 spawnPos = nearestEnemy.position + Vector3.up * 2f;
                GameObject flame = Instantiate(flameEffectPrefab, spawnPos, Quaternion.identity);
                flame.transform.SetParent(nearestEnemy);

                // ★ 不控制播放，只控制显示
                flame.SetActive(true);

                // 3秒后销毁
                Destroy(flame, 3f);
            }

            // 禁用移动
            MovementController movement = nearestEnemy.GetComponent<MovementController>();
            if (movement != null)
            {
                movement.enabled = false;
            }

            // 停止物理
            Rigidbody rb = nearestEnemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 触发回溯
            RespawnSystem respawn = nearestEnemy.GetComponent<RespawnSystem>();
            if (respawn != null && !respawn.isRespawning)
            {
                respawn.TriggerRespawn();
            }

            Debug.Log($"💥 Attacked {nearestEnemy.name}!");
        }
        else
        {
            // ★ 没打中只输出日志，不显示特效
            Debug.Log("❌ No enemy in front!");
        }
    }

    // 开始冷却
    void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = cooldownDuration;
        Debug.Log($"⏳ 技能进入冷却... {cooldownDuration} 秒");
    }

    // 获取冷却剩余时间（供外部UI显示）
    public float GetCooldownRemaining()
    {
        return currentCooldown;
    }

    // 是否冷却中
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