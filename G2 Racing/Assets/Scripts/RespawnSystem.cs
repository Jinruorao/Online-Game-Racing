using UnityEngine;
using System.Collections.Generic;

public class RespawnSystem : MonoBehaviour
{
    [Header("=== 回溯参数 ===")]
    public float historyDuration = 3f;
    public float respawnBlinkDuration = 2f;
    public float blinkInterval = 0.1f;
    public float recordInterval = 0.1f;

    [Header("=== 引用 ===")]
    public MovementController movementController;
    public Rigidbody rb;
    public Shield shield;

    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private float historyTimer = 0f;

    public bool isRespawning = false;
    private float respawnTimer = 0f;
    private float blinkTimer = 0f;
    private bool isVisible = true;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (movementController == null) movementController = GetComponent<MovementController>();

        positionHistory = new Queue<Vector3>();
        historyTimer = 0f;

        
        positionHistory.Enqueue(transform.position);
        Debug.Log($"RespawnSystem 启动，初始位置：{transform.position}");

        if (shield == null)//获取保护罩
        {
            shield = GetComponentInChildren<Shield>();
        }
    }

    void Update()
    {
        if (!isRespawning)
        {
            RecordPositionHistory();
        }

        if (isRespawning)
        {
            UpdateRespawn();
        }
    }

    void FixedUpdate()
    {
        if (isRespawning)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void RecordPositionHistory()
    {
        historyTimer += Time.deltaTime;

        if (historyTimer >= recordInterval)
        {
            positionHistory.Enqueue(transform.position);
            historyTimer = 0f;

            int maxCount = Mathf.FloorToInt(historyDuration / recordInterval);
            while (positionHistory.Count > maxCount)
            {
                positionHistory.Dequeue();
            }

            // ★ 调试：每30帧输出一次历史数量
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"📊 历史记录数：{positionHistory.Count}");
            }
        }
    }

    void UpdateRespawn()
    {
        respawnTimer -= Time.deltaTime;
        blinkTimer -= Time.deltaTime;

        if (blinkTimer <= 0f)
        {
            isVisible = !isVisible;
            blinkTimer = blinkInterval;
            SetShipVisibility(isVisible);
        }

        if (respawnTimer <= 0f)
        {
            isRespawning = false;
            isVisible = true;
            SetShipVisibility(true);

            if (movementController != null)
            {
                movementController.enabled = true;
            }

            Debug.Log("✅ 回溯结束，恢复控制");
        }
    }

    public void TriggerRespawn()
    {
        if (isRespawning) return;

        // ★ 关键修复：获取3秒前的位置
        Vector3 targetPosition = GetPositionAtTime(historyDuration);

        Debug.Log($"🔍 获取历史位置... 历史总数：{positionHistory.Count}");

        if (targetPosition == Vector3.zero && positionHistory.Count > 0)
        {
            // ★ 如果获取失败，取最早的位置（最老的那个）
            Vector3[] historyArray = positionHistory.ToArray();
            targetPosition = historyArray[0];
            Debug.Log($"⚠️ 使用最早位置：{targetPosition}");
        }

        if (targetPosition == Vector3.zero)
        {
            // 没有任何历史，回到当前位置后方
            targetPosition = transform.position - transform.forward * 5f;
            targetPosition.y = transform.position.y;
            Debug.Log($"⚠️ 没有历史，使用后方位置：{targetPosition}");
        }

        Debug.Log($"📍 回溯目标位置：{targetPosition}");

        isRespawning = true;
        respawnTimer = respawnBlinkDuration;
        blinkTimer = 0f;
        isVisible = true;

        if (movementController != null)
        {
            movementController.enabled = false;
        }

        // ★ 使用 MovePosition 并确保位置正确
        rb.MovePosition(targetPosition);
        rb.position = targetPosition;  // 直接设置确保生效
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 重置速度倍率
        if (movementController != null)
        {
            movementController.ResetSpeedMultiplier();
        }

        Debug.Log($"💥 触发回溯！回到 {historyDuration} 秒前的位置");
    }

    Vector3 GetPositionAtTime(float secondsAgo)
    {
        if (positionHistory.Count == 0)
        {
            Debug.LogWarning("⚠️ 历史记录为空！");
            return Vector3.zero;
        }

        // ★ 计算目标索引：从最新往前数
        int targetIndex = Mathf.FloorToInt(secondsAgo / recordInterval);

        // 确保不超出范围
        targetIndex = Mathf.Min(targetIndex, positionHistory.Count - 1);

        Vector3[] historyArray = positionHistory.ToArray();
        // 索引0是最老的，索引-1是最新的
        int index = historyArray.Length - targetIndex - 1;
        index = Mathf.Max(0, index);

        Vector3 result = historyArray[index];
        Debug.Log($"📌 获取位置：索引 {index} / 总数 {historyArray.Length}，位置 {result}");

        return result;
    }

    void SetShipVisibility(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"💥 碰撞检测到：{collision.gameObject.name}，Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Obstacle") && !isRespawning)
        {
            if (shield != null && shield.TryBlockHit())
            {
                Debug.Log("🛡️ 护盾挡住了撞击！飞船安全！");
                return;  // 护盾挡住，不触发回溯
            }

            TriggerRespawn();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (positionHistory != null && positionHistory.Count > 0)
        {
            Gizmos.color = Color.cyan;
            Vector3[] historyArray = positionHistory.ToArray();
            for (int i = 0; i < historyArray.Length; i++)
            {
                Gizmos.DrawSphere(historyArray[i], 0.3f);
            }
        }
    }
}