using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public static CameraShake Instance { get; private set; }

	[Header("Настройки тряски")]
	[SerializeField] private float defaultDuration = 0.4f;
	[SerializeField] private float defaultMagnitude = 0.3f;
	[SerializeField] private float defaultFrequency = 25f;

	private Vector3 originalPosition;
	private Coroutine shakeRoutine;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Start()
	{
		originalPosition = transform.localPosition;
	}

	public void Shake(float duration = -1f, float magnitude = -1f, float frequency = -1f)
	{
		if (shakeRoutine != null)
			StopCoroutine(shakeRoutine);

		shakeRoutine = StartCoroutine(ShakeRoutine(
			duration > 0 ? duration : defaultDuration,
			magnitude > 0 ? magnitude : defaultMagnitude,
			frequency > 0 ? frequency : defaultFrequency
		));
	}

	private IEnumerator ShakeRoutine(float duration, float magnitude, float frequency)
	{
		float elapsed = 0f;
		originalPosition = transform.localPosition;

		while (elapsed < duration)
		{
			float t = elapsed / duration;
			// Затухание амплитуды к концу
			float damper = 1f - t;

			float x = Mathf.PerlinNoise(Time.unscaledTime * frequency, 0f) * 2f - 1f;
			float y = Mathf.PerlinNoise(0f, Time.unscaledTime * frequency) * 2f - 1f;
			float z = Mathf.PerlinNoise(Time.unscaledTime * frequency, Time.unscaledTime * frequency) * 2f - 1f;

			Vector3 offset = new Vector3(x, y, z) * magnitude * damper;
			transform.localPosition = originalPosition + offset;

			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}

		transform.localPosition = originalPosition;
		shakeRoutine = null;
	}

	public void StopShake()
	{
		if (shakeRoutine != null)
		{
			StopCoroutine(shakeRoutine);
			shakeRoutine = null;
		}
		transform.localPosition = originalPosition;
	}
}