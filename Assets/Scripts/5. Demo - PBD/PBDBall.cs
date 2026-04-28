using UnityEngine;

public class PBDBall : MonoBehaviour
{
    public readonly float radius;
    public readonly float mass;
    public Vector3 pos;
    public Vector3 vel;
    public Transform ballTransform;

    public Vector3 prevPos;

    public PBDBall(Transform ballTrans, float density = 1f)
    {
        ballTransform = ballTrans;
        pos = ballTransform.position;
        radius = ballTransform.localScale.x * 0.5f;
        mass = (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3f) * density;
    }

    public void Integration(float dt, Vector3 gravity)
    {
        vel += gravity * dt;
        prevPos = pos;
        pos += vel * dt;
    }

    //Move the bead to the closest point on the wire
    public void SolveConstraints(Vector3 center, float radiusWire)
    {
        //Direction from center to the bead
        Vector3 dir = pos - center;
        float dist = dir.magnitude;

        if (dist == 0f)
            return;

        Vector3 dirNormalized = dir.normalized;

        //Constraint error: How far should we move the bead?
        float lambda = radiusWire - dist;
        pos += dirNormalized * lambda;
    }

    //Calculate new velocity because the velocity we calculate during integration will explode due to gravity
    public void UpdateVelocity(float dt)
    {
        vel = new Vector3(0,0,0);// (pos - prevPos) / dt;
    }

    public virtual void UpdateVisualPosition()
    {
        ballTransform.position = pos;
    }
}
