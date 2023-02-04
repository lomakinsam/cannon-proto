using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private Transform tip;
    [SerializeField]
    private Transform tipDefault;
    [SerializeField]
    private Transform barrel;
    [SerializeField]
    private LineRenderer trajectoryLineRenderer;
    [SerializeField]
    private Projectile projectilePrefab;

    [Header("Rotation Limits")]
    [SerializeField]
    private Vector2 horizontalRotationRange = new Vector2(-20f, 20f);
    [SerializeField]
    private Vector2 verticalRotationRange = new Vector2(45f, 90f);

    [Header("Other")]
    [SerializeField]
    private float shotPower = 10f;
    [SerializeField]
    private int trajectoryLineMaxLength = 100;

    private const float horizontalRotationDefautAngle = 0;
    private const float verticalRotationDefaultAngle = 90f;

    private const int lineRendererPointsPerUnit = 2;

    private float horizontalAngle = horizontalRotationDefautAngle;
    private float verticalAngle = verticalRotationDefaultAngle;

    private void OnDrawGizmos()
    {
        if (tip == null) return;

        float rayDist = 10f;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(tip.position, tip.forward * rayDist);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(tipDefault.position, tipDefault.forward * rayDist);

        Vector3 trajectoryPoint = tip.position + tip.forward * rayDist;
        float trajectoryProjection = Vector3.Dot(tipDefault.forward, trajectoryPoint - tip.position);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(tipDefault.position, tipDefault.forward * trajectoryProjection);
    }

    public void RotateVertically(float angle)
    {
        verticalAngle = Mathf.Clamp(verticalAngle - angle, verticalRotationRange.x, verticalRotationRange.y);
        barrel.localRotation = Quaternion.AngleAxis(verticalAngle, Vector3.right);
    }

    public void RotateHorizontally(float angle)
    {
        horizontalAngle = Mathf.Clamp(horizontalAngle + angle, horizontalRotationRange.x, horizontalRotationRange.y);
        transform.rotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
    }

    public void DrawTrajectory()
    {
        Vector3[] drawPoint = new Vector3[trajectoryLineMaxLength * lineRendererPointsPerUnit];

        if (drawPoint.Length == 0)
        {
            trajectoryLineRenderer.positionCount = 0;
            return;
        }

        drawPoint[0] = tip.position;

        float distance = 0f;
        float distanceStep = 1 / (float)lineRendererPointsPerUnit;
        float shotAngle = Vector3.Angle(tipDefault.forward, tip.forward) * Mathf.Deg2Rad;

        Vector3 tipOffset = tip.position - tipDefault.position;

        for (int i = 1; i < drawPoint.Length; i++)
        {
            distance += distanceStep;

            float height = Trajectory.GetHeightOverDistance(distance, shotAngle, shotPower);

            Vector3 point = tipDefault.position + tipDefault.forward * distance + tipOffset;
            point.y += height;

            drawPoint[i] = point;
        }

        trajectoryLineRenderer.positionCount = drawPoint.Length;
        trajectoryLineRenderer.SetPositions(drawPoint);
    }

    public void Shoot()
    {
        Projectile projectile = Instantiate(projectilePrefab, tip.position, tip.rotation);
        projectile.Release(shotPower, tipDefault, tip);
    }
}