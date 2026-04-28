using UnityEngine;

public class CustomHit
{
    public readonly float distance;
    public Vector3 location;
    public Vector3 normal;
    public int index;

    public CustomHit(float distance, Vector3 location, Vector3 normal, int index = -1)
    {
        this.distance = distance;
        this.location = location;
        this.normal = normal;
        this.index = index;
    }
}
