using UnityEngine;

public class PoolBall
{
    public readonly float radius;
    public readonly float mass;
    public Vector3 pos;
    public Vector3 vel;
    private Transform ballTransform;

    public PoolBall(Vector3 ballVel, Transform ballTrans, float density = 1f)
    {
        ballTransform = ballTrans;
        pos = ballTransform.position;
        radius = ballTransform.localScale.x * 0.5f;
        mass = (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3f) * density;
        vel = ballVel;
    }

    public void SimulateBall(int subSteps, float dt, Vector3 gravity)
    {
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
