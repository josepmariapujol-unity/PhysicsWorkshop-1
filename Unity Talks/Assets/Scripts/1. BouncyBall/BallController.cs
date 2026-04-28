using UnityEngine;

// Simulate a bouncy ball
public class BallController : MonoBehaviour
{
    public GameObject ball;

    private Vector2 pos;
    private Vector2 vel;
    private Vector2 gravity = new Vector2(0, -9.81f);

    public Vector3 worldSize = new Vector3(20.0f, 15.0f, 0f);

    void Start()
    {
        pos = Vector2.zero;
        vel = new Vector2(10, 15);
    }

    void Update()
    {
        Simulate();
    }

    private void Simulate()
    {
        //Update velocity and position
        // TODO Exercise 1.1: Implement basic motion using the Symplectic Euler method by updating velocity (vel) and position (pos).
        
        
        
        
        
        
        // --------------------------------------------------------------------

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
        //Sky is the limit, so no collision detection in y-positive direction ;)

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
