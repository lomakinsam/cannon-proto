using System.Collections.Generic;
using UnityEngine;

public class PoolProjectile : MonoBehaviour
{
    [SerializeField]
    private Projectile projectilePrefab;

    public static PoolProjectile Instance;

    private List<Projectile> pool;

    private void Awake() => HandleSingleton();

    private void HandleSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Projectile GetProjectile()
    {
        if (pool == null)
            pool = new();

        Projectile selectedProjectile = null;

        foreach (var projectile in pool)
        {
            if (!projectile.gameObject.activeSelf)
            {
                selectedProjectile = projectile;
                selectedProjectile.gameObject.SetActive(true);
                break;
            }
        }

        if (selectedProjectile == null)
        {
            selectedProjectile = Instantiate(projectilePrefab);
            selectedProjectile.transform.SetParent(transform, true);
            pool.Add(selectedProjectile);
        }

        return selectedProjectile;
    }
}