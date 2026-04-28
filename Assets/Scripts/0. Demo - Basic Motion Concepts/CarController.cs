using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//Simulate a basic car
public class CarController : MonoBehaviour
{
    public Transform carTrans;

    private Queue<float> carPos = new();
    private Queue<float> carVel = new();
    private Queue<float> carAcc = new();

    private float pos;
    private float vel;
    private float acc;
    private int pedalPos; // -1 for reverse, 0 for no pedal, 1 for forward
    private bool constantAccInput;
    private bool constantVelInput;
    private readonly float constantAccValue = 2f;
    private readonly float constantVelValue = 3f;
    private bool hasMovementStarted;
    private Vector3 startPos;
    private GUIStyle textStyle;

    void Start()
    {
        pos = carTrans.position.x;
        carPos.Enqueue(pos);
        carVel.Enqueue(0f);
        carAcc.Enqueue(0f);
        startPos = carTrans.position;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        pedalPos = 0;

        if (keyboard.rightArrowKey.isPressed)
            pedalPos = 1;
        else if (keyboard.leftArrowKey.isPressed)
            pedalPos = -1;

        constantAccInput = keyboard.aKey.isPressed;
        constantVelInput = keyboard.vKey.isPressed;

        if (!hasMovementStarted && IsMovementKeyPressedThisFrame(keyboard))
            StartGraphCapture();

        Vector3 visualCarPos = carTrans.position;
        visualCarPos.x = pos;
        carTrans.position = visualCarPos;

        if (hasMovementStarted)
            ShowGraph();
    }

    private void FixedUpdate()
    {
        if (constantVelInput)
        {
            acc = 0f;
            vel = constantVelValue;
        }
        else
        {
            if (constantAccInput)
            {
                acc = constantAccValue;
            }
            else
            {
                //Acceleration
                float pedalFactor = 5f;
                if (pedalPos == 1)
                    acc += pedalFactor * Time.fixedDeltaTime;
                if (pedalPos == -1)
                    acc -= pedalFactor * Time.fixedDeltaTime;
            }

            //Velocity
            vel += acc * Time.fixedDeltaTime;
        }

        //Position
        pos += vel * Time.fixedDeltaTime;

        if (!hasMovementStarted)
            return;

        //Cache
        carPos.Enqueue(pos);
        carVel.Enqueue(vel);
        carAcc.Enqueue(acc);
    }

    private static bool IsMovementKeyPressedThisFrame(Keyboard keyboard)
    {
        return keyboard.rightArrowKey.wasPressedThisFrame
               || keyboard.leftArrowKey.wasPressedThisFrame
               || keyboard.vKey.wasPressedThisFrame
               || keyboard.aKey.wasPressedThisFrame;
    }

    private void StartGraphCapture()
    {
        hasMovementStarted = true;
        carPos.Clear();
        carVel.Clear();
        carAcc.Clear();
        carPos.Enqueue(pos);
        carVel.Enqueue(vel);
        carAcc.Enqueue(acc);
    }

    private void ShowGraph()
    {
        DisplayGraph(carPos, DisplayShapes.ColorOptions.White);
        DisplayGraph(carVel, DisplayShapes.ColorOptions.Yellow);
        DisplayGraph(carAcc, DisplayShapes.ColorOptions.Red);
    }

    private void OnGUI()
    {
        textStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            fontSize = 22
        };

        float startX = 10f;
        float startY = 10f;
        float lineHeight = 30f;
        float width = 420f;

        textStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(startX, startY + lineHeight * 0f, width, lineHeight), "Position", textStyle);

        textStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(startX, startY + lineHeight * 1f, width, lineHeight), "Velocity", textStyle);

        textStyle.normal.textColor = Color.red;
        GUI.Label(new Rect(startX, startY + lineHeight * 2f, width, lineHeight), "Acceleration", textStyle);

        textStyle.normal.textColor = Color.white;
        float bottomMargin = 10f;
        float bottomY = Screen.height - bottomMargin - lineHeight;
        GUI.Label(new Rect(startX, bottomY - lineHeight * 0f, width, lineHeight), "Press Arrows Manual Control", textStyle);
        GUI.Label(new Rect(startX, bottomY - lineHeight * 1f, width, lineHeight), "Press 'V' Constant Velocity", textStyle);
        GUI.Label(new Rect(startX, bottomY - lineHeight * 2f, width, lineHeight), "Press 'A' Constant Acceleration", textStyle);
        GUI.Label(new Rect(startX, bottomY - lineHeight * 3f, width, lineHeight), "Press 'R' Reset", textStyle);
    }

    private void DisplayGraph(Queue<float> data, DisplayShapes.ColorOptions color)
    {
        // Normalize frame-to-frame deltas using track span so all three graphs stay similarly scaled.
        Vector2 diffRemapRange = new Vector2(startPos.x, startPos.x * -1f);
        Vector2 graphRange = new Vector2(-3f, 3f);

        // Time ticks on constantly with some value
        float yScale = 0.05f;
        List<Vector3> graphPos3D = new();

        // Distance traveled is y-axis and time is x-axis
        List<float> graphY = new List<float>();
        foreach (float value in data)
        {
            graphY.Add(value);
        }
        Vector3 graphPos = new Vector3(startPos.x, 0f, 0f);

        for (int i = 0; i < graphY.Count; i++)
        {
            graphPos3D.Add(graphPos);

            if (i > 0)
            {
                // Time ticks on constantly with some value
                graphPos.x += yScale;

                // Make it fit on the screen
                float diff = graphY[i] - graphY[i - 1];
                float diffNormalized = Remap(diff, diffRemapRange, graphRange);
                graphPos.y += diffNormalized;
            }
        }

        DisplayShapes.DrawLine(graphPos3D, color);
    }

    private static float Remap(float value, Vector2 range1, Vector2 range2)
    {
        float t = Mathf.InverseLerp(range1.x, range1.y, value);
        return Mathf.Lerp(range2.x, range2.y, t);
    }
}
