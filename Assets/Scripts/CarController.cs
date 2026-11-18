using System.Collections;
using UnityEngine;

public class CarController : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float forwardSpeed = 12f;
	[SerializeField] [Tooltip("Скорость перестроения между полосами. Чем больше — тем быстрее машина уходит на соседнюю полосу.")] private float laneChangeSpeed = 10f;
	[SerializeField] [Tooltip("Максимальное отклонение от центра влево/вправо. Ограничивает, насколько далеко машина может уйти в сторону.")] private float maxSideOffset = 10f;

	[Header("Lanes")]
	[SerializeField] private int laneCount = 3;
	[SerializeField] private float laneWidth = 3.5f;

	[Header("Visual")]
	[SerializeField] [Tooltip("Смещение визуала машины относительно центра полосы, если pivot модели в FBX не по центру.")] private float visualCenterOffset = 0f;
	[SerializeField] [Tooltip("Автоматически сдвигать машину при старте так, чтобы центр её визуала (MeshRenderer) совпадал с целевой полосой.")] private bool autoCenterToVisual = true;

	[Header("Crash Effect")]
	[SerializeField] [Tooltip("Сила разворота машины при аварии (градусов в секунду)")] private float crashSpinSpeed = 360f;
	[SerializeField] [Tooltip("Случайное направление разворота")] private bool randomSpinDirection = true;

	private int currentLaneIndex;
	private float targetX;

	private SwipeInput subscribedInput;
	private bool isCrashed = false;
	private Coroutine crashRoutine;

	private void Awake()
	{
		currentLaneIndex = laneCount / 2;
		targetX = GetLaneX(currentLaneIndex);
		targetX = Mathf.Clamp(targetX, -maxSideOffset, maxSideOffset);

		// Назначаем тег Player, если он существует в проекте; иначе пропускаем,
		// чтобы не падать с ошибкой "Tag is not defined".
		if (IsTagDefined("Player"))
			gameObject.tag = "Player";

		var collider = GetComponent<BoxCollider>();
		if (collider == null)
		{
			collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
		}
		// Автоматически подгоняем коллайдер под визуал (MeshRenderer)
		FitColliderToVisual(collider);

		// Rigidbody нужен для работы OnTriggerEnter — без него триггеры не срабатывают.
		// Ставим isKinematic = true, так как машину двигаем через transform.position, а не физику.
		var rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.useGravity = false;
		}

		Vector3 pos = transform.position;
		pos.x = targetX + visualCenterOffset;
		transform.position = pos;

		if (autoCenterToVisual)
			CenterToVisual();
	}

	private void CenterToVisual()
	{
		var renderers = GetComponentsInChildren<MeshRenderer>();
		if (renderers.Length == 0)
			return;

		Bounds bounds = renderers[0].bounds;
		for (int i = 1; i < renderers.Length; i++)
			bounds.Encapsulate(renderers[i].bounds);

		float correctionX = targetX + visualCenterOffset - bounds.center.x;
		if (Mathf.Approximately(correctionX, 0f))
			return;

		Vector3 pos = transform.position;
		pos.x += correctionX;
		transform.position = pos;
	}

	private void FitColliderToVisual(BoxCollider collider)
	{
		var renderers = GetComponentsInChildren<MeshRenderer>();
		if (renderers.Length == 0)
			return;

		Bounds bounds = renderers[0].bounds;
		for (int i = 1; i < renderers.Length; i++)
			bounds.Encapsulate(renderers[i].bounds);

		// Переводим bounds в локальное пространство объекта
		Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
		Vector3 localSize = bounds.size;

		// Небольшой запас (5%), чтобы коллайдер был чуть больше визуала
		const float padding = 1.05f;
		collider.center = localCenter;
		collider.size = localSize * padding;
	}

	private void OnEnable()
	{
		ConnectInput();
	}

	private void OnDisable()
	{
		if (subscribedInput != null)
		{
			subscribedInput.OnSwipeLeft -= SwipeLeft;
			subscribedInput.OnSwipeRight -= SwipeRight;
		}
		subscribedInput = null;
	}

	private void Update()
	{
		if (isCrashed)
			return;

		if (subscribedInput == null)
			ConnectInput();
		UpdateLanePosition();
	}

	private void ConnectInput()
	{
		if (subscribedInput != null || SwipeInput.Instance == null)
			return;
		subscribedInput = SwipeInput.Instance;
		subscribedInput.OnSwipeLeft += SwipeLeft;
		subscribedInput.OnSwipeRight += SwipeRight;
	}

	private void SwipeLeft() => ChangeLane(-1);
	private void SwipeRight() => ChangeLane(1);

	private void UpdateLanePosition()
	{
		Vector3 pos = transform.position;
		float newX = Mathf.MoveTowards(pos.x, targetX, laneChangeSpeed * Time.deltaTime);
		pos.x = newX;
		transform.position = pos;
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log($"[CarController] OnTriggerEnter: other={other.name}, tag={other.tag}, layer={other.gameObject.layer}");
		
		if (!other.CompareTag("Traffic"))
		{
			Debug.Log($"[CarController] Not Traffic tag, skipping");
			return;
		}

		var traffic = other.GetComponent<TrafficCar>();
		if (traffic != null && traffic.laneIndex != currentLaneIndex)
		{
			Debug.Log($"[CarController] Different lane: traffic={traffic.laneIndex}, player={currentLaneIndex}");
			return;
		}

		if (isCrashed)
			return;

		Debug.Log($"[CarController] COLLISION! Starting crash effect");
		StartCrashEffect();
		GameOverManager.Instance?.TriggerGameOver();
	}

	private void StartCrashEffect()
	{
		isCrashed = true;
		
		// Отключаем ввод
		if (subscribedInput != null)
		{
			subscribedInput.OnSwipeLeft -= SwipeLeft;
			subscribedInput.OnSwipeRight -= SwipeRight;
			subscribedInput = null;
		}

		// Коллайдер больше не нужен (чтобы не было повторных срабатываний)
		var collider = GetComponent<BoxCollider>();
		if (collider != null)
			collider.enabled = false;

		// Запускаем корутину разворота
		if (crashRoutine != null)
			StopCoroutine(crashRoutine);
		crashRoutine = StartCoroutine(CrashSpinRoutine());
	}

	private IEnumerator CrashSpinRoutine()
	{
		float spinDirection = randomSpinDirection ? (Random.value > 0.5f ? 1f : -1f) : 1f;
		float elapsed = 0f;
		float duration = 1.5f; // длительность анимации разворота

		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime; // используем unscaledTime, так как Time.timeScale будет меняться
			float t = Mathf.Clamp01(elapsed / duration);
			
			// Эффект замедления вращения к концу (ease out)
			float easedT = 1f - Mathf.Pow(1f - t, 3f);
			float currentSpinSpeed = Mathf.Lerp(crashSpinSpeed, 0f, easedT);
			
			transform.Rotate(0f, currentSpinSpeed * spinDirection * Time.unscaledDeltaTime, 0f, Space.World);
			
			// Небольшой "дрожание" позиции для реалистичности
			transform.position += new Vector3(
				Mathf.Sin(elapsed * 20f) * 0.02f * (1f - t),
				0f,
				Mathf.Cos(elapsed * 15f) * 0.01f * (1f - t)
			);

			yield return null;
		}
	}

	private float GetLaneX(int index)
	{
		float totalWidth = (laneCount - 1) * laneWidth;
		float startX = -totalWidth * 0.5f;
		return startX + index * laneWidth;
	}

	public void ChangeLane(int direction)
	{
		int newLane = currentLaneIndex + direction;
		newLane = Mathf.Clamp(newLane, 0, laneCount - 1);

		if (newLane != currentLaneIndex)
		{
			currentLaneIndex = newLane;
			targetX = GetLaneX(currentLaneIndex);
			targetX = Mathf.Clamp(targetX, -maxSideOffset, maxSideOffset);
		}
	}

	public void ResetPosition()
	{
		// Сброс состояния аварии
		isCrashed = false;
		if (crashRoutine != null)
		{
			StopCoroutine(crashRoutine);
			crashRoutine = null;
		}

		// Восстанавливаем поворот
		transform.rotation = Quaternion.identity;

		// Включаем коллайдер обратно
		var collider = GetComponent<BoxCollider>();
		if (collider != null)
			collider.enabled = true;

		// Переподключаем ввод
		ConnectInput();

		currentLaneIndex = laneCount / 2;
		targetX = GetLaneX(currentLaneIndex);
		targetX = Mathf.Clamp(targetX, -maxSideOffset, maxSideOffset);
		Vector3 pos = transform.position;
		pos.x = targetX + visualCenterOffset;
		transform.position = pos;
	}

	public int CurrentLane => currentLaneIndex;
	public float ForwardSpeed => forwardSpeed;

	private static bool IsTagDefined(string tagName)
	{
		var go = new GameObject();
		bool result = false;
		try
		{
			go.tag = tagName;
			result = true;
		}
		catch (UnityEngine.UnityException)
		{
			result = false;
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
		return result;
	}
}