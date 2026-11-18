using UnityEngine;

public class TrafficCar : MonoBehaviour
{
	public float speed = 10f;
	public int laneIndex = 1;

	private void OnEnable()
	{
		// Назначаем тег Traffic, если он существует в проекте; иначе пропускаем,
		// чтобы не падать с ошибкой "Tag is not defined".
		if (IsTagDefined("Traffic"))
			gameObject.tag = "Traffic";

		var collider = GetComponent<BoxCollider>();
		if (collider == null)
		{
			collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.size = new Vector3(1.2f, 1.4f, 2.5f);
			collider.center = new Vector3(0f, 0.7f, 0f);
		}

		var rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.useGravity = false;
		}
	}

	private void Update()
	{
		// Едем навстречу игроку (дорога скроллит к нему, мы едем в ту же сторону по -Z)
		transform.position += Vector3.back * speed * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player"))
			return;

		// Авария только если мы на той же полосе, что и игрок
		var playerController = other.GetComponent<CarController>();
		if (playerController != null && playerController.CurrentLane != laneIndex)
			return;

		GameOverManager.Instance?.TriggerGameOver();
	}

	private static bool IsTagDefined(string tagName)
	{
		// Unity не предоставляет публичного API для проверки существования тега,
		// поэтому пробуем присвоить временный тег в try-catch.
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
