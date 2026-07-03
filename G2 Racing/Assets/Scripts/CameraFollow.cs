using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随参数")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 5f, -10f);  // 相对飞船的偏移（后方10米，上方5米）

    [Header("平滑参数")]
    public float positionSmoothSpeed = 0.15f;
    public float rotationSmoothSpeed = 0.12f;

    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        // === 1. 计算目标位置 ===
        // 获取飞船的Y轴旋转（忽略翻滚和俯仰）
        Quaternion targetYawRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

        // 偏移量只应用Y轴旋转，这样摄像机始终在飞船屁股后面
        Vector3 targetPosition = target.position + targetYawRotation * offset;

        // 平滑移动到目标位置
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothSpeed
        );

        // === 2. 旋转跟随（只跟随Y轴） ===
        float targetYaw = target.eulerAngles.y;

        float smoothYaw = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetYaw,
            ref rotationVelocity,
            rotationSmoothSpeed
        );

        // 摄像机保持水平，只改变Y轴旋转
        transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
    }
}