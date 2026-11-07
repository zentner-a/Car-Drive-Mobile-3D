using UnityEngine;

[ExecuteAlways]
public class PortraitAspectEnforcer : MonoBehaviour
{
	[SerializeField] private Camera targetCamera;
	[SerializeField] private float portraitAspect = 9f / 16f; // 9:16 portrait

	private void Reset()
	{
		targetCamera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		if (targetCamera == null) targetCamera = GetComponent<Camera>();
		ApplyPortraitAspect();
	}

	private void Update()
	{
#if UNITY_EDITOR
		ApplyPortraitAspect();
#endif
	}

	private void ApplyPortraitAspect()
	{
		if (targetCamera != null)
		{
			targetCamera.aspect = portraitAspect;
		}
	}
}