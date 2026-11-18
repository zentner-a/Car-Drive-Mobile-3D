using UnityEngine;

public class TrafficCar : MonoBehaviour
{
	public float speed = 10f;
	public int laneIndex = 1;

	private void OnEnable()
	{
		gameObject.tag = "Traffic";

		var collider = GetComponent<BoxCollider>();
		if (collider == null)
		{
			collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.size = new Vector3(1.5f, 1.5f, 3f);
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
		if (other.CompareTag("Player"))
		{
			GameOverManager.Instance?.TriggerGameOver();
		}
	}
}
