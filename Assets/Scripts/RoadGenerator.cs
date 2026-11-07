using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject roadSegmentPrefab;
	[SerializeField] private Transform car;

	[Header("Settings")]
	[SerializeField] private float segmentLength = 8f;
	[SerializeField] private int segmentsAhead = 10;
	[SerializeField] private float removeBehindDistance = 20f;

	[Header("Spawn rotation (если дорога стоит ребром — выставите X = 90 или -90)")]
	[SerializeField] private Vector3 segmentRotation = new Vector3(90f, 0f, 0f);

	private readonly Queue<GameObject> activeSegments = new Queue<GameObject>();
	private float nextSpawnZ;
	private GameObject templateSegment;
	private Vector3 segmentPositionOffset;

	private void Awake()
	{
		Debug.Log("[RoadGenerator] Awake started");

		if (car == null)
		{
			GameObject carGo = GameObject.Find("car");
			if (carGo != null)
			{
				car = carGo.transform;
				Debug.Log("[RoadGenerator] Found car by name: car");
			}
			else
			{
				Debug.LogWarning("[RoadGenerator] Car reference is NULL and could not find 'car' object!");
			}
		}

		// Всегда деактивируем оригинальную road1 на сцене, чтобы не мешала
		GameObject originalRoad = GameObject.Find("road1");
		if (originalRoad != null)
		{
			originalRoad.SetActive(false);
			Debug.Log("[RoadGenerator] Deactivated original road1");

			// Запоминаем смещение оригинальной дороги, чтобы клоны появлялись на той же высоте
			if (originalRoad.transform != null)
			{
				segmentPositionOffset = Vector3.zero;
				segmentPositionOffset.y = originalRoad.transform.position.y;
				Debug.Log($"[RoadGenerator] Road position offset: {segmentPositionOffset}");
			}
		}
		else
		{
			Debug.LogWarning("[RoadGenerator] Could not find 'road1' object on scene!");
		}

		if (roadSegmentPrefab == null)
		{
			roadSegmentPrefab = originalRoad;
			Debug.Log("[RoadGenerator] roadSegmentPrefab was null, using original road1");
		}

		templateSegment = roadSegmentPrefab;

		if (templateSegment == null)
		{
			Debug.LogError("[RoadGenerator] templateSegment is NULL! No road prefab to spawn. Assign Road Segment Prefab in inspector.");
		}
		else
		{
			Debug.Log($"[RoadGenerator] templateSegment set: {templateSegment.name}");
		}

		if (car != null)
		{
			nextSpawnZ = car.position.z;
			Debug.Log($"[RoadGenerator] nextSpawnZ initialized to car Z: {nextSpawnZ}");
		}
		else
		{
			nextSpawnZ = 0f;
			Debug.Log("[RoadGenerator] Car is null, nextSpawnZ = 0");
		}

		for (int i = 0; i < segmentsAhead; i++)
		{
			SpawnSegment();
			Debug.Log($"[RoadGenerator] Initial spawn {i + 1}/{segmentsAhead}, nextSpawnZ now: {nextSpawnZ}");
		}

		Debug.Log($"[RoadGenerator] Awake complete. Active segments: {activeSegments.Count}");
	}

	private void Update()
	{
		if (car == null)
		{
			return;
		}

		float carZ = car.position.z;

		// Спавним новые сегменты когда машина приближается к краю текущей дороги
		// nextSpawnZ - это Z следующего сегмента для спавна
		// Хотим чтобы впереди машины всегда было segmentsAhead сегментов
		float maxRoadZ = nextSpawnZ - segmentLength; // Z последнего заспавненного сегмента
		while (maxRoadZ < carZ + segmentLength * segmentsAhead)
		{
			SpawnSegment();
			maxRoadZ = nextSpawnZ - segmentLength;
		}

		RemoveOldSegment(carZ);
	}

	private void SpawnSegment()
	{
		if (templateSegment == null)
		{
			Debug.LogWarning("[RoadGenerator] SpawnSegment: templateSegment is NULL, skipping");
			return;
		}

		Vector3 position = new Vector3(segmentPositionOffset.x, segmentPositionOffset.y, nextSpawnZ);
		Quaternion rotation = Quaternion.Euler(segmentRotation);
		GameObject segment = Instantiate(templateSegment, position, rotation, transform);
		segment.name = "RoadSegment_" + nextSpawnZ;

		segment.SetActive(true);

		activeSegments.Enqueue(segment);
		Debug.Log($"[RoadGenerator] Spawned segment at Z={nextSpawnZ}, total active: {activeSegments.Count}");

		nextSpawnZ += segmentLength;
	}

	private void RemoveOldSegment(float carZ)
	{
		while (activeSegments.Count > 0)
		{
			GameObject oldest = activeSegments.Peek();
			if (oldest == null)
			{
				activeSegments.Dequeue();
				continue;
			}

			if (oldest.transform.position.z < carZ - removeBehindDistance)
			{
				activeSegments.Dequeue();
				Destroy(oldest);
				Debug.Log($"[RoadGenerator] Removed old segment at Z={oldest.transform.position.z}");
			}
			else
			{
				break;
			}
		}
	}
}