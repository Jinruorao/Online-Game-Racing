using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float boostMultiplier = 1.5f;
    public float boostDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //PlayerMovement movement = other.GetComponent<PlayerMovement>();

           // if (movement != null)
            //{
                //movement.StartCoroutine(movement.SpeedBoost(boostMultiplier, boostDuration));
           // }
        }
    }
}