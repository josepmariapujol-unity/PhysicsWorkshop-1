using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PBDCollisions
{
    public static void HandleBallBallCollision(PBDBall b1, PBDBall b2, float restitution)
    {
        Vector3 dir = b2.pos - b1.pos;
        float dist = dir.magnitude;

        if (dist == 0f || dist > b1.radius + b2.radius)
            return;

        Vector3 dirNormalized = dir.normalized;

        // Position correction
        float corr = (b1.radius + b2.radius - dist) * 0.5f;
        b1.pos += dirNormalized * -corr; //-corr because dir goes from b1 to b2
        b2.pos += dirNormalized * corr;

        //Update velocities
        //The velocity is now in 1D making it easier to use standardized physics equations
        float v1 = Vector3.Dot(b1.vel, dirNormalized);
        float v2 = Vector3.Dot(b2.vel, dirNormalized);

        float m1 = b1.mass;
        float m2 = b2.mass;

        float newV1 = (m1 * v1 + m2 * v2 - m2 * (v1 - v2) * restitution) / (m1 + m2);
        float newV2 = (m1 * v1 + m2 * v2 - m1 * (v2 - v1) * restitution) / (m1 + m2);

        b1.vel += dirNormalized * (newV1 - v1);
        b2.vel += dirNormalized * (newV2 - v2);
    }
}
