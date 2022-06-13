using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIVerticalScrollView : UIEntity, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerExitHandler
{
	const float SCROLL_FRICTION_FACTOR = 0.96f;

	[SerializeField] RectTransform m_Viewport;
	[SerializeField] RectTransform m_Content;
	[SerializeField] float         m_Limit  = 350;
	[SerializeField] UnityEvent    m_Reposition;

	int     m_Pointer;
	bool    m_Drag;
	bool    m_Pressed;
	float   m_Delta;
	Vector2 m_Position;

	CancellationTokenSource m_TokenSource;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Clamp();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		CancelScroll();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Reposition = null;
	}

	void LateUpdate()
	{
		if (m_Pressed && Input.touchCount == 0)
		{
			m_Pressed = false;
			
			Swipe(0);
		}
	}

	void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData _EventData)
	{
		if (m_Pressed)
			return;
		
		m_Pointer = _EventData.pointerId;
		
		m_Pressed = true;
		
		CancelScroll();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		if (m_Drag || !m_Pressed || m_Pointer != _EventData.pointerId)
			return;
		
		Swipe(0);
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData _EventData)
	{
		if (m_Pointer != _EventData.pointerId)
			return;
		
		CancelScroll();
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		if (m_Pointer != _EventData.pointerId)
			return;
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Viewport, _EventData.position, _EventData.pressEventCamera, out Vector2 pointer);
		
		if (!m_Drag)
		{
			m_Drag     = true;
			m_Position = pointer;
		}
		
		m_Delta = pointer.y - m_Position.y;
		
		m_Position = pointer;
		
		Vector2 position = m_Content.anchoredPosition;
		float   overflow = GetOverflow(position);
		
		position.y += m_Delta * overflow;
		
		m_Content.anchoredPosition = position;
		
		m_Reposition?.Invoke();
	}

	void IEndDragHandler.OnEndDrag(PointerEventData _EventData)
	{
		if (m_Pointer != _EventData.pointerId)
			return;
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Viewport, _EventData.position, _EventData.pressEventCamera, out Vector2 pointer);
		
		float delta = pointer.y - m_Position.y;
		
		m_Position = pointer;
		
		if (Mathf.Abs(m_Delta) < Mathf.Abs(delta))
			m_Delta = delta;
		
		m_Pressed = false;
		
		Swipe(m_Delta);
	}

	void CancelScroll()
	{
		m_Drag     = false;
		m_Delta    = 0;
		m_Position = Vector2.zero;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	float MinPosition => 0;
	float MaxPosition => Mathf.Max(m_Viewport.rect.height, m_Content.rect.height) - m_Viewport.rect.height;

	float GetOverflow(Vector2 _Position)
	{
		float minOverflow = Mathf.Max(0, MinPosition - _Position.y);
		float maxOverflow = Mathf.Max(0, _Position.y - MaxPosition);
		
		float phase = Mathf.InverseLerp(0, m_Limit, minOverflow + maxOverflow);
		
		return EaseFunction.EaseOutQuad.Get(1, 0.1f, phase);
	}

	async void Swipe(float _Speed)
	{
		CancelScroll();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		float friction  = SCROLL_FRICTION_FACTOR;
		float direction = Mathf.Sign(_Speed);
		float speed     = Mathf.Abs(_Speed);
		float min       = MinPosition;
		float max       = MaxPosition;
		
		Vector2 position = m_Content.anchoredPosition;
		
		try
		{
			await UnityTask.Condition(
				() => speed >= 0.5f,
				() =>
				{
					speed *= GetOverflow(position);
					
					position.y += speed * direction;
					
					m_Content.anchoredPosition = position;
					
					m_Reposition?.Invoke();
					
					speed *= friction;
				},
				token
			);
			
			await Spring(min, max, token);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Clamp()
	{
		Vector2 position = m_Content.anchoredPosition;
		float   min      = MinPosition;
		float   max      = MaxPosition;
		
		if (position.y >= min && position.y <= max)
			return;
		
		CancelScroll();
		
		if (position.y < min)
			position = new Vector2(position.x, min);
		
		if (position.y > max)
			position = new Vector2(position.x, max);
		
		m_Content.anchoredPosition = position;
		
		m_Reposition?.Invoke();
	}

	Task Spring(float _Min, float _Max, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.CompletedTask;
		
		Vector2 position = m_Content.anchoredPosition;
		
		if (position.y >= _Min && position.y <= _Max)
			return Task.CompletedTask;
		
		Vector2 target = position;
		
		if (position.y < _Min)
			target = new Vector2(position.x, _Min);
		
		if (position.y > _Max)
			target = new Vector2(position.x, _Max);
		
		EaseFunction function = EaseFunction.EaseOut;
		
		return UnityTask.Phase(
			_Phase =>
			{
				m_Content.anchoredPosition = Vector2.Lerp(position, target, function.Get(_Phase));
				
				m_Reposition?.Invoke();
			},
			0.25f,
			_Token
		);
	}
}