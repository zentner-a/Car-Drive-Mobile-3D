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

	private readonly Queue<GameObject> activeSegments = new Queue<GameObject>();
	private float nextSpawnZ;

	private void Awake()
	{
		if (roadSegmentPrefab == null || car == null || segmentLength <= 0f ||
			float.IsNaN(segmentLength) || float.IsInfinity(segmentLength) || segmentsAhead <= 0)
		{
			Debug.LogError("[RoadGenerator] Assign the segment and car, a finite positive length, and a positive segment count.", this);
			enabled = false;
			return;
		}

		nextSpawnZ = car.position.z;
		for (int i = 0; i < segmentsAhead; i++)
			SpawnSegment();
	}

	private void Update()
	{
		if (car == null)
			return;

		float carZ = car.position.z;
		while (nextSpawnZ - segmentLength < carZ + segmentLength * segmentsAhead)
			SpawnSegment();

		RemoveOldSegment(carZ);
	}

	private void SpawnSegment()
	{
		Vector3 position = new Vector3(0f, 0f, nextSpawnZ);
		GameObject segment = Instantiate(roadSegmentPrefab, position, Quaternion.identity, transform);
		segment.name = "RoadSegment_" + nextSpawnZ;
		activeSegments.Enqueue(segment);
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
			if (oldest.transform.position.z >= carZ - removeBehindDistance)
				break;
			activeSegments.Dequeue();
			Destroy(oldest);
		}
	}
}
