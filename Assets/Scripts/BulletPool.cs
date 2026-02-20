// BulletPool.cs
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int poolSize = 50; // Pre-create 50 bullets
    private List<PooledBullet> pool;

    void Start()
    {
        // Initialize the pool
        pool = new List<PooledBullet>();

        // Pre-create all bullets
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewBullet();
        }
        Debug.Log($"Pool initialized with {poolSize} bullets");
    }

    PooledBullet CreateNewBullet()
    {
        GameObject obj = Instantiate(bulletPrefab);
        PooledBullet bullet = obj.GetComponent<PooledBullet>();
        obj.SetActive(false);
        pool.Add(bullet);
        return bullet;
    }

    public PooledBullet GetBullet(Vector3 position, Quaternion rotation)
    {
        // Find an inactive bullet
        foreach (var bullet in pool)
        {
            if (!bullet.gameObject.activeSelf)
            {
                bullet.transform.position = position;
                bullet.transform.rotation = rotation;
                bullet.Initialize(this);
                return bullet;
            }
        }

        // Pool exhausted - create new bullet (with warning)
        Debug.LogWarning("Pool exhausted! Creating new bullet.");
        PooledBullet newBullet = CreateNewBullet();
        newBullet.transform.position = position;
        newBullet.transform.rotation = rotation;
        newBullet.Initialize(this);
        return newBullet;
    }

    public void ReturnBullet(PooledBullet bullet)
    {
        // Bullet is already deactivated by PooledBullet script
    }
}
