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
    
    private const float gravity = 20f;
    private const float maxDistance = 100f;
    private const float maxRicochetDistance = 50f;
    private const float minReflectedVelocity = 1.5f;
    private const int wallLayer = 7;

    private Coroutine movementCoroutine;
    private RaycastHit raycastHit;

    public void Release(float velocityInit, Transform tipDefault, Transform tip)
    {
        GenerateRandomMesh();
        movementCoroutine = StartCoroutine(ReleaseLoop(velocityInit, tipDefault, tip));
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

    private IEnumerator ReleaseLoop(float velocityInit, Transform tipDefault, Transform tip)
    {
        float distance = 0f;
        float shotAngle = Vector3.Angle(tipDefault.forward, tip.forward) * Mathf.Deg2Rad;

        Vector3 tipOffset = tip.position - tipDefault.position;
        Vector3 tipDefaultPos = tipDefault.position;
        Vector3 tipDefaultFwd = tipDefault.forward;

        while (distance < maxDistance)
        {
            float step = velocityInit * Time.fixedDeltaTime;

            distance += step;
            float height = Trajectory.GetHeightOverDistance(distance, shotAngle, velocityInit);
            Vector3 point = tipDefaultPos + tipDefaultFwd * distance + tipOffset;
            point.y += height;
            transform.position = point;

            float predictedDist = distance + step;
            float predictedHeight = Trajectory.GetHeightOverDistance(predictedDist, shotAngle, velocityInit);
            Vector3 predictedPoint = tipDefaultPos + tipDefaultFwd * predictedDist + tipOffset;
            predictedPoint.y += predictedHeight;

            if (PredictCollision(predictedPoint, step))
                Ricochet(velocityInit);

            yield return new WaitForFixedUpdate();
        }

        CustomDestruction();
    }

    private bool PredictCollision(Vector3 nextPoint, float step)
    {
        Vector3 movementDir = (nextPoint - transform.position).normalized;
        Vector3 bottomPoint = new Vector3(nextPoint.x, nextPoint.y - meshRenderer.bounds.extents.y, nextPoint.z);
        float heightChange = Mathf.Abs(transform.position.y - nextPoint.y);

        if (Physics.Raycast(nextPoint - movementDir * meshRenderer.bounds.extents.z, movementDir, out raycastHit, step, collisionLayerMask))
            return true;
        else if (Physics.Raycast(bottomPoint, Vector3.down, out raycastHit, heightChange, collisionLayerMask))
            return true;
        else
            return false;
    }

    private void Ricochet(float velocityInit)
    {
        ResetMovement();
        movementCoroutine = StartCoroutine(RicochetLoop(velocityInit));
    }

    private IEnumerator RicochetLoop(float velocityInit)
    {
        Vector3 collisionDirNormalized = (transform.position - raycastHit.point).normalized;
        Vector3 projectionOverNormal = raycastHit.normal * Vector3.Dot(raycastHit.normal, collisionDirNormalized);
        Vector3 reflectionBuildPoint = raycastHit.point + collisionDirNormalized + (projectionOverNormal - collisionDirNormalized) * 2;
        Vector3 reflectedDir = reflectionBuildPoint - raycastHit.point;

        float velocityMultiplayer = Mathf.Abs(Vector3.Dot(collisionDirNormalized, reflectedDir));
        float reflectedVelocity = velocityInit * (1 - velocityMultiplayer);

        yield return new WaitForFixedUpdate();
        transform.position = raycastHit.point + collisionDirNormalized * (meshRenderer.bounds.extents.z + 0.1f);
        yield return new WaitForFixedUpdate();

        if (reflectedVelocity < minReflectedVelocity)
            CustomDestruction();

        float distance = 0f;
        float downForce = 0f;
        while (distance < maxRicochetDistance)
        {
            float step = reflectedVelocity * Time.fixedDeltaTime;
            distance += step;

            Vector3 position = transform.position + reflectedDir * step + Vector3.down * (downForce * Time.fixedDeltaTime);
            transform.position = position;

            Vector3 predictedPosition = transform.position + reflectedDir * step + Vector3.down * (downForce * Time.fixedDeltaTime);

            if (PredictCollision(predictedPosition, step))
                CustomDestruction();


            if (downForce < gravity)
                downForce += velocityInit * Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        CustomDestruction();
    }

    private void CustomDestruction()
    {
        ResetMovement();
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

    private void ResetMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }
}