using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputReceiver : Graphic, IPointerDownHandler, IPointerUpHandler
{
	public override Material material { get; set; }

	[SerializeField] UnityEvent m_OnTap;
	[SerializeField] UnityEvent m_OnSwipeLeft;
	[SerializeField] UnityEvent m_OnSwipeRight;
	[SerializeField] UnityEvent m_OnSwipeUp;
	[SerializeField] UnityEvent m_OnSwipeDown;

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	public void OnPointerDown(PointerEventData _EventData) { }

	public void OnPointerUp(PointerEventData _EventData)
	{
		Vector2 delta = GetLocalDelta(_EventData);
		
		Debug.Log($"[{GetType().Name}] Pointer Up. Delta: {delta}");
		
		float dx = Mathf.Abs(delta.x);
		float dy = Mathf.Abs(delta.y);
		
		float threshold = EventSystem.current.pixelDragThreshold * 4;
		
		#if UNITY_EDITOR
		threshold = 0;
		#endif
		
		if (dx <= threshold && dy <= threshold)
		{
			Debug.Log($"[{GetType().Name}] Tap.");
			m_OnTap?.Invoke();
			return;
		}
		
		if (dx >= dy)
		{
			float direction = Mathf.Sign(delta.x);
			if (direction >= 0)
			{
				Debug.Log($"[{GetType().Name}] Swipe Right");
				m_OnSwipeRight?.Invoke();
			}
			else
			{
				Debug.Log($"[{GetType().Name}] Swipe Left");
				m_OnSwipeLeft?.Invoke();
			}
		}
		else
		{
			float direction = Mathf.Sign(delta.y);
			if (direction >= 0)
			{
				Debug.Log($"[{GetType().Name}] Swipe Up");
				m_OnSwipeUp?.Invoke();
			}
			else
			{
				Debug.Log($"[{GetType().Name}] Swipe Down");
				m_OnSwipeDown?.Invoke();
			}
		}
	}

	Vector2 GetLocalPosition(PointerEventData _EventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			rectTransform,
			_EventData.position,
			_EventData.pressEventCamera,
			out Vector2 position
		);
		
		return position;
	}

	Vector2 GetLocalDelta(PointerEventData _EventData)
	{
		RectTransformUtility.ScreenPointToWorldPointInRectangle(
			rectTransform,
			_EventData.delta,
			_EventData.pressEventCamera,
			out Vector3 delta
		);
		
		return delta;
	}
}
