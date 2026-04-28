using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper
{
    public readonly float radius;
    public Vector3 pos;

    public readonly float pushVel; //The velocity the ball gets if it collides with this obstacle

    public Bumper(float radius, Vector3 pos, float pushVel)
    {
        this.radius = radius;
        this.pos = pos;
        this.pushVel = pushVel;
    }
}
