// Unity C# Example - Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    private float timer;

    void Start()
    {
        timer = 0;
    }

    void Update()
    {
        // Move forward
        transform.position += transform.up * speed * Time.deltaTime;

        // Destroy after lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject); // ← This is expensive!
        }
    }
}
