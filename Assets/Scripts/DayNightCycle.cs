using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
	[Header("Sun")]
	[SerializeField] private Transform sunTransform;
	[SerializeField] private Light sunLight;

	[Header("Cycle speed")]
	[SerializeField] [Tooltip("Длительность полного цикла суток в секундах. Чем меньше — тем быстрее смена дня и ночи.")] private float cycleDuration = 120f;

	[Header("Colors")]
	[SerializeField] private Color noonColor = Color.white;
	[SerializeField] private Color sunriseSunsetColor = new Color(1f, 0.55f, 0.3f);
	[SerializeField] private Color nightColor = new Color(0.15f, 0.2f, 0.35f);

	[Header("Ambient & Fog")]
	[SerializeField] private Color noonAmbient = new Color(0.4f, 0.4f, 0.4f);
	[SerializeField] private Color nightAmbient = new Color(0.05f, 0.05f, 0.08f);
	[SerializeField] private bool updateFogColor = true;

	[Header("Intensity")]
	[SerializeField] private float noonIntensity = 1f;
	[SerializeField] [Range(0f, 1f)] private float nightIntensity = 0.1f;

	[Range(0f, 1f)]
	[SerializeField] private float timeOfDay = 0.5f;

	private void OnValidate()
	{
		if (sunTransform == null && sunLight != null)
			sunTransform = sunLight.transform;
		if (sunLight == null && sunTransform != null)
			sunLight = sunTransform.GetComponent<Light>();
	}

	private void Awake()
	{
		if (sunTransform == null && sunLight != null)
			sunTransform = sunLight.transform;
		if (sunLight == null && sunTransform != null)
			sunLight = sunTransform.GetComponent<Light>();
	}

	private void Update()
	{
		if (cycleDuration <= 0f)
			return;

		timeOfDay += Time.deltaTime / cycleDuration;
		if (timeOfDay >= 1f)
			timeOfDay -= 1f;

		ApplyTimeOfDay();
	}

	private void ApplyTimeOfDay()
	{
		// t: 0 = полночь, 0.25 = рассвет, 0.5 = полдень, 0.75 = закат, 1 = полночь
		float t = timeOfDay * 2f * Mathf.PI;

		// Угол солнца над горизонтом: -90° (полночь) .. +90° (полдень)
		float elevation = Mathf.Sin(t) * 90f;

		if (sunTransform != null)
			sunTransform.localRotation = Quaternion.Euler(elevation, sunTransform.localEulerAngles.y, sunTransform.localEulerAngles.z);

		if (sunLight != null)
		{
			// Фактор daylight: 0 ночью, 1 в полдень
			float dayFactor = Mathf.Max(0f, Mathf.Sin(t));

			// Цвет света: ночь → день, с тёплым оттенком на рассвете/закате
			Color baseColor = Color.Lerp(nightColor, noonColor, dayFactor);
			float horizonWarmth = 1f - Mathf.Abs(Mathf.Sin(t)); // 1 на горизонте, 0 в зените и под землей
			sunLight.color = Color.Lerp(baseColor, sunriseSunsetColor, horizonWarmth * 0.7f);

			sunLight.intensity = Mathf.Lerp(nightIntensity, noonIntensity, dayFactor);
		}

		// Освещение сцены и туман
		RenderSettings.ambientLight = Color.Lerp(nightAmbient, noonAmbient, Mathf.Max(0f, Mathf.Sin(t)));
		if (updateFogColor)
			RenderSettings.fogColor = RenderSettings.ambientLight;
	}
}
