using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
	public static GameOverManager Instance { get; private set; }

	[Header("UI")]
	[SerializeField] private Canvas gameOverCanvas;
	[SerializeField] private Text gameOverText;
	[SerializeField] private Button restartButton;

	[Header("Slow Motion")]
	[SerializeField] private float slowMotionTimeScale = 0.2f;

	private bool isGameOver = false;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		if (gameOverCanvas != null)
			gameOverCanvas.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (isGameOver)
			Time.timeScale = slowMotionTimeScale;
	}

	public void TriggerGameOver()
	{
		if (isGameOver) return;
		isGameOver = true;
		Time.timeScale = slowMotionTimeScale;

		if (gameOverCanvas != null)
			gameOverCanvas.gameObject.SetActive(true);
		else
			CreateDefaultUI();
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		isGameOver = false;

		// Сброс позиции игрока на стартовую полосу
		var player = FindObjectOfType<CarController>();
		if (player != null)
		{
			player.ResetPosition();
		}

		// Удаляем все машины трафика
		foreach (var car in FindObjectsOfType<TrafficCar>())
			Destroy(car.gameObject);

		if (gameOverCanvas != null)
			gameOverCanvas.gameObject.SetActive(false);
	}

	private void CreateDefaultUI()
	{
		var go = new GameObject("GameOverCanvas");
		var canvas = go.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		gameOverCanvas = canvas;

		var panel = new GameObject("Panel");
		panel.transform.SetParent(canvas.transform, false);
		var img = panel.AddComponent<Image>();
		img.color = new Color(0f, 0f, 0f, 0.7f);
		var prt = panel.GetComponent<RectTransform>();
		prt.anchorMin = Vector2.zero;
		prt.anchorMax = Vector2.one;
		prt.offsetMin = Vector2.zero;
		prt.offsetMax = Vector2.zero;

		var textGO = new GameObject("Text");
		textGO.transform.SetParent(panel.transform, false);
		var text = textGO.AddComponent<Text>();
		text.text = "АВАРИЯ";
		text.fontSize = 50;
		text.color = Color.red;
		text.alignment = TextAnchor.MiddleCenter;
		var trt = text.GetComponent<RectTransform>();
		trt.anchorMin = new Vector2(0.2f, 0.45f);
		trt.anchorMax = new Vector2(0.8f, 0.55f);
		trt.offsetMin = Vector2.zero;
		trt.offsetMax = Vector2.zero;
		gameOverText = text;

		var btnGO = new GameObject("Button");
		btnGO.transform.SetParent(panel.transform, false);
		var btn = btnGO.AddComponent<Button>();
		var btnTextGO = new GameObject("Text");
		btnTextGO.transform.SetParent(btnGO.transform, false);
		var btnText = btnTextGO.AddComponent<Text>();
		btnText.text = "НАЧАТЬ ЗАНОВО";
		btnText.fontSize = 30;
		btnText.color = Color.white;
		btnText.alignment = TextAnchor.MiddleCenter;
		var btrt = btnTextGO.GetComponent<RectTransform>();
		btrt.anchorMin = Vector2.zero;
		btrt.anchorMax = Vector2.one;
		btrt.offsetMin = Vector2.zero;
		btrt.offsetMax = Vector2.zero;

		var brt = btnGO.GetComponent<RectTransform>();
		brt.anchorMin = new Vector2(0.35f, 0.3f);
		brt.anchorMax = new Vector2(0.65f, 0.4f);
		brt.offsetMin = Vector2.zero;
		brt.offsetMax = Vector2.zero;

		btn.onClick.AddListener(RestartGame);
		restartButton = btn;
	}
}
