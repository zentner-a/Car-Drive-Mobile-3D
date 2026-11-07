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

	private int currentLaneIndex;
	private float targetX;

	private System.Action onSwipeLeft;
	private System.Action onSwipeRight;

	private void Awake()
	{
		currentLaneIndex = laneCount / 2;
		targetX = GetLaneX(currentLaneIndex);
		targetX = Mathf.Clamp(targetX, -maxSideOffset, maxSideOffset);

		Vector3 pos = transform.position;
		pos.x = targetX;
		transform.position = pos;

		Debug.Log($"[CarController] Awake. Start lane: {currentLaneIndex}, X: {targetX}");
	}

	private void OnEnable()
	{
		if (SwipeInput.Instance != null)
		{
			onSwipeLeft = () => ChangeLane(-1);
			onSwipeRight = () => ChangeLane(1);
			SwipeInput.Instance.OnSwipeLeft += onSwipeLeft;
			SwipeInput.Instance.OnSwipeRight += onSwipeRight;
			Debug.Log("[CarController] Subscribed to SwipeInput events");
		}
		else
		{
			Debug.LogWarning("[CarController] SwipeInput.Instance is NULL! Place SwipeInput on an active GameObject in the scene.");
		}
	}

	private void OnDisable()
	{
		if (SwipeInput.Instance != null)
		{
			SwipeInput.Instance.OnSwipeLeft -= onSwipeLeft;
			SwipeInput.Instance.OnSwipeRight -= onSwipeRight;
		}
	}

	private void Update()
	{
		MoveForward();
		UpdateLanePosition();
	}

	private void MoveForward()
	{
		transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
	}

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

			Debug.Log($"[CarController] ChangeLane to {currentLaneIndex}, targetX: {targetX}");
		}
	}

	public int CurrentLane => currentLaneIndex;
	public float ForwardSpeed => forwardSpeed;
}