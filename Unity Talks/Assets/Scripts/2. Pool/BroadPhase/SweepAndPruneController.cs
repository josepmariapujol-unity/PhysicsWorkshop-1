using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SweepAndPruneController : MonoBehaviour
{
    public GameObject ballPrefabGO;
    private enum CollisionAlgorithm
    {
        BruteForce,
        SweepPrune
    }
    private CollisionAlgorithm activeCollisionAlgorithm = CollisionAlgorithm.SweepPrune;

    private int collisionChecks;
    private int actualCollisions;

    private Vector2 worldSize = new Vector2(30.0f, 30.0f);

    private List<BPBall> balls  = new();
    private const int numBalls = 1000;
    private int subSteps = 1;

    private void Start()
    {
        SetupScene();
    }

    private void Update()
    {
        if (Keyboard.current.bKey.isPressed)
            activeCollisionAlgorithm = CollisionAlgorithm.BruteForce;
        if (Keyboard.current.sKey.isPressed)
            activeCollisionAlgorithm = CollisionAlgorithm.SweepPrune;
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        collisionChecks = 0;
        actualCollisions = 0;

        float sdt = Time.fixedDeltaTime / subSteps;

        for (int i = 0; i < subSteps; i++)
        {
            //Move each ball and make sure the ball is not outside the border
            foreach (BPBall b in balls)
            {
                b.SimulateBall(sdt);
                HandleBallWallCollision(b);
            }

            //Check for collisions with other balls
            if (activeCollisionAlgorithm == CollisionAlgorithm.BruteForce)
                BruteForceCollisions(balls);
            else
                SweepAndPruneCollisions(balls);

            foreach (BPBall b in balls)
            {
                b.UpdateVisualPosition();
            }
        }
    }

    private void SweepAndPruneCollisions(List<BPBall> balls)
    {
        // STEP 1: Sort all balls by their AABB left edge
        // This creates a 1D ordering along the X-axis
        List<BPBall> sortedBalls = balls.OrderBy(b => b.Left).ToList();

        // STEP 2: Sweep through the sorted list
        // For each ball, we only compare with balls to the right
        for (int i = 0; i < sortedBalls.Count; i++)
        {
            BPBall b1 = sortedBalls[i];

            // Compare with subsequent balls (j > i)
            for (int j = i + 1; j < sortedBalls.Count; j++)
            {
                BPBall b2 = sortedBalls[j];

                // EARLY EXIT (core optimization of Sweep and Prune)
                // If the next ball starts after b1 ends on the X-axis,
                if (b2.Left > b1.Right)
                    break;

                // STEP 3: Narrow check on Y-axis
                // Now check if they overlap in Y
                // TODO Exercise 2.3. Calculate if the distance between centers is smaller than sum of radius, if it is then use add: SolveCollision(b1, b2); to resolve collisions


            }
        }
    }

    //Slow collision detection where we check each ball against all other balls
    private void BruteForceCollisions(List<BPBall> balls)
    {
        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = i + 1; j < balls.Count; j++)
            {
                SolveCollision(balls[i], balls[j]);
            }
        }
    }

    //Check if two balls are colliding
    //If so push them apart and update velocity
    private void SolveCollision(BPBall b1, BPBall b2)
    {
        collisionChecks++;

        if (HandleBallBallCollision(b1, b2, restitution: 1))
            actualCollisions++;
    }

    public static bool HandleBallBallCollision(BPBall b1, BPBall b2, float restitution)
    {
        Vector3 dir = b2.pos - b1.pos;
        float dist = dir.magnitude;

        //Check if the balls are colliding
        if (dist == 0f || dist > b1.radius + b2.radius)
            return false;

        //Normalized direction
        Vector3 dirNormalized = dir.normalized;

        // Position correction
        float corr = (b1.radius + b2.radius - dist) * 0.5f; //corr is overlapping / 2
        b1.pos += dirNormalized * -corr; //-corr because direction goes from b1 to b2
        b2.pos += dirNormalized * corr;

        //Update velocities
        //The velocity is now in 1D making it easier to use standardized physics equations
        float v1 = Vector3.Dot(b1.vel, dirNormalized);
        float v2 = Vector3.Dot(b2.vel, dirNormalized);

        float m1 = b1.mass;
        float m2 = b2.mass;

        //If we assume the objects are stiff we can calculate the new velocities after collision
        float newV1 = (m1 * v1 + m2 * v2 - m2 * (v1 - v2) * restitution) / (m1 + m2);
        float newV2 = (m1 * v1 + m2 * v2 - m1 * (v2 - v1) * restitution) / (m1 + m2);

        //Change velocity components along dir
        //Subtract the old velocity because it doesnt exist anymore and then add the new velocity
        b1.vel += dirNormalized * (newV1 - v1); //(without - v1), you’d effectively double-count existing normal velocity
        b2.vel += dirNormalized * (newV2 - v2);

        return true;
    }


    void HandleBallWallCollision(BPBall b)
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
            balls.Add(new BPBall(vel, ballGo.transform));
        }
    }

    private void OnGUI()
    {
        MyOnGUI();
    }

    private void OnDrawGizmos()
    {
        // 2D canvas guides: floor, left wall, right wall and ceiling.
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(worldSize.x, 0f, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0f, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, worldSize.y, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);
    }

    public void MyOnGUI()
    {
        GUILayout.BeginHorizontal("box");
        int fontSize = 20;
        RectOffset offset = new(5, 5, 5, 5);

        string infoText = $"[{activeCollisionAlgorithm}] Spheres: {numBalls} | Collision checks / frame: {collisionChecks} | Actual collisions / frame: {actualCollisions}";
        GUIStyle textStyle = GUI.skin.GetStyle("Label");
        textStyle.fontSize = fontSize;
        textStyle.margin = offset;
        GUILayout.Label(infoText, textStyle);
        GUILayout.EndHorizontal();
    }
}
