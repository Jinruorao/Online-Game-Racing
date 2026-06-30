using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随参数")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 5f, -10f);

    [Header("平滑参数")]
    public float smoothSpeed = 0.15f;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;


        Vector3 targetPosition = target.position + target.rotation * offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothSpeed
        );

        transform.LookAt(target);
    }
}
