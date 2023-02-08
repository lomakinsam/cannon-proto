using System.Collections.Generic;
using UnityEngine;

public static class Trajectory
{
    private const float gravity = 9.8f;
    private const float drag = 0.5f;
    private const float simulatedPathMaxLength = 100f;
    private const float simulationSpeed = 1f;
    private const float downForceConstant = 0.8f;
    private static List<Vector3> pathPoints;

    public static Vector3[] SimulatePath(Vector3 position, Vector3 velocity)
    {
        if (pathPoints == null)
            pathPoints = new();

        if (pathPoints.Count > 0)
            pathPoints.Clear();

        pathPoints.Add(position);

        float pathLengthSqr = 0f;
        float downForce = 0f;
        Vector3 newPos = position;

        while (pathLengthSqr < simulatedPathMaxLength * simulatedPathMaxLength)
        {
            newPos = SimulateStep(newPos, ref velocity, ref downForce);
            pathPoints.Add(newPos);

            pathLengthSqr = (newPos - position).sqrMagnitude;
        }

        return pathPoints.ToArray();
    }

    public static Vector3 SimulateStep(Vector3 position, ref Vector3 velocity, ref float downForce)
    {
        float step = Time.fixedDeltaTime * simulationSpeed;

        Vector3 resultingVelocity = GetResultingVelocity(velocity, downForce);
        position += resultingVelocity * step;

        velocity *= 1 - (drag * step);

        downForce = Mathf.Clamp(downForce + gravity * step, 0, gravity);

        return position;
    }

    public static Vector3 GetResultingVelocity(Vector3 velocity, float downForce) => velocity + (Vector3.down * downForce * downForce * downForceConstant);
}