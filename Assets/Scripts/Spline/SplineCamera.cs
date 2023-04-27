using UnityEngine;

//made by Amrit Chatha

public class SplineCamera : MonoBehaviour
{
	public Camera splineCamera;
	public Camera playerCamera;

	public GameObject playerTarget;
	public BezierSpline spline;
	public float duration = 10;

	[HideInInspector] public float progress;
	private bool goingForward = true;

	private void Awake()
	{
	}	

	private void LateUpdate()
	{
		if (goingForward)
		{
			progress += Time.deltaTime / duration;
			if (progress >= 1)
			{
				progress = 2 - progress;
				goingForward = false;
			}
		}
		else
		{
			progress -= Time.deltaTime / duration;
			if (progress <= 0)
			{
				progress = -progress;
				goingForward = true;
			}
		}

		Vector3 position = spline.GetPoint(progress);
		transform.localPosition = position;
		transform.LookAt(playerTarget.transform);
	}
}