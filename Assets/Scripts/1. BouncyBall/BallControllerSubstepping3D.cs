using UnityEngine;

// Simulate a ball in 3D
public class BallControllerSubstepping3D : MonoBehaviour
{
    public GameObject ball;

    private Vector3 pos;
    private Vector3 vel;
    private Vector3 gravity = new Vector3(0, -9.81f, 0f);

    public Vector3 worldSize = new Vector3(20.0f, 15.0f, 15.0f);

    public int subSteps = 5;

    void Start()
    {
        pos = Vector3.zero;
        vel = new Vector3(10, 15, 5);
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        //Update velocity and position with sub stepping
        // TODO Exercise 1.2: Implement basic motion using the Symplectic Euler method with substepping by updating velocity (vel) and position (pos).        
        
        
        
        
        
        
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

        if (pos.z < 0.0)
        {
            pos.z = 0.0f;
            vel.z = -vel.z;
        }
        else if (pos.z > worldSize.z)
        {
            pos.z = worldSize.z;
            vel.z = -vel.z;
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
        // 3D canvas guides: floor, left wall, right wall.
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(worldSize.x, 0f, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0f, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);

        Debug.DrawLine(new Vector3(0f, 0f, worldSize.z), new Vector3(worldSize.x, 0f, worldSize.z), Color.red);
        Debug.DrawLine(new Vector3(0f, 0f, worldSize.z), new Vector3(0f, worldSize.y, worldSize.z), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0f, worldSize.z), new Vector3(worldSize.x, worldSize.y, worldSize.z), Color.red);

        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, worldSize.z), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0f, 0f), new Vector3(worldSize.x, 0f, worldSize.z), Color.red);
    }
}
