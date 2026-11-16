using UnityEngine;

public class FpsCounter : MonoBehaviour
{
	[Header("Отображение")]
	[SerializeField] private bool showFps = true;
	[SerializeField] private int fontSize = 20;
	[SerializeField] private Color textColor = Color.green;
	[SerializeField] private Vector2 position = new Vector2(10f, 10f);

	[Header("Расчёт")]
	[SerializeField] private float updateInterval = 0.5f;

	private float accum = 0f;
	private int frames = 0;
	private float timeleft;
	private float fps;

	private void Start()
	{
		timeleft = updateInterval;
	}

	private void Update()
	{
		if (!showFps)
			return;

		timeleft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		++frames;

		if (timeleft <= 0.0f)
		{
			fps = accum / frames;
			timeleft = updateInterval;
			accum = 0f;
			frames = 0;
		}
	}

	private void OnGUI()
	{
		if (!showFps)
			return;

		var style = new GUIStyle
		{
			fontSize = fontSize,
			normal = { textColor = textColor },
			alignment = TextAnchor.UpperLeft
		};

		GUI.Label(new Rect(position.x, position.y, 200, fontSize), $"FPS: {fps:F1}", style);
	}
}
