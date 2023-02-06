using System;
using System.Collections;
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
    [SerializeField]
    private CameraShaker cameraShaker;

    [Header("Rotation Limits")]
    [SerializeField]
    private Vector2 horizontalRotationRange = new Vector2(-20f, 20f);
    [SerializeField]
    private Vector2 verticalRotationRange = new Vector2(45f, 90f);

    [Header("Other")]
    [SerializeField] [Range(0f, maxShotPower)]
    private float shotPower = 10f;
    [SerializeField]
    private int trajectoryLineMaxLength = 100;

    public const float maxShotPower = 50f;

    private const float horizontalRotationDefautAngle = 0;
    private const float verticalRotationDefaultAngle = 90f;

    private const int lineRendererPointsPerUnit = 2;

    private float horizontalAngle = horizontalRotationDefautAngle;
    private float verticalAngle = verticalRotationDefaultAngle;

    private Coroutine shootAnimation;

    public event Action<float> OnShotPowerChange;
    public float ShotPower
    {
        get { return shotPower; }
        set
        {
            shotPower = Mathf.Clamp(value, 0f, maxShotPower);
            OnShotPowerChange?.Invoke(shotPower);
            DrawTrajectory();
        }
    }

    private void Awake() => DrawTrajectory();

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
        if (shotPower < 1)
        {
            trajectoryLineRenderer.positionCount = 0;
            return;
        }

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
        if (shotPower < 1) return;

        PlayShootAnimation();
        cameraShaker.Shake();
        
        Projectile projectile = Instantiate(projectilePrefab);
        projectile.Release(shotPower, tipDefault, tip);
    }

    private void PlayShootAnimation()
    {
        if (shootAnimation != null)
        {
            StopCoroutine(shootAnimation);
            shootAnimation = null;
        }

        shootAnimation = StartCoroutine(ShootAnimation());
    }

    private IEnumerator ShootAnimation()
    {
        float time = 0;
        float maxOffset = 5.5f;
        float animDuration = 0.25f;

        Vector3 defaultPos = new Vector3(0, 1.5f, 0);
        barrel.localPosition = defaultPos;

        while (time < animDuration)
        {
            float step = Mathf.PingPong(time, animDuration / 2);
            Vector3 moveAxis = barrel.TransformDirection(Vector3.down);
            barrel.localPosition = defaultPos + maxOffset * step * transform.InverseTransformDirection(moveAxis);

            time += Time.deltaTime;

            yield return null;
        }
    }
}