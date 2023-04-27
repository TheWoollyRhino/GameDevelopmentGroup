using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//made by Amrit Chatha

public class SplineTrigger : MonoBehaviour
{
    public Camera playerCamera;
    private bool inTrigger;
    public SplineCamera splineCamera;
    public BetterPlayerMovement playerController;

    public BezierSpline spline;
    public GameObject playerTarget;

    private void Awake()
    {

    }

    private void LateUpdate()
    {
        if (inTrigger)
        {
            // move the players camera to start of the spline
            playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position, spline.GetPoint(splineCamera.progress), Time.deltaTime * 10);

            // look at player
            playerCamera.transform.LookAt(playerTarget.transform);
        }
        else
        {
            // look at player
            playerCamera.transform.LookAt(playerTarget.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inTrigger = true;
            splineCamera.splineCamera.enabled = true;
            playerCamera.enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inTrigger = false;
            splineCamera.splineCamera.enabled = false;
            playerCamera.enabled = true;
        }
    }
}
