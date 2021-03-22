using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputReceiver : Graphic, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	[Serializable]
	public class InputHandler : UnityEvent<InputType> { }

	public override Material material { get; set; }

	public override bool raycastTarget => true;

	float PixelSize { get; set; }

	[SerializeField] float        m_SwipeThreshold;
	[SerializeField] InputHandler m_OnInput;

	bool  m_SwipePerformed;

	protected override void Awake()
	{
		base.Awake();
		
		CalcPixelSize();
	}

	void CalcPixelSize()
	{
		Rect rect = GetPixelAdjustedRect();
		
		PixelSize = Mathf.Min(rect.width / Screen.width, rect.height / Screen.height);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		CalcPixelSize();
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	public void OnPointerDown(PointerEventData _EventData)
	{
		m_OnInput?.Invoke(InputType.TouchDown);
	}

	public void OnDrag(PointerEventData _EventData)
	{
		Vector2 delta = _EventData.delta * PixelSize;
		
		float speed = delta.magnitude / Time.deltaTime;
		
		if (m_SwipePerformed || speed < m_SwipeThreshold)
			return;
		
		m_SwipePerformed = true;
		
		float dx = Mathf.Abs(delta.x);
		float dy = Mathf.Abs(delta.y);
		
		InputType inputType;
		
		if (dx >= dy)
		{
			inputType = Mathf.Sign(delta.x) >= 0
				? InputType.SwipeRight
				: InputType.SwipeLeft;
		}
		else
		{
			inputType = Mathf.Sign(delta.y) >= 0
				? InputType.SwipeUp
				: InputType.SwipeDown;
		}
		
		m_OnInput?.Invoke(inputType);
	}

	public void OnPointerUp(PointerEventData _EventData)
	{
		m_SwipePerformed = false;
		
		m_OnInput?.Invoke(InputType.TouchUp);
	}
}
