using UnityEngine;

public class CarHeadlights : MonoBehaviour
{
	[Header("Фары машины")]
	[SerializeField] private Light[] headlights;

	[Header("Солнце (направленный свет)")]
	[SerializeField] private Light sunLight;

	[Header("Пороги")]
	[SerializeField] [Tooltip("Угол Солнца ниже которого считается ночью — фары включаются.")] private float nightAngleThreshold = 0f;

	private bool isOn;

	private void Awake()
	{
		// Если солнце не назначено в инспекторе, ищем направленный свет на сцене
		if (sunLight == null)
		{
			var lights = FindObjectsOfType<Light>();
			foreach (var l in lights)
			{
				if (l.type == LightType.Directional)
				{
					sunLight = l;
					break;
				}
			}
		}
	}

	private void Update()
	{
		if (sunLight == null || headlights == null || headlights.Length == 0)
			return;

		float angle = sunLight.transform.localEulerAngles.x;
		// Переводим из диапазона 0..360 в -180..180
		if (angle > 180f)
			angle -= 360f;

		bool isNight = angle < nightAngleThreshold;

		if (isNight != isOn)
		{
			isOn = isNight;
			SetHeadlights(isOn);
		}
	}

	private void SetHeadlights(bool on)
	{
		foreach (var h in headlights)
		{
			if (h != null)
				h.enabled = on;
		}
	}
}
