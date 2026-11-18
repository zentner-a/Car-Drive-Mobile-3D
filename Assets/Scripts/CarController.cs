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

	private int currentLaneIndex;
	private float targetX;

	private SwipeInput subscribedInput;

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
			collider.size = new Vector3(0.9f, 1f, 1.8f);
			collider.center = new Vector3(0f, 0.8f, 0f);
		}

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

	public int CurrentLane => currentLaneIndex;
	public float ForwardSpeed => forwardSpeed;

	public void ResetPosition()
	{
		currentLaneIndex = laneCount / 2;
		targetX = GetLaneX(currentLaneIndex);
		targetX = Mathf.Clamp(targetX, -maxSideOffset, maxSideOffset);
		Vector3 pos = transform.position;
		pos.x = targetX + visualCenterOffset;
		transform.position = pos;
	}

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
