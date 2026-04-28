using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//General class to grab objects with mouse and throw them around
public class Grabber
{
    private readonly Camera mainCamera;
    private IGrabbable grabbedBody;

    //Mesh grabbing data
    //When we have grabbed a mesh by using ray-triangle intersection we identify the closest vertex. The distance from camera to this vertex is constant so we can move it around without doing another ray-triangle intersection
    private float distanceToGrabPos;

    //To give the mesh a velocity when we release it
    private Vector3 lastGrabPos;
    private readonly float releaseVelocityScale;

    public Grabber(Camera mainCamera, float releaseVelocityScale = 1f)
    {
        this.mainCamera = mainCamera;
        this.releaseVelocityScale = releaseVelocityScale;
    }

    public void StartGrab(List<IGrabbable> bodies)
    {
        if (grabbedBody != null)
            return;

        //A ray from the mouse into the scene
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        float maxDist = float.MaxValue;
        IGrabbable closestBody = null;
        CustomHit closestHit = null;

        foreach (IGrabbable body in bodies)
        {
            body.IsRayHittingBody(ray, out CustomHit hit);

            if (hit == null) continue;
            if (!(hit.distance < maxDist)) continue;
            closestBody = body;
            maxDist = hit.distance;
            closestHit = hit;
        }

        if (closestBody != null)
        {
            grabbedBody = closestBody;

            //StartGrab is finding the closest vertex and setting it to the position where the ray hit the triangle
            closestBody.StartGrab(closestHit.location);
            lastGrabPos = closestHit.location;

            //distanceToGrabPos = (ray.origin - hit.location).magnitude;
            distanceToGrabPos = closestHit.distance;
        }
    }

    public void MoveGrab()
    {
        if (grabbedBody == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 vertexPos = ray.origin + ray.direction * distanceToGrabPos;

        lastGrabPos = grabbedBody.GetGrabbedPos();

        grabbedBody.MoveGrabbed(vertexPos);
    }

    public void EndGrab()
    {
        if (grabbedBody == null)
            return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 screenDelta = Mouse.current.delta.ReadValue();

        Ray rayNow = mainCamera.ScreenPointToRay(screenPos);
        Vector3 grabPos = rayNow.origin + rayNow.direction * distanceToGrabPos;

        float dt = Mathf.Max(Time.deltaTime, 0.000001f);

        // World velocity from cursor motion: same grab depth, previous vs current screen ray.
        Vector3 worldVel;
        if (screenDelta.sqrMagnitude > 0.01f)
        {
            Ray rayPrev = mainCamera.ScreenPointToRay(screenPos - screenDelta);
            Vector3 prevGrabPos = rayPrev.origin + rayPrev.direction * distanceToGrabPos;
            worldVel = (grabPos - prevGrabPos) / dt;
        }
        else
        {
            worldVel = (grabPos - lastGrabPos) / dt;
        }

        worldVel *= releaseVelocityScale;

        grabbedBody.EndGrab(grabPos, worldVel);
        grabbedBody = null;
    }
}
