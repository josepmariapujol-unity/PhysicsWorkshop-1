using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPBall
{
    public readonly float radius;
    public readonly float mass;
    public Vector3 pos;
    public Vector3 vel;
    private Transform ballTransform;

    //Getters
    //Left border of the AABB belonging to the disc
    public float Left => pos.x - radius;
    //Right border of the AABB belonging to the disc
    public float Right => pos.x + radius;

    public BPBall(Vector3 ballVel, Transform ballTrans, float density = 1f)
    {
        ballTransform = ballTrans;
        pos = ballTransform.position;
        radius = ballTransform.localScale.x * 0.5f;
        mass = (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3f) * density;
        vel = ballVel;
    }

    public void SimulateBall(float dt)
    {
        pos += vel * dt;
    }

    public void UpdateVisualPosition()
    {
        ballTransform.position = pos;
    }
}
