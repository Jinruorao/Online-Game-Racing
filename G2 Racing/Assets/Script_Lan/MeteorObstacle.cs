using UnityEngine;
using System.Collections;

public class MeteorObstacle : MonoBehaviour
{
    private AudioSource audioSource;
    private Renderer[] renderers;
    private Collider col;

    void Start()
    {

        renderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
            // PlayerMovement player = other.GetComponent<PlayerMovement>();

            // if (player != null)
            // {
            //     player.StartCoroutine(player.HitObstacle());
            //  }

            StartCoroutine(BlinkAndDestroy());
        }
    }

    IEnumerator BlinkAndDestroy()
    {
        col.enabled = false;

        for (int i = 0; i < 6; i++)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = !r.enabled;
            }

            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}