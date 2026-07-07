using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float boostMultiplier = 1.5f;
    public float boostDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log($"💥 碰撞检测到：{other.gameObject.name}，Tag：{other.gameObject.tag}");
        if (other.CompareTag("Player"))
        {
            MovementController movement = other.GetComponent<MovementController>();
            if (movement != null)
            {
                Debug.Log($"🚀 加速前速度：{movement.forwardSpeed}");
                movement.forwardSpeed *= boostMultiplier;
                movement.Invoke("ResetSpeed", boostDuration);
            }
        }
    }
}