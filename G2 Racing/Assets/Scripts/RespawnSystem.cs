using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class RespawnSystem : MonoBehaviourPunCallbacks
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

    private PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (pv == null) pv = GetComponentInParent<PhotonView>();

        if (rb == null) rb = GetComponent<Rigidbody>();
        if (movementController == null) movementController = GetComponent<MovementController>();

        positionHistory = new Queue<Vector3>();
        historyTimer = 0f;

        positionHistory.Enqueue(transform.position);
        Debug.Log($"RespawnSystem 启动，初始位置：{transform.position}");

        if (shield == null)
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
        // FixedUpdate 里已经有 null 判断
        if (isRespawning && pv != null && pv.IsMine)
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

    /// <summary>
    /// 公开方法：触发重生（本地 + 网络 RPC）
    /// </summary>
    public void TriggerRespawn()
    {
        if (isRespawning) return;

        Vector3 targetPosition = GetPositionAtTime(historyDuration);

        Debug.Log($"🔍 获取历史位置... 历史总数：{positionHistory.Count}");

        if (targetPosition == Vector3.zero && positionHistory.Count > 0)
        {
            Vector3[] historyArray = positionHistory.ToArray();
            targetPosition = historyArray[0];
            Debug.Log($"⚠️ 使用最早位置：{targetPosition}");
        }

        if (targetPosition == Vector3.zero)
        {
            targetPosition = transform.position - transform.forward * 5f;
            targetPosition.y = transform.position.y;
            Debug.Log($"⚠️ 没有历史，使用后方位置：{targetPosition}");
        }

        Debug.Log($"📍 回溯目标位置：{targetPosition}");

        if (pv != null)
        {
            pv.RPC("RPC_DoRespawn", RpcTarget.All, targetPosition);
        }
        else
        {
            ExecuteRespawn(targetPosition);
        }
    }

    [PunRPC]
    void RPC_DoRespawn(Vector3 targetPosition)
    {
        ExecuteRespawn(targetPosition);
    }

    void ExecuteRespawn(Vector3 targetPosition)
    {
        isRespawning = true;
        respawnTimer = respawnBlinkDuration;
        blinkTimer = 0f;
        isVisible = true;

        if (movementController != null)
        {
            movementController.enabled = false;
        }

        if (rb != null)
        {
            rb.MovePosition(targetPosition);
            rb.position = targetPosition;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = targetPosition;
        }

        if (movementController != null)
        {
            movementController.ResetSpeedMultiplier();
        }

        Debug.Log($"💥 触发回溯！回到位置 {targetPosition}");
    }

    Vector3 GetPositionAtTime(float secondsAgo)
    {
        if (positionHistory.Count == 0)
        {
            Debug.LogWarning("⚠️ 历史记录为空！");
            return Vector3.zero;
        }

        int targetIndex = Mathf.FloorToInt(secondsAgo / recordInterval);
        targetIndex = Mathf.Min(targetIndex, positionHistory.Count - 1);

        Vector3[] historyArray = positionHistory.ToArray();
        int index = historyArray.Length - targetIndex - 1;
        index = Mathf.Max(0, index);

        return historyArray[index];
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
        // ★ 修复：pv 可能为 null
        if (pv == null || !pv.IsMine) return;

        Debug.Log($"💥 碰撞检测到：{collision.gameObject.name}，Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Obstacle") && !isRespawning)
        {
            if (shield != null && shield.TryBlockHit())
            {
                Debug.Log("🛡️ 护盾挡住了撞击！飞船安全！");
                return;
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
