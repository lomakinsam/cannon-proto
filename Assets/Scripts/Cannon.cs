using System;
using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private Transform tip;
    [SerializeField]
    private Transform barrel;
    [SerializeField]
    private LineRenderer trajectoryLineRenderer;
    [SerializeField]
    private CameraShaker cameraShaker;

    [Header("Rotation Limits")]
    [SerializeField]
    private Vector2 horizontalRotationRange = new Vector2(-20f, 20f);
    [SerializeField]
    private Vector2 verticalRotationRange = new Vector2(45f, 90f);

    [Header("Shot Config")]
    [SerializeField] [Range(0f, maxShotPower)]
    private float shotPower = 10f;

    public const float maxShotPower = 100f;

    private const float horizontalRotationDefautAngle = 0;
    private const float verticalRotationDefaultAngle = 90f;

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
        if(shotPower < 1)
        {
            trajectoryLineRenderer.positionCount = 0;
            return;
        }

        Vector3 velocity = tip.forward * shotPower;
        Vector3[] drawPoints = Trajectory.SimulatePath(tip.position, velocity);

        if (drawPoints.Length == 0)
        {
            trajectoryLineRenderer.positionCount = 0;
            return;
        }

        trajectoryLineRenderer.positionCount = drawPoints.Length;
        trajectoryLineRenderer.SetPositions(drawPoints);
    }

    public void Shoot()
    {
        if (shotPower < 1) return;

        PlayShootAnimation();
        cameraShaker.Shake();

        Projectile projectile = PoolProjectile.Instance.GetProjectile();
        projectile.transform.position = tip.position;
        projectile.SimulateForce(tip.forward * shotPower);
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