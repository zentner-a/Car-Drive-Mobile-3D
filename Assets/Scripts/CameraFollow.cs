using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("Target")]
	[SerializeField] private Transform target;

	[Header("Offset")]
	[SerializeField] private Vector3 baseOffset = new Vector3(0f, 2f, -5f);
	[SerializeField] [Tooltip("Additional distance along Z axis (negative = further back)")] private float extraDistance = 0f;

	[Header("Smoothing")]
	[SerializeField] private float smoothSpeed = 5f;

	private void LateUpdate()
	{
		if (target == null)
		{
			return;
		}

		Vector3 offset = baseOffset;
		offset.z += extraDistance;

		Vector3 desiredPosition = target.position + offset;
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
		transform.position = smoothedPosition;

		transform.LookAt(target);
	}
}