using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private GameObject hitDecalPrefab;
    [SerializeField]
    private LayerMask collisionLayerMask;
    
    private const float maxDistance = 100f;
    private const float maxRicochetDistance = 50f;
    private const float verticalBounceForce = 10f;
    private const float collisionSimularityThreshold = 0.95f;
    private const int wallLayer = 7;
    private const int collisionsLimit = 1;
    private Coroutine simulation;
    private RaycastHit raycastHit;

    public void SimulateForce(Vector3 force)
    {
        if (simulation != null)
        {
            StopCoroutine(simulation);
            simulation = null;
        }

        simulation = StartCoroutine(Simulate(force));
        GenerateRandomMesh();
    }

    private IEnumerator Simulate(Vector3 velocity)
    {
        int collisionsCount = 0;
        float downForce = 0f;
        float distanceSqr = 0f;
        float distanceLimit = maxDistance;
        Vector3 startPoint = transform.position;

        while (distanceSqr < distanceLimit * distanceLimit && collisionsCount <= collisionsLimit)
        {
            if (DetectCollision(velocity))
            {
                if (RecalculateVelocity(ref velocity, ref downForce))
                    break;

                collisionsCount++;
                distanceLimit = maxRicochetDistance;
                startPoint = raycastHit.point;

                if (collisionsCount > collisionsLimit)
                    break;
            }

            transform.position = Trajectory.SimulateStep(transform.position, ref velocity, ref downForce);
            distanceSqr = (transform.position - startPoint).sqrMagnitude;

            yield return new WaitForFixedUpdate();
        }

        CustomDestruction();
    }

    private bool RecalculateVelocity(ref Vector3 velocity, ref float downForce)
    {
        Vector3 resultingVelocityDirection = Trajectory.GetResultingVelocity(velocity, downForce).normalized;
        Vector3 reflectedDirection = Vector3.Reflect(resultingVelocityDirection, raycastHit.normal);

        float collisionDirectionSimilarity = Mathf.Abs(Vector3.Dot(reflectedDirection, raycastHit.normal));

        downForce *= 1 - collisionDirectionSimilarity;
        velocity = (reflectedDirection * velocity.magnitude + Vector3.up * verticalBounceForce) * (1 - collisionDirectionSimilarity);

        bool directHit = collisionDirectionSimilarity > collisionSimularityThreshold;
        return directHit;
    }

    private bool DetectCollision(Vector3 velocity)
    {
        if (Physics.Raycast(transform.position, velocity.normalized, out raycastHit, meshRenderer.bounds.size.z, collisionLayerMask))
            return true;
        else if (Physics.Raycast(transform.position, Vector3.down, out raycastHit, meshRenderer.bounds.size.y, collisionLayerMask))
            return true;
        else
            return false;
    }

    private void GenerateRandomMesh()
    {
        if (meshFilter.mesh.vertexCount == 0)
            meshFilter.mesh = ProjectileMeshGenerator.GenerateMesh();
        else
        {
            Mesh updatedMesh = meshFilter.mesh;
            ProjectileMeshGenerator.RegenerateVertexNoise(ref updatedMesh);
            meshFilter.mesh = updatedMesh;
        }
    }

    private void CustomDestruction()
    {
        CreateHitDecal();
        CreateExplosion();
        gameObject.SetActive(false);
    }

    private void CreateHitDecal()
    {
        if (raycastHit.transform == null) return;

        if (raycastHit.transform.gameObject.layer == wallLayer)
        {
            GameObject decal = Instantiate(hitDecalPrefab);
            decal.transform.position = raycastHit.point + raycastHit.normal * 0.01f;
            decal.transform.forward = raycastHit.normal;
            decal.transform.SetParent(raycastHit.transform, true);
        }
    }

    private void CreateExplosion()
    {
        ParticleSystem explosionFX = PoolExplosionFX.Instance.GetExplosionFX();

        if (raycastHit.transform != null)
        {
            explosionFX.transform.position = raycastHit.point + raycastHit.normal;
            explosionFX.transform.forward = raycastHit.normal;
        }
        else
        {
            explosionFX.transform.position = transform.position;
            explosionFX.transform.forward = Vector3.up;
        }

        explosionFX.Play();
    }
}