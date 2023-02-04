using UnityEngine;

public static class Trajectory
{
    private const float gravity = 9.8f;

    public static float GetHeightOverDistance(float distance, float shotAngle, float velocityInit)
    {
        float angleTan = Mathf.Tan(shotAngle);
        float angleCos = Mathf.Cos(shotAngle);

        float height = distance * angleTan - (gravity * distance * distance) / (2 * velocityInit * velocityInit * angleCos * angleCos);

        return height;
    }
}