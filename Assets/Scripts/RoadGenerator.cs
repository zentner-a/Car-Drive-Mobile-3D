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

	private readonly List<GameObject> activeSegments = new List<GameObject>();
	private CarController carController;

	private void Awake()
	{
		if (roadSegmentPrefab == null || car == null || segmentLength <= 0f ||
			float.IsNaN(segmentLength) || float.IsInfinity(segmentLength) || segmentsAhead <= 0)
		{
			Debug.LogError("[RoadGenerator] Assign the segment and car, a finite positive length, and a positive segment count.", this);
			enabled = false;
			return;
		}

		carController = car.GetComponent<CarController>();

		// Машина стоит на месте, поэтому все сегменты спавним относительно её фиксированной позиции.
		// Мировые координаты не улетают в бесконечность даже на очень поздних стадиях.
		float carZ = car.position.z;
		
		// Сначала спавним сегмент ПОД машиной (чтобы не проваливалась)
		SpawnSegment(carZ);
		
		// Затем segmentsAhead сегментов впереди
		float z = carZ + segmentLength;
		for (int i = 0; i < segmentsAhead; i++)
		{
			SpawnSegment(z);
			z += segmentLength;
		}
	}

	private void Update()
	{
		if (car == null)
			return;

		float carZ = car.position.z;
		float scrollSpeed = carController != null ? carController.ForwardSpeed : 0f;
		float moveDelta = scrollSpeed * Time.deltaTime;

		// Двигаем всю дорогу навстречу машине (назад по Z). Сама машина стоит неподвижно.
		for (int i = activeSegments.Count - 1; i >= 0; i--)
		{
			var segment = activeSegments[i];
			if (segment == null)
			{
				activeSegments.RemoveAt(i);
				continue;
			}
			Vector3 pos = segment.transform.position;
			pos.z -= moveDelta;
			segment.transform.position = pos;
		}

		// Поддерживаем segmentsAhead сегментов впереди машины.
		float targetZ = carZ + segmentLength * segmentsAhead;
		float furthest = activeSegments.Count > 0 ? GetFurthestSegmentZ() : carZ;
		while (furthest < targetZ)
		{
			SpawnSegment(furthest + segmentLength);
			furthest += segmentLength;
		}

		RemoveOldSegment(carZ);
	}

	private float GetFurthestSegmentZ()
	{
		float furthest = float.MinValue;
		foreach (var segment in activeSegments)
		{
			if (segment != null && segment.transform.position.z > furthest)
				furthest = segment.transform.position.z;
		}
		return furthest;
	}

	private void SpawnSegment(float z)
	{
		Vector3 position = new Vector3(0f, 0f, z);
		GameObject segment = Instantiate(roadSegmentPrefab, position, Quaternion.identity);
		segment.transform.SetParent(transform, worldPositionStays: true);
		segment.name = "RoadSegment_" + z;
		activeSegments.Add(segment);
	}

	private void RemoveOldSegment(float carZ)
	{
		for (int i = activeSegments.Count - 1; i >= 0; i--)
		{
			var segment = activeSegments[i];
			if (segment == null)
			{
				activeSegments.RemoveAt(i);
				continue;
			}
			if (segment.transform.position.z < carZ - removeBehindDistance)
			{
				activeSegments.RemoveAt(i);
				Destroy(segment);
			}
		}
	}
}