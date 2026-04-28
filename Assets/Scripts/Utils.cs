using UnityEngine;

public class Utils
{
    public static Vector3 PerpendicularXY(Vector3 v)
    {
        return new Vector3(-v.y, v.x, 0f);
    }
}
