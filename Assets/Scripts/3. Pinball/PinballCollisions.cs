using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PinballCollisions
{
    private static Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;

        // TODO Exercise 3.1. Calculate float t for the projection of point p onto the line a–b, then clamp it to the range [0,1]
        
        float t = 0;
        
        // --------------------------------------------------------------------

        //Find the closest point from p to the line segment a-b
        Vector3 c = a + t * ab;

        return c;
    }

    public static void HandleBallBallCollision(PinballBall b1, PinballBall b2, float restitution)
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
        float v1 = Vector3.Dot(b1.vel, dirNormalized);
        float v2 = Vector3.Dot(b2.vel, dirNormalized);

        float m1 = b1.mass;
        float m2 = b2.mass;

        float newV1 = (m1 * v1 + m2 * v2 - m2 * (v1 - v2) * restitution) / (m1 + m2);
        float newV2 = (m1 * v1 + m2 * v2 - m1 * (v2 - v1) * restitution) / (m1 + m2);

        b1.vel += dirNormalized * (newV1 - v1);
        b2.vel += dirNormalized * (newV2 - v2);
    }

    public static void HandleBallBumperCollision(PinballBall ball, Bumper bumper)
    {
        Vector3 dir = ball.pos - bumper.pos;
        float dist = dir.magnitude;

        if (dist == 0f || dist > ball.radius + bumper.radius)
            return;

        Vector3 dirNormalized = dir.normalized;

        // Position correction
        float corr = ball.radius + bumper.radius - dist;
        ball.pos += dirNormalized * corr;

        //Update velocity
        float vel = Vector3.Dot(ball.vel, dirNormalized);

        ball.vel += dirNormalized * (bumper.pushVel - vel);
    }

    public static void HandleBallFlipperCollision(PinballBall ball, Flipper flipper)
    {
        Vector3 closest = ClosestPointOnSegment(ball.pos, flipper.pos, flipper.EndTip());
        Vector3 dir = ball.pos - closest;

        float dist = dir.magnitude;

        //The ball is not colliding with the flipper
        if (dist == 0f || dist > ball.radius + flipper.radius)
            return;

        //Update position: The distance between the ball and the closest point on the flipper
        Vector3 dirNormalized = dir.normalized;

        //Move the ball outside the flipper
        float corr = ball.radius + flipper.radius - dist;
        ball.pos += dirNormalized * corr;

        //Update velocity
        //Radius vector from rotation center to contact point on the surface of the flipper
        // TODO Exercise 3.2. Calculate Vector3 radius from rotation center to contact point on the surface of the flipper


        Vector3 radius = new Vector3(0, 0, 0);
        
        
        // --------------------------------------------------------------------

        //Contact velocity by turning the vector 90 degrees and scaling with angular velocity
        Vector3 surfaceVel = Utils.PerpendicularXY(radius) * flipper.currentAngularVel;

        //The flipper modifies ball velocity along the penetration direction dir
        float vel = Vector3.Dot(ball.vel, dirNormalized);
        float newVel = Vector3.Dot(surfaceVel, dirNormalized) * flipper.restitution;

        ball.vel += dirNormalized * (newVel - vel);
    }

    //The walls are a list if edges ordered counter-clockwise
    public static void HandleBallWallEdgesCollision(PinballBall ball, List<Vector3> border, float restitution)
    {
        //We need at least a triangle (the start and end are the same point, thus the 4)
        if (border.Count < 4)
            return;

        Vector3 closest = Vector3.zero;
        Vector3 ab = Vector3.zero;
        Vector3 wallNormal = Vector3.zero;

        float minDist = 0f;

        //Loop all walls to get the segment closest to the ball to follow up with
        for (int i = 0; i < border.Count - 1; i++)
        {
            Vector3 a = border[i];
            Vector3 b = border[i + 1];
            Vector3 c = ClosestPointOnSegment(ball.pos, a, b);

            float testDist = (ball.pos - c).magnitude;

            if (i == 0 || testDist < minDist)
            {
                minDist = testDist;
                closest = c;
                ab = b - a;
                wallNormal = Utils.PerpendicularXY(ab);
            }
        }

        Vector3 dir = ball.pos - closest;
        float dist = dir.magnitude;

        //Direction closest point on the wall to the ball
        Vector3 dirNormalized = dir.normalized;

        //If they point in the same direction, meaning the ball is to the left of the wall
        if (Vector3.Dot(dirNormalized, wallNormal) >= 0f)
        {
            //The ball is not colliding with the wall
            if (dist > ball.radius)
                return;

            //The ball is colliding with the wall, so push it in again
            float corr = ball.radius - dist;
            ball.pos += dirNormalized * corr;
        }
        else //Push in the opposite direction because the ball is outside the wall (to the right)
        {
            //We have to push it dist so it ends up on the wall, and then radius so it ends up outside the wall
            float corr = ball.radius - dist;
            ball.pos += dirNormalized * -corr;
        }

        //Update velocity
        float vel = Vector3.Dot(ball.vel, dirNormalized);
        float newVel = Mathf.Abs(vel) * restitution;

        ball.vel += dirNormalized * (newVel - vel);
    }
}
