using System.Collections;
using UnityEngine;

public class TrafficCar : MonoBehaviour
{
	[Header("Movement")]
	public float speed = 10f;
	public int laneIndex = 1;

	[Header("Crash Effect")]
	[SerializeField] private float crashSpinSpeed = 270f;
	[SerializeField] private float bounceBackDistance = 2f;
	[SerializeField] private float crashDuration = 1f;

	private bool isCrashed = false;
	private Coroutine crashRoutine;

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
		}
		// Автоматически подгоняем коллайдер под визуал (MeshRenderer)
		FitColliderToVisual(collider);

		// Rigidbody не нужен — движемся через transform, коллизии обрабатываем вручную
		var rb = GetComponent<Rigidbody>();
		if (rb != null)
			Destroy(rb);

		isCrashed = false;
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

	private void Update()
	{
		if (isCrashed)
			return;

		// Едем навстречу игроку (дорога скроллит к нему, мы едем в ту же сторону по -Z)
		transform.position += Vector3.back * speed * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log($"[TrafficCar] OnTriggerEnter: other={other.name}, tag={other.tag}, layer={other.gameObject.layer}");
		
		if (!other.CompareTag("Player"))
		{
			Debug.Log($"[TrafficCar] Not Player tag, skipping");
			return;
		}

		// Авария только если мы на той же полосе, что и игрок
		var playerController = other.GetComponent<CarController>();
		if (playerController != null && playerController.CurrentLane != laneIndex)
		{
			Debug.Log($"[TrafficCar] Different lane: traffic={laneIndex}, player={playerController.CurrentLane}");
			return;
		}

		if (isCrashed)
			return;

		Debug.Log($"[TrafficCar] COLLISION! Starting crash effect");
		StartCrashEffect();
		// GameOverManager вызовет CarController, не дублируем здесь
	}

	private void StartCrashEffect()
	{
		isCrashed = true;

		// Отключаем коллайдер, чтобы не было повторных срабатываний
		var collider = GetComponent<BoxCollider>();
		if (collider != null)
			collider.enabled = false;

		if (crashRoutine != null)
			StopCoroutine(crashRoutine);
		crashRoutine = StartCoroutine(CrashRoutine());
	}

	private IEnumerator CrashRoutine()
	{
		float spinDirection = Random.value > 0.5f ? 1f : -1f;
		float elapsed = 0f;

		// Небольшой "отскок" назад при ударе
		Vector3 startPos = transform.position;
		Vector3 targetPos = startPos + Vector3.forward * bounceBackDistance;

		while (elapsed < crashDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / crashDuration);
			
			// Ease out для плавной остановки
			float easedT = 1f - Mathf.Pow(1f - t, 3f);
			
			// Отскок назад
			transform.position = Vector3.Lerp(startPos, targetPos, easedT);
			
			// Вращение
			float currentSpinSpeed = Mathf.Lerp(crashSpinSpeed, 0f, easedT);
			transform.Rotate(0f, currentSpinSpeed * spinDirection * Time.unscaledDeltaTime, 0f, Space.World);
			
			// Микро-дрожание
			transform.position += new Vector3(
				Mathf.Sin(elapsed * 25f) * 0.015f * (1f - t),
				0f,
				Mathf.Cos(elapsed * 18f) * 0.01f * (1f - t)
			);

			yield return null;
		}
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