// PooledBulletSpawner.cs
using UnityEngine;

public class PooledBulletSpawner : MonoBehaviour
{
    public BulletPool pool;
    public float spawnRate = 0.1f;
    private float timer;
    private int bulletsSpawned = 0;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            SpawnBullet();
            timer = 0;
        }
    }

    void SpawnBullet()
    {
        // Get bullet from pool - FAST!
        PooledBullet bullet = pool.GetBullet(transform.position, transform.rotation);
        bulletsSpawned++;
        Debug.Log($"Spawned pooled bullet #{bulletsSpawned}");
    }
}
