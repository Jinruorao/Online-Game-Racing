using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float floatHeight = 0.5f;
    public float floatSpeed = 1.5f;
    public float rotateSpeed = 30f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position = startPos + new Vector3(0, y, 0);

        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}