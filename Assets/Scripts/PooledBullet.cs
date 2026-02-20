// PooledBullet.cs - Modified bullet for pooling
using UnityEngine;

public class PooledBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    private float timer;
    private BulletPool pool; // Reference to the pool

    public void Initialize(BulletPool bulletPool)
    {
        pool = bulletPool;
        timer = 0;
        gameObject.SetActive(true);
    }

    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            ReturnToPool(); // Don't destroy - return to pool!
        }
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false); // Hide instead of destroy
        pool.ReturnBullet(this);
    }
}
