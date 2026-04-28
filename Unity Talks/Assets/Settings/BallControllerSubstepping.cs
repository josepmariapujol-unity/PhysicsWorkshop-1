using UnityEngine;

// Simulate a ball with sub stepping
public class BallControllerSubstepping : MonoBehaviour
{
    //How to improve accuracy of a simulation?
    //- Find formula with calculus - impossible for difficult problems
    //- More sophisticated integration - slower, no improvement when collision occurs
    //- Make dt small - works great! Introduce sub stepping

    public GameObject ball;

    private Vector2 pos;
    private Vector2 vel;
    private Vector2 gravity = new Vector2(0, -9.81f);

    public Vector3 worldSize = new Vector3(20.0f, 15.0f, 0f);

    public int subSteps = 5;

    void Start()
    {
        pos = Vector2.zero;
        vel = new Vector2(10, 15);
    }

    // FixedUpdate runs at a constant timestep (Time.fixedDeltaTime).
    // We perform the physics simulation here (including sub stepping)
    // to ensure stable and consistent results regardless of frame rate.
    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        //Update velocity and position with sub stepping
        float sdt = Time.fixedDeltaTime / subSteps;

        for (int i = 0; i < subSteps; i++)
        {
            vel += gravity * sdt;
            pos += vel * sdt;
        }

        //Handle Ball Wall Collision
        if (pos.x < 0.0)
        {
            pos.x = 0.0f;
            vel.x = -vel.x;
        }
        else if (pos.x > worldSize.x)
        {
            pos.x = worldSize.x;
            vel.x = -vel.x;
        }

        if (pos.y < 0.0)
        {
            pos.y = 0.0f;
            vel.y = -vel.y;
        }

        ball.transform.position = pos;
    }

    private void OnDrawGizmos()
    {
        // 2D canvas guides: floor, left wall, right wall.
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(worldSize.x, 0f, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0f, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);
    }
}
