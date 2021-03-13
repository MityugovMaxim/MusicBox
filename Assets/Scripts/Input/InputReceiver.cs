using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputReceiver : Graphic, IPointerDownHandler, IPointerUpHandler
{
	public override Material material { get; set; }

	[SerializeField] float      m_SwipeThreshold;
	[SerializeField] UnityEvent m_OnTap;
	[SerializeField] UnityEvent m_OnSwipeLeft;
	[SerializeField] UnityEvent m_OnSwipeRight;
	[SerializeField] UnityEvent m_OnSwipeUp;
	[SerializeField] UnityEvent m_OnSwipeDown;

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	public void OnPointerDown(PointerEventData _EventData)
	{
		Debug.LogError("---> TAP");
		
		m_OnTap?.Invoke();
	}

	public void OnPointerUp(PointerEventData _EventData)
	{
		Debug.LogError("---> POINTER UP DELTA: " + _EventData.delta);
		
		float dx = Mathf.Abs(_EventData.delta.x);
		float dy = Mathf.Abs(_EventData.delta.y);
		
		if (dx < m_SwipeThreshold && dy < m_SwipeThreshold)
			return;
		
		if (dx >= dy)
		{
			float direction = Mathf.Sign(_EventData.delta.x);
			if (direction >= 0)
				m_OnSwipeRight?.Invoke();
			else
				m_OnSwipeLeft?.Invoke();
		}
		else
		{
			float direction = Mathf.Sign(_EventData.delta.y);
			if (direction >= 0)
				m_OnSwipeUp?.Invoke();
			else
				m_OnSwipeDown?.Invoke();
		}
	}
}
