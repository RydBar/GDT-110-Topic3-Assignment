// BulletSpawner.cs
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float spawnRate = 0.1f; // 10 bullets per second
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
        // Create new bullet every time - EXPENSIVE!
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        bulletsSpawned++;
        Debug.Log($"Spawned bullet #{bulletsSpawned}");
    }
}
