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

    [Header("Config")]
    [SerializeField]
    private Vector2 horizontalRotationRange = new Vector2(-20f, 20f);
    [SerializeField]
    private Vector2 verticalRotationRange = new Vector2(45f, 90f);

    private const float horizontalRotationDefautAngle = 0;
    private const float verticalRotationDefaultAngle = 90f;

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
}