using UnityEngine;

public class CameraFollow : MonoBehaviour
{


    [Header("跟随参数")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 5f, -10f);  // ��Էɴ���ƫ�ƣ���10�ף��Ϸ�5�ף�

    [Header("ƽ平滑参数)]
    public float positionSmoothSpeed = 0.15f;
    public float rotationSmoothSpeed = 0.12f;

    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity = 0f;


    void LateUpdate()
    {
        if (target == null) return;





        Quaternion targetYawRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

        Vector3 targetPosition = target.position + targetYawRotation * offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothSpeed
        );


        float targetYaw = target.eulerAngles.y;

        float smoothYaw = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetYaw,
            ref rotationVelocity,
            rotationSmoothSpeed
        );


        transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
    }
}
>>>>>>> origin/JIN10086
