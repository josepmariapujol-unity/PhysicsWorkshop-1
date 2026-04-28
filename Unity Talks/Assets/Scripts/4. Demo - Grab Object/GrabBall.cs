using UnityEngine;

public class GrabBall : IGrabbable
{
    public readonly float radius;
    public Vector3 pos;
    public Vector3 vel;
    private Transform ballTransform;

    private bool isGrabbed;

    public GrabBall(Transform ballTrans)
    {
        ballTransform = ballTrans;
        pos = ballTransform.position;
        radius = ballTransform.localScale.x * 0.5f;
    }

    // --------- Methods related to user interaction with the ball ---------
    public void StartGrab(Vector3 pos)
    {
        isGrabbed = true;
        this.pos = pos;
        vel = Vector3.zero;
    }

    public void MoveGrabbed(Vector3 pos)
    {
        this.pos = pos;
    }

    public void EndGrab(Vector3 pos, Vector3 vel)
    {
        isGrabbed = false;
        this.pos = pos;
        this.vel = vel;
    }

    public void IsRayHittingBody(Ray ray, out CustomHit hit)
    {
        hit = null;

        if (Intersections.IsRayHittingSphere(ray, pos, radius, out float hitDistance))
        {
            Vector3 hitPoint = ray.GetPoint(hitDistance);
            Vector3 normal = (hitPoint - pos).normalized;
            float depthAlongRay = Vector3.Dot(pos - ray.origin, ray.direction);
            hit = new CustomHit(depthAlongRay, pos, normal);
        }
    }

    public Vector3 GetGrabbedPos()
    {
        return pos;
    }

    // --------- Methods related to the simulation of the ball physics ---------
    public void SimulateBall(int subSteps, float dt, Vector3 gravity)
    {
        if (isGrabbed)
            return;

        float sdt = dt / subSteps;

        for (int i = 0; i < subSteps; i++)
        {
            vel += gravity * sdt;
            pos += vel * sdt;
        }
    }

    public void UpdateVisualPosition()
    {
        ballTransform.position = pos;
    }
}
