using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private LayerMask collisionLayerMask;

    private const float maxDistance = 100f;
    private const float maxRicochetDistance = 100f;
    private const float gravity = 9.8f;

    private float velocity = 0f;
    private Coroutine movementCoroutine;

    private RaycastHit raycastHitForward;
    private RaycastHit raycastHitBottom;
    private RaycastHit raycastHitSelected;

    public void Release(float velocityInit, Transform tipDefault, Transform tip) => movementCoroutine = StartCoroutine(ReleaseLoop(velocityInit, tipDefault, tip));

    private IEnumerator ReleaseLoop(float velocityInit, Transform tipDefault, Transform tip)
    {
        velocity = velocityInit;

        float distance = 0f;
        float shotAngle = Vector3.Angle(tipDefault.forward, tip.forward) * Mathf.Deg2Rad;

        Vector3 tipOffset = tip.position - tipDefault.position;
        Vector3 tipDefaultPos = tipDefault.position;
        Vector3 tipDefaultFwd = tipDefault.forward;

        while (distance < maxDistance)
        {
            distance += velocity * Time.fixedDeltaTime;

            float height = Trajectory.GetHeightOverDistance(distance, shotAngle, velocity);

            Vector3 point = tipDefaultPos + tipDefaultFwd * distance + tipOffset;
            point.y += height;

            transform.position = point;

            if (DetectCollision(ref raycastHitSelected))
                Ricochet();

            yield return new WaitForFixedUpdate();
        }

        CustomDestruction();
    }

    private bool DetectCollision(ref RaycastHit selectedRaycastHit)
    {
        bool forwardCollision = Physics.BoxCast(transform.position, meshRenderer.bounds.extents, transform.forward, out raycastHitForward, Quaternion.identity, meshRenderer.bounds.size.z, collisionLayerMask);
        bool bottomCollision = Physics.BoxCast(transform.position, meshRenderer.bounds.extents, transform.TransformDirection(Vector3.down), out raycastHitForward, Quaternion.identity, meshRenderer.bounds.size.z, collisionLayerMask);

        if (forwardCollision)
        {
            selectedRaycastHit = raycastHitForward;
            return forwardCollision;
        }

        if (bottomCollision)
        {
            selectedRaycastHit = raycastHitBottom;
            return bottomCollision;
        }

        return false;        
    }

    private void Ricochet()
    {
        ResetMovement();
        movementCoroutine = StartCoroutine(RicochetLoop());
    }

    private IEnumerator RicochetLoop()
    {
        Vector3 collisionDirNormalized = (transform.position - raycastHitSelected.point).normalized;
        Vector3 projectionOverNormal = raycastHitSelected.normal * Vector3.Dot(raycastHitSelected.normal, collisionDirNormalized);
        Vector3 reflectionBuildPoint = raycastHitSelected.point + collisionDirNormalized + (projectionOverNormal - collisionDirNormalized) * 2;
        Vector3 reflectedDir = reflectionBuildPoint - raycastHitSelected.point;

        float velocityMultiplayer = Vector3.Dot(raycastHitSelected.normal, reflectedDir);

        if (velocityMultiplayer > 0.5f)
            CustomDestruction();

        velocity *= 1 - velocityMultiplayer;

        float distance = 0f;
        while(distance < maxRicochetDistance)
        {
            float step = velocity * Time.fixedDeltaTime;
            distance += step;

            Vector3 position = transform.position + reflectedDir * step;
            position.y -= gravity * Time.fixedDeltaTime;

            transform.position = position;

            if (DetectCollision(ref raycastHitSelected))
                CustomDestruction();

            yield return new WaitForFixedUpdate();
        }

        CustomDestruction();
    }

    private void CustomDestruction()
    {
        ResetMovement();

        Destroy(gameObject);
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