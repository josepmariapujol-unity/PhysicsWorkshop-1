using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//Simulate a pinball machine
//Based on: https://matthias-research.github.io/pages/tenMinutePhysics/
public class PinballController : MonoBehaviour
{
    public GameObject ballPrefabGO;
    public GameObject flipper_L_GO;
    public GameObject flipper_R_GO;
    public Transform boundaryTransformsParent; //Should be ordered counter-clockwise
    public Transform bumpersParent;

    private bool is_L_FlipperActivated;
    private bool is_R_FlipperActivated;

    private Flipper flipper_L;
    private Flipper flipper_R;
    private List<PinballBall> balls = new List<PinballBall>();
    private List<Bumper> bumpers = new List<Bumper>();

    private readonly List<Vector3> boundary = new List<Vector3>();

    private Vector3 gravity = new Vector3(0f, -3f, 0f); //Low gravity since balls are rolling on a surface, not falling through air
    private float restitution = 0.3f;

    private void Start()
    {
        SetupScene();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        ReloadScene(keyboard);

        is_L_FlipperActivated = keyboard.leftArrowKey.isPressed;
        is_R_FlipperActivated = keyboard.rightArrowKey.isPressed;
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        flipper_L.touchIdentifier = is_L_FlipperActivated ? 1f : -1f;
        flipper_R.touchIdentifier = is_R_FlipperActivated ? 1f : -1f;

        flipper_L.Simulate(Time.fixedDeltaTime);
        flipper_R.Simulate(Time.fixedDeltaTime);

        for (int i = 0; i < balls.Count; i++)
        {
            PinballBall ball = balls[i];

            //Update velocity and position
            ball.SimulateBall(1, Time.fixedDeltaTime, gravity);

            //Handle Ball-Ball Collision
            for (int j = i + 1; j < balls.Count; j++)
            {
                PinballCollisions.HandleBallBallCollision(ball, balls[j], restitution);
            }

            //Handle Ball Bumper Collision
            foreach (Bumper bumper in bumpers)
            {
                PinballCollisions.HandleBallBumperCollision(ball, bumper);
            }

            //Handle Ball Flipper Collision
            PinballCollisions.HandleBallFlipperCollision(ball, flipper_L);
            PinballCollisions.HandleBallFlipperCollision(ball, flipper_R);

            //Handle Ball Wall Collision
            PinballCollisions.HandleBallWallEdgesCollision(ball, boundary, restitution);
        }

        foreach (PinballBall ball in balls)
        {
            ball.UpdateVisualPosition();
        }
    }

    private void LateUpdate()
    {
        //Display boundary and flippers
        DisplayShapes.DrawLine(boundary, DisplayShapes.ColorOptions.White);
        DisplayShapes.DrawCapsule(flipper_L.pos, flipper_L.EndTip(), flipper_L.radius, DisplayShapes.ColorOptions.Red);
        DisplayShapes.DrawCapsule(flipper_R.pos, flipper_R.EndTip(), flipper_R.radius, DisplayShapes.ColorOptions.Red);
    }

    private void SetupScene()
    {
        //Add boundary
        foreach (Transform trans in boundaryTransformsParent)
            boundary.Add(trans.position);

        //Close the boundary
        boundary.Add(boundary[0]);
        boundaryTransformsParent.gameObject.SetActive(false);

        //Add balls
        for (int i = 0; i < 10; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(5f, 10f),
                0f
            );
            float size = Random.Range(0.3f, 1f);

            GameObject ballGo = Instantiate(ballPrefabGO, pos, Quaternion.identity, transform);
            ballGo.transform.localScale = Vector3.one * size;
            balls.Add(new PinballBall(Vector3.zero, ballGo.transform, restitution));
        }

        //Add obstacles = bumpers
        foreach (Transform trans in bumpersParent)
        {
            float pushVel = 15f;
            float bumperRadius = trans.localScale.x * 0.5f;
            Vector3 pos = trans.position;
            Bumper bumper = new Bumper(bumperRadius, pos, pushVel);
            bumpers.Add(bumper);
        }

        //Add flippers
        float flipperRadius = flipper_L_GO.transform.localScale.x * 0.5f;
        float flipperLength = 1.6f;
        float maxRotation = 1f;
        float restAngle = 0.5f;
        float angularVel = 10f;
        float flipperRestitution = 1f;

        Vector3 pos_L = flipper_L_GO.transform.position;
        Vector3 pos_R = flipper_R_GO.transform.position;

        flipper_L = new Flipper(flipperRadius, pos_L, flipperLength, -restAngle, maxRotation, angularVel, flipperRestitution);
        flipper_R = new Flipper(flipperRadius, pos_R, flipperLength, Mathf.PI + restAngle, -maxRotation, angularVel, flipperRestitution);
    }

    private void ReloadScene(Keyboard keyboard)
    {
        if (keyboard.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
