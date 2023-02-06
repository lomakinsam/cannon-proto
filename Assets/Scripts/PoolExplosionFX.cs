using System.Collections.Generic;
using UnityEngine;

public class PoolExplosionFX : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem explosionPrefab;

    public static PoolExplosionFX Instance;

    private List<ParticleSystem> pool;

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

    public ParticleSystem GetExplosionFX()
    {
        if (pool == null)
            pool = new();

        ParticleSystem selectedExplosion = null;

        foreach (var explosion in pool)
        {
            if (!explosion.IsAlive(true))
            {
                selectedExplosion = explosion;
                break;
            }
        }

        if (selectedExplosion == null)
        {
            selectedExplosion = Instantiate(explosionPrefab);
            selectedExplosion.transform.SetParent(transform, true);
            pool.Add(selectedExplosion);
        }

        return selectedExplosion;
    }
}