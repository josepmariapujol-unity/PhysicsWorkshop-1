using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simulate beads attached to a circular wire
//Based on: https://matthias-research.github.io/pages/tenMinutePhysics/
public class PBDController : MonoBehaviour
{
    public GameObject ballPrefabGO;

    //All beads on the constraint
    private List<PBDBall> allBeads = new List<PBDBall>();

    private Vector3 wireCenter = Vector3.zero;
    private float wireRadius = 5f;
    private Vector3 gravity = new Vector3(0f, -9.81f, 0f);
    private float restitution = 1f;

    //Important to use sub steps or the bead will lose momentum
    //The more time steps the better, but also slower and may lead floating point precision issues
    private int subSteps = 20;

    private void Start()
    {
        SetupScene();
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        float sdt = Time.fixedDeltaTime / subSteps;
        for (int i = 0; i < subSteps; i++)
        {
            for (int ball = 0; ball < allBeads.Count; ball++)
                allBeads[ball].Integration(sdt, gravity);

            for (int ball = 0; ball < allBeads.Count; ball++)
                allBeads[ball].SolveConstraints(wireCenter, wireRadius);

            for (int ball = 0; ball < allBeads.Count; ball++)
                allBeads[ball].UpdateVelocity(sdt);

            for (int ball = 0; ball < allBeads.Count; ball++)
            {
                for (int j = ball + 1; j < allBeads.Count; j++)
                {
                    PBDCollisions.HandleBallBallCollision(allBeads[ball], allBeads[j], restitution);
                }
            }
            
            foreach (PBDBall b in allBeads)
            {
                b.UpdateVisualPosition();
            }
        }
    }

    private void SetupScene()
    {
        //Add balls
        for (int i = 0; i < 6; i++)
        {
            Vector2 posOnCircle = Random.insideUnitCircle.normalized * 5f;
            Vector3 pos = new Vector3(posOnCircle.x, posOnCircle.y, 0f);

            GameObject ballGo = Instantiate(ballPrefabGO, pos, Quaternion.identity, transform);
            ballGo.transform.localScale = Vector3.one * Random.Range(0.5f, 2f);
            allBeads.Add(new PBDBall(ballGo.transform, restitution));
        }
    }
    
    private void LateUpdate()
    {
        //Display the circle the beads are attached to
        DisplayShapes.DrawCircle(wireCenter, wireRadius, DisplayShapes.ColorOptions.White, DisplayShapes.Space2D.XY);
    }
}
