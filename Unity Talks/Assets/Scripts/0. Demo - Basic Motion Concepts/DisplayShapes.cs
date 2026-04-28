using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DisplayShapes
{
    private static readonly Material matWhite;
    private static readonly Material matRed;
    private static readonly Material matBlue;
    private static readonly Material matYellow;
    private static readonly Material matGray;

    public enum ColorOptions
    {
        White, Red, Blue, Yellow, Gray
    }

    static DisplayShapes()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            Debug.LogError("DisplayShapes: Could not find 'Universal Render Pipeline/Unlit' shader. Add it to Always Included Shaders in Project Settings > Graphics.");
        matWhite = CreateMaterial(shader, Color.white);
        matRed = CreateMaterial(shader, Color.red);
        matBlue = CreateMaterial(shader, Color.blue);
        matYellow = CreateMaterial(shader, Color.yellow);
        matGray = CreateMaterial(shader, Color.gray);
    }

    private static Material CreateMaterial(Shader shader, Color color)
    {
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        return mat;
    }

    public static Material GetMaterial(ColorOptions color)
    {
        return color switch
        {
            (ColorOptions.Red) => matRed,
            (ColorOptions.Blue) => matBlue,
            (ColorOptions.Yellow) => matYellow,
            (ColorOptions.White) => matWhite,
            (ColorOptions.Gray) => matGray,
            _ => matWhite,
        };
    }

    //Draw a line which may consist of several segments, but it has to be connnected into one line
    public static void DrawLine(List<Vector3> vertices, ColorOptions color)
    {
        Material material = GetMaterial(color);
        DrawLine(vertices, material);
    }

    public static void DrawLine(List<Vector3> vertices, Material material)
    {
        if (vertices.Count < 2)
            return;

        List<int> indices = new List<int>();
        for (int i = 0; i < vertices.Count; i++)
        {
            indices.Add(i);
        }

        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetIndices(indices, MeshTopology.LineStrip, 0);
        Graphics.DrawMesh(m, Vector3.zero, Quaternion.identity, material, 0, Camera.main, 0);
    }

    public enum Space2D { XY, XZ, YX };

    public static void DrawCapsule(Vector3 a, Vector3 b, float radius, ColorOptions color)
    {
        //Draw the end points
        DrawCircle(a, radius, color, Space2D.XY);
        DrawCircle(b, radius, color, Space2D.XY);

        //Draw the two lines connecting the end points
        Vector3 vecAB = (a - b).normalized;

        // Build a 2D perpendicular in the XY plane.
        Vector3 normalAB = Utils.PerpendicularXY(vecAB);

        DrawLine(new List<Vector3> { a + normalAB * radius, b + normalAB * radius }, color);
        DrawLine(new List<Vector3> { a - normalAB * radius, b - normalAB * radius }, color);
    }

    public static void DrawCircle(Vector3 circleCenter, float radius, ColorOptions color, Space2D space)
    {
        //Generate the vertices and the indices
        int circleResolution = 100;
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        float angleStep = 360f / circleResolution;
        float angle = 0f;

        for (int i = 0; i < circleResolution + 1; i++)
        {
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3 vertex = new Vector3(x, y, 0f) + circleCenter;

            if (space == Space2D.YX)
                vertex = new Vector3(0f, y, x) + circleCenter;
            else if (space == Space2D.XZ)
                vertex = new Vector3(x, 0f, y) + circleCenter;

            vertices.Add(vertex);
            indices.Add(i);

            angle += angleStep;
        }

        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetIndices(indices, MeshTopology.LineStrip, 0);
        Material material = GetMaterial(color);
        Graphics.DrawMesh(m, Vector3.zero, Quaternion.identity, material, 0, Camera.main, 0);
    }
}
