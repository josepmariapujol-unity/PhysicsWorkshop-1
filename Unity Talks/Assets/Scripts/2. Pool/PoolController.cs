using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Simulate a Pool balls colliding each other
public class PoolController : MonoBehaviour
{
    public GameObject ballPrefabGO;
    private readonly List<PoolBall> balls = new List<PoolBall>();

    [Range(1, 1000)]
    public int numBalls;
    [Range(0.0f, 1.0f)]
    public float restitution;

    public Vector2 worldSize = new Vector2(20.0f, 15.0f);
    private Vector3 gravity = new Vector3(0f, 0f, 0f);

    private int subSteps = 1;

    void Start()
    {
        SetupScene();
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    void Simulate()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            PoolBall b1 = balls[i];

            //Update velocity and position
            b1.SimulateBall(subSteps, Time.fixedDeltaTime, gravity);

            //Handle Ball-Ball Collision
            for (int j = i + 1; j < balls.Count; j++)
            {
                HandleBallBallCollision(b1, balls[j]);
            }

            //Handle Ball Wall Collision
            HandleBallWallCollision(b1);

            b1.UpdateVisualPosition();
        }
    }

    void HandleBallBallCollision(PoolBall b1, PoolBall b2)
    {
        Vector3 dir = b2.pos - b1.pos;
        float dist = dir.magnitude;

        if (dist == 0f || dist > b1.radius + b2.radius)
            return;

        Vector3 dirNormalized = dir.normalized;

        // Position correction
        float corr = (b1.radius + b2.radius - dist) * 0.5f; //corr is overlapping / 2
        b1.pos += dirNormalized * -corr; //-corr because direction goes from b1 to b2
        b2.pos += dirNormalized * corr;

        //Update velocities
        // TODO Exercise 2.1: Decompose initial ball velocities along the collision normal (reduce to 1D for collision response).
        
        float v1 = 0;
        float v2 = 0;
        
        // --------------------------------------------------------------------

        float m1 = b1.mass;
        float m2 = b2.mass;

        
        // TODO Exercise 2.2: Calculate new velocities after collisions for newV1 and newV2.
        
        float newV1 = 0;
        float newV2 = 0;
        
        // --------------------------------------------------------------------
        
        b1.vel += dirNormalized * (newV1 - v1); //(without - v1), you’d effectively double-count existing normal velocity
        b2.vel += dirNormalized * (newV2 - v2);
    }

    void HandleBallWallCollision(PoolBall b)
    {
        if (b.pos.x < b.radius)
        {
            b.pos.x = b.radius;
            b.vel.x *= -1;
        }
        else if (b.pos.x > worldSize.x - b.radius)
        {
            b.pos.x = worldSize.x - b.radius;
            b.vel.x *= -1;
        }

        if (b.pos.y < b.radius)
        {
            b.pos.y = b.radius;
            b.vel.y *= -1;
        }
        else if (b.pos.y > worldSize.y - b.radius)
        {
            b.pos.y = worldSize.y - b.radius;
            b.vel.y *= -1;
        }
    }

    void SetupScene()
    {
        // Add random balls
        for (int i = 0; i < numBalls; i++)
        {
            float radius = Random.Range(0.05f, 0.50f);
            Vector3 pos = new Vector3(
                Random.Range(0f, worldSize.x),
                Random.Range(0f, worldSize.y),
                0f
            );

            Vector3 vel = new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(-4f, 4f),
                0f
            );

            GameObject ballGo = Instantiate(ballPrefabGO, pos, Quaternion.identity, transform);
            ballGo.transform.localScale = Vector3.one * (radius * 2f);
            balls.Add(new PoolBall(vel, ballGo.transform));
        }
    }

    private void OnDrawGizmos()
    {
        // 2D canvas guides: floor, left wall, right wall and ceiling.
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(worldSize.x, 0f, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0f, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, worldSize.y, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);
    }
}
