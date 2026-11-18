using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
	public static GameOverManager Instance { get; private set; }

	[Header("Slow Motion")]
	[SerializeField] [Tooltip("Во сколько раз замедлить игру при аварии (0.2 = в 5 раз медленнее)")] private float slowMotionTimeScale = 0.2f;
	[SerializeField] [Tooltip("Сколько секунд идти в слоу-мо перед полной остановкой")] private float slowMotionDuration = 0.8f;
	[SerializeField] [Tooltip("Длительность плавного затемнения экрана в секундах")] private float fadeDuration = 1f;

	private bool isGameOver = false;
	private Coroutine fadeRoutine;
	private Canvas canvas;
	private CanvasGroup fadeGroup;
	private Text gameOverText;
	private Button restartButton;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		EnsureEventSystem();
	}

	private static void EnsureEventSystem()
	{
		if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
		{
			var esGO = new GameObject("EventSystem");
			esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
			
			// Используем InputSystemUIInputModule для нового Input System
			var inputModule = esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
			
			DontDestroyOnLoad(esGO);
		}
	}

	public void TriggerGameOver()
	{
		if (isGameOver) return;
		isGameOver = true;

		Time.timeScale = slowMotionTimeScale;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;

		if (fadeRoutine != null)
			StopCoroutine(fadeRoutine);
		fadeRoutine = StartCoroutine(GameOverRoutine());
	}

	private IEnumerator GameOverRoutine()
	{
		// Тряска камеры сразу при аварии
		CameraShake.Instance?.Shake(0.5f, 0.4f, 30f);

		yield return new WaitForSecondsRealtime(slowMotionDuration);

		Time.timeScale = 0f;
		Time.fixedDeltaTime = 0f;

		CreateUI();

		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / fadeDuration);
			fadeGroup.alpha = Mathf.Lerp(0f, 1f, t);
			yield return null;
		}
		fadeGroup.alpha = 1f;

		gameOverText.gameObject.SetActive(true);
		restartButton.gameObject.SetActive(true);
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		Time.fixedDeltaTime = 0.02f;
		isGameOver = false;

		if (fadeRoutine != null)
		{
			StopCoroutine(fadeRoutine);
			fadeRoutine = null;
		}

		// Удаляем старый канвас (он с DontDestroyOnLoad, иначе останется после перезагрузки)
		if (canvas != null)
			Destroy(canvas.gameObject);
		canvas = null;
		fadeGroup = null;
		gameOverText = null;
		restartButton = null;

		// Сбрасываем синглтон, чтобы новый GameOverManager в загруженной сцене стал Instance
		Instance = null;

		// Полная перезагрузка текущей сцены
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void CreateUI()
	{
		// Canvas
		var canvasGO = new GameObject("GameOverCanvas");
		canvas = canvasGO.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 200;
		canvasGO.AddComponent<GraphicRaycaster>();
		DontDestroyOnLoad(canvasGO);

		// Fade Panel (black background)
		var fadeGO = new GameObject("FadePanel");
		fadeGO.transform.SetParent(canvas.transform, false);
		var fadeImage = fadeGO.AddComponent<Image>();
		fadeImage.color = Color.black;
		fadeImage.raycastTarget = false; // не блокирует клики
		var fadeRT = fadeGO.GetComponent<RectTransform>();
		fadeRT.anchorMin = Vector2.zero;
		fadeRT.anchorMax = Vector2.one;
		fadeRT.offsetMin = Vector2.zero;
		fadeRT.offsetMax = Vector2.zero;
		fadeGroup = fadeGO.AddComponent<CanvasGroup>();
		fadeGroup.alpha = 0f;
		fadeGroup.blocksRaycasts = false;

		// Text "АВАРИЯ"
		var textGO = new GameObject("GameOverText");
		textGO.transform.SetParent(canvas.transform, false);
		gameOverText = textGO.AddComponent<Text>();
		gameOverText.text = "АВАРИЯ";
		gameOverText.fontSize = 72;
		gameOverText.color = Color.white;
		gameOverText.alignment = TextAnchor.MiddleCenter;
		gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // встроенный шрифт
		gameOverText.resizeTextForBestFit = true;
		gameOverText.resizeTextMinSize = 40;
		gameOverText.resizeTextMaxSize = 100;
		var textRT = textGO.GetComponent<RectTransform>();
		textRT.anchorMin = new Vector2(0.15f, 0.5f);
		textRT.anchorMax = new Vector2(0.85f, 0.7f);
		textRT.offsetMin = Vector2.zero;
		textRT.offsetMax = Vector2.zero;
		gameOverText.gameObject.SetActive(false);

		// Button "НАЧАТЬ ЗАНОВО"
		var btnGO = new GameObject("RestartButton");
		btnGO.transform.SetParent(canvas.transform, false);
		restartButton = btnGO.AddComponent<Button>();
		
		var btnImage = btnGO.AddComponent<Image>();
		btnImage.color = new Color(0.15f, 0.5f, 0.9f, 1f);
		btnImage.raycastTarget = true;
		
		var btnRT = btnGO.GetComponent<RectTransform>();
		btnRT.anchorMin = new Vector2(0.3f, 0.25f);
		btnRT.anchorMax = new Vector2(0.7f, 0.38f);
		btnRT.offsetMin = Vector2.zero;
		btnRT.offsetMax = Vector2.zero;
		btnRT.SetAsLastSibling(); // гарантированно поверх всего

		// Button text
		var btnTextGO = new GameObject("ButtonText");
		btnTextGO.transform.SetParent(btnGO.transform, false);
		var btnText = btnTextGO.AddComponent<Text>();
		btnText.text = "НАЧАТЬ ЗАНОВО";
		btnText.fontSize = 36;
		btnText.color = Color.white;
		btnText.alignment = TextAnchor.MiddleCenter;
		btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		btnText.raycastTarget = false;
		var btnTextRT = btnTextGO.GetComponent<RectTransform>();
		btnTextRT.anchorMin = Vector2.zero;
		btnTextRT.anchorMax = Vector2.one;
		btnTextRT.offsetMin = Vector2.zero;
		btnTextRT.offsetMax = Vector2.zero;

		// Навигация кнопки — явная, чтобы работала на мобильных
		restartButton.navigation = new Navigation { mode = Navigation.Mode.None };
		restartButton.transition = Selectable.Transition.ColorTint;
		var colors = restartButton.colors;
		colors.normalColor = new Color(0.15f, 0.5f, 0.9f, 1f);
		colors.highlightedColor = new Color(0.25f, 0.6f, 1f, 1f);
		colors.pressedColor = new Color(0.1f, 0.4f, 0.7f, 1f);
		colors.selectedColor = new Color(0.15f, 0.5f, 0.9f, 1f);
		restartButton.colors = colors;

		restartButton.onClick.RemoveAllListeners();
		restartButton.onClick.AddListener(RestartGame);
		restartButton.gameObject.SetActive(false);
	}
}