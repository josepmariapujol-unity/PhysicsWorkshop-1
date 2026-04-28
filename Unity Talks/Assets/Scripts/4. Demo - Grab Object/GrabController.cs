using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabController : MonoBehaviour
{
    public Transform ballTransform;
    private GrabBall ball;

    private Vector3 gravity = new Vector3(0f, -9.81f, 0f);
    private Vector3 worldSize = new Vector3(20.0f, 15.0f, 15.0f);

    private Grabber grabber;

    [SerializeField] [Min(0f)] float cursorReleaseVelocityScale = 1.25f;

    private void Start()
    {
        //Init the ball
        ball = new GrabBall(ballTransform);
        ball.vel = new Vector3(3f, 5f, 2f);

        //Init the grabber
        grabber = new Grabber(Camera.main, cursorReleaseVelocityScale);
    }

    private void Update()
    {
        //Update the visual position of the ball
        ball.UpdateVisualPosition();
        grabber.MoveGrab();
    }

    //User interactions should be in LateUpdate
    private void LateUpdate()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            List<IGrabbable> temp = new List<IGrabbable>();
            temp.Add(ball);
            grabber.StartGrab(temp);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            grabber.EndGrab();
        }
    }

    private void FixedUpdate()
    {
        //Update velocity and position
        ball.SimulateBall(1, Time.fixedDeltaTime, gravity);

        //Handle Wall Collision detection
        HandleWallCollision(ball);
    }

    void HandleWallCollision(GrabBall b)
    {
        var restitution = 0.8f;

        if (b.pos.x < b.radius)
        {
            b.pos.x = b.radius;
            b.vel.x *= -1 * restitution;
        }
        else if (b.pos.x > worldSize.x - b.radius)
        {
            b.pos.x = worldSize.x - b.radius;
            b.vel.x *= -1 * restitution;
        }

        if (b.pos.y < b.radius)
        {
            b.pos.y = b.radius;
            b.vel.y *= -1 * restitution;
        }
        else if (b.pos.y > worldSize.y - b.radius)
        {
            b.pos.y = worldSize.y - b.radius;
            b.vel.y *= -1 * restitution;
        }

        if (b.pos.z < b.radius)
        {
            b.pos.z = b.radius;
            b.vel.z *= -1 * restitution;
        }
        else if (b.pos.z > worldSize.z - b.radius)
        {
            b.pos.z = worldSize.z - b.radius;
            b.vel.z *= -1 * restitution;
        }
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
