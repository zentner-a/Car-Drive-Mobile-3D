using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeInput : MonoBehaviour
{
	public static SwipeInput Instance { get; private set; }

	[SerializeField] private float minSwipeDistance = 30f;

	private Vector2 startPos;
	private bool isSwiping;

	public event System.Action OnSwipeLeft;
	public event System.Action OnSwipeRight;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (Touchscreen.current != null)
		{
			var touch = Touchscreen.current.primaryTouch;

			if (touch.press.wasPressedThisFrame)
			{
				isSwiping = true;
				startPos = touch.position.ReadValue();
			}
			else if (isSwiping && touch.press.wasReleasedThisFrame)
			{
				HandleSwipe(touch.position.ReadValue() - startPos);
				isSwiping = false;
			}
		}
		else if (Application.isEditor && Mouse.current != null)
		{
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				isSwiping = true;
				startPos = Mouse.current.position.ReadValue();
			}
			else if (isSwiping && Mouse.current.leftButton.wasReleasedThisFrame)
			{
				HandleSwipe(Mouse.current.position.ReadValue() - startPos);
				isSwiping = false;
			}

			if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
			{
				OnSwipeLeft?.Invoke();
				Debug.Log("[SwipeInput] Keyboard: Left");
			}
			if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
			{
				OnSwipeRight?.Invoke();
				Debug.Log("[SwipeInput] Keyboard: Right");
			}
		}
	}

	private void HandleSwipe(Vector2 delta)
	{
		Debug.Log($"[SwipeInput] Swipe delta: {delta}, magnitude: {delta.magnitude}");

		if (delta.magnitude < minSwipeDistance)
		{
			Debug.Log("[SwipeInput] Swipe too short, ignored");
			return;
		}

		if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
		{
			if (delta.x > 0f)
			{
				Debug.Log("[SwipeInput] Swipe Right");
				OnSwipeRight?.Invoke();
			}
			else
			{
				Debug.Log("[SwipeInput] Swipe Left");
				OnSwipeLeft?.Invoke();
			}
		}
		else
		{
			Debug.Log("[SwipeInput] Vertical swipe, ignored");
		}
	}
}
