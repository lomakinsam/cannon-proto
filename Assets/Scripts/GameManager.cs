using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Cannon Setup")]
    [SerializeField]
    private Cannon cannon;
    [SerializeField]
    private float verticalRotationSpeed = 5f;
    [SerializeField]
    private float horizontalRotationSpeed = 10f;
    [SerializeField]
    private float shotPowerIncrement = 10f;

    private void Update()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");
        float scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");

        if (verticalAxis != 0)
            cannon.RotateVertically(verticalAxis * verticalRotationSpeed * Time.deltaTime);

        if (horizontalAxis != 0)
            cannon.RotateHorizontally(horizontalAxis * horizontalRotationSpeed * Time.deltaTime);

        if (verticalAxis != 0 || horizontalAxis != 0)
            cannon.DrawTrajectory();

        if (scrollWheelAxis != 0)
            cannon.ShotPower += shotPowerIncrement * scrollWheelAxis;

        if (Input.GetKeyDown(KeyCode.Space))
            cannon.Shoot();
    }
}