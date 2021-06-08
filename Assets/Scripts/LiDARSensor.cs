using UnityEngine;

public struct LiDARSensor
{
    public Vector3 direction;
    public float distance;
    public Color color;
    public float currentDistance;

    public LiDARSensor(Vector3 direction, float distance, Color color)
    {
        this.direction = direction;
        this.distance = distance;
        this.color = color;
        currentDistance = 0;
    }

    public float ShootRay(Transform transform)
    {
        Debug.DrawRay(transform.position, transform.TransformDirection(direction.normalized) * distance, color);
        if (Physics.Raycast(transform.position, transform.TransformDirection(direction.normalized), out RaycastHit objectHit, distance))
        {
            currentDistance = Vector3.Distance(transform.position, objectHit.point);
            return currentDistance;
        }
        currentDistance = distance;
        return currentDistance;
    }
}