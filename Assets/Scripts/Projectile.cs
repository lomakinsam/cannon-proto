using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private const float maxDistance = 100f;

    private Coroutine movementCoroutine;

    public void Release(float velocityInit, Transform tipDefault, Transform tip) => movementCoroutine = StartCoroutine(_Release(velocityInit, tipDefault, tip));

    private IEnumerator _Release(float velocityInit, Transform tipDefault, Transform tip)
    {
        float distance = 0f;
        float shotAngle = Vector3.Angle(tipDefault.forward, tip.forward) * Mathf.Deg2Rad;

        Vector3 tipOffset = tip.position - tipDefault.position;
        Vector3 tipDefaultPos = tipDefault.position;
        Vector3 tipDefaultFwd = tipDefault.forward;

        while (distance < maxDistance)
        {
            distance += velocityInit * Time.fixedDeltaTime;

            float height = Trajectory.GetHeightOverDistance(distance, shotAngle, velocityInit);

            Vector3 point = tipDefaultPos + tipDefaultFwd * distance + tipOffset;
            point.y += height;

            transform.position = point;

            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }
}