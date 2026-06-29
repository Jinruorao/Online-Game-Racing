using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随参数")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 5f, -10f);

    [Header("平滑参数")]
    public float smoothSpeed = 0.15f;  // 只控制位置平滑

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 计算目标位置（跟随飞船位置和旋转）
        Vector3 targetPosition = target.position + target.rotation * offset;

        // 2. 平滑移动到目标位置（只平滑位置）
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothSpeed
        );

        // 3. 直接看向飞船（不旋转平滑，避免抖动）
        transform.LookAt(target);
    }
}