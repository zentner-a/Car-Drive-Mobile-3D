using System.Collections.Generic;
using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
	[Header("Префаб трафика")]
	[SerializeField] private GameObject trafficCarPrefab;

	[Header("Настройки")]
	[SerializeField] private int maxTrafficCars = 4;
	[SerializeField] private float spawnZ = 90f;
	[SerializeField] private float removeZ = -40f;
	[SerializeField] private float minSpeed = 6f;
	[SerializeField] private float maxSpeed = 12f;
	[SerializeField] private float spawnInterval = 2.5f;

	[Header("Полоса движения (X)")]
	[Tooltip("0 — левая встречная, 1 — центр, 2 — правая")]
	[SerializeField] private float[] laneX = { -3.5f, 0f, 3.5f };

	[Header("Поворот машин трафика")]
	[Tooltip("Поворот по Y для каждой полосы (параллельно laneX). 0 — едет лицом вперёд, 180 — развернута навстречу.")]
	[SerializeField] private float[] laneYRotation = { 0f, 0f, 0f };

	private readonly List<TrafficCar> activeCars = new List<TrafficCar>();
	private float timer;
	private Transform player;

	private void Awake()
	{
		var car = FindObjectOfType<CarController>();
		if (car != null)
			player = car.transform;
	}

	private void Update()
	{
		if (player == null || trafficCarPrefab == null)
			return;

		timer += Time.deltaTime;
		if (timer >= spawnInterval && activeCars.Count < maxTrafficCars)
		{
			Spawn();
			timer = 0f;
		}

		for (int i = activeCars.Count - 1; i >= 0; i--)
		{
			if (activeCars[i] == null || activeCars[i].transform.position.z < removeZ)
			{
				if (activeCars[i] != null)
					Destroy(activeCars[i].gameObject);
				activeCars.RemoveAt(i);
			}
		}
	}

	private void Spawn()
	{
		int lane = Random.Range(0, laneX.Length);

		// Поворот по Y для этой полосы (защита от несовпадения длин массивов)
		float yRot = 0f;
		if (laneYRotation != null && laneYRotation.Length > lane)
			yRot = laneYRotation[lane];

		Vector3 pos = new Vector3(laneX[lane], 0f, player.position.z + spawnZ);
		Quaternion rot = Quaternion.Euler(0f, yRot, 0f);
		GameObject go = Instantiate(trafficCarPrefab, pos, rot);
		var traffic = go.GetComponent<TrafficCar>();
		if (traffic == null)
			traffic = go.AddComponent<TrafficCar>();
		traffic.speed = Random.Range(minSpeed, maxSpeed);
		traffic.laneIndex = lane;
		activeCars.Add(traffic);
	}
}
