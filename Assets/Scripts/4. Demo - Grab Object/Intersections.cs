using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Intersections
{
    //
    // Ray-sphere intersection
    //

    //https://www.lighthouse3d.com/tutorials/maths/ray-sphere-intersection/
    public static bool IsRayHittingSphere(Ray ray, Vector3 sphereCenter, float radius, out float hitDistance)
    {
        Vector3 p = ray.origin;
        Vector3 c = sphereCenter;
        float r = radius;

        //This is the vector from p to c
        Vector3 vpc = c - p;

        //Assume the ray starts outside the sphere
        //The closest point on the ray from the sphere center
        Vector3 pc = ClosestPointOnRay(c, ray);

        //There is no intersection if the distance between the center of the sphere and the closest point on the ray is larger than the radius of the sphere
        if ((pc - c).sqrMagnitude > r * r)
        {
            hitDistance = 0f;
            return false;
        }

        //Distance from pc to i1 (intersection point 1) by using the triangle pc - c - i1
        float dist_i1_pc = Mathf.Sqrt(Mathf.Pow(radius, 2f) - Mathf.Pow((pc - c).magnitude, 2f));

        //The distance to the first intersection point (there are two because the ray is also exiting the sphere) from the start of the ray
        //But we don't care about exiting the sphere becase that intersection point is further away
        float dist_i1 = 0f;

        if (vpc.sqrMagnitude > r * r) //Ray start is outside sphere
            dist_i1 = (pc - p).magnitude - dist_i1_pc;
        else //Ray start is inside sphere
            dist_i1 = (pc - p).magnitude + dist_i1_pc;

        hitDistance = dist_i1;
        return true;
    }

    //
    // The closest point on a ray from a vertex
    //

    //https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/closest_point_on_ray.html
    private static Vector3 ClosestPointOnRay(Vector3 p, Ray ray)
    {
        Vector3 a = ray.origin;
        Vector3 b = ray.origin + ray.direction;
        Vector3 ab = b - a;

        //Find the closest point from p to the line segment a-b
        float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);

        //Clamp t to not be behind the ray
        t = Mathf.Max(0f, t);

        //Find the coordinate of this point
        Vector3 c = a + t * ab;

        return c;
    }
}
