using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper
{
    public readonly float radius;
    private readonly float length;
    public Vector3 pos;

    public float restitution;

    //Angles in radians
    private readonly float restAngle;
    private readonly float maxRotation;
    private readonly float sign;
    private readonly float angularVel;

    private float rotation;
    public float currentAngularVel;

    public float touchIdentifier = -1f;

    public Flipper(float radius, Vector3 pos, float length, float restAngle, float maxRotation, float angularVel, float restitution)
    {
        this.radius = radius;
        this.pos = pos;
        this.length = length;
        this.restAngle = restAngle;
        this.maxRotation = Mathf.Abs(maxRotation);
        this.sign = Mathf.Sign(maxRotation);
        this.angularVel = angularVel;
        this.restitution = restitution;

        this.rotation = 0f;
        this.currentAngularVel = 0f;
    }

    public void Simulate(float dt)
    {
        float prevRotation = this.rotation;
        bool pressed = this.touchIdentifier >= 0f;

        if (pressed)
            this.rotation = Mathf.Min(this.rotation + dt * angularVel, this.maxRotation);
        else
            this.rotation = Mathf.Max(this.rotation - dt * angularVel, 0f);

        //omega = alpha / dt as in vel = dist / time
        this.currentAngularVel = this.sign * (this.rotation - prevRotation) / dt;
    }

    //Get the tip of the flipper = the other position we need to display it
    public Vector3 EndTip()
    {
        float angle = this.restAngle + this.sign * this.rotation;
        float x = Mathf.Cos(angle);
        float y = Mathf.Sin(angle);

        //This one is already normalized
        Vector3 dir = new Vector3(x, y, 0f);
        Vector3 tip = this.pos + dir * length;
        return tip;
    }
}
