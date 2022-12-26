using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIHorizontalScrollView : UIEntity, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerExitHandler
{
	const float SCROLL_FRICTION_FACTOR = 0.96f;

	[SerializeField] RectTransform m_Viewport;
	[SerializeField] RectTransform m_Content;
	[SerializeField] float         m_Limit = 350;
	[SerializeField] UnityEvent    m_Reposition;

	int     m_Pointer;
	bool    m_Drag;
	bool    m_Pressed;
	float   m_Delta;
	Vector2 m_Position;

	CancellationTokenSource m_TokenSource;

	public void Scroll(Vector2 _Position, TextAnchor _Alignment)
	{
		CancelScroll();
		
		float direction = m_Content.pivot.x * 2 - 1;
		
		float offset = m_Viewport.rect.height * _Alignment.GetVerticalPivot();
		
		Vector2 position = m_Content.anchoredPosition;
		
		position.x = (_Position.x - offset) * direction;
		
		m_Content.anchoredPosition = position;
		
		Clamp();
	}

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
		
		m_Delta = pointer.x - m_Position.x;
		
		m_Position = pointer;
		
		Vector2 position = m_Content.anchoredPosition;
		float   overflow = GetOverflow(position);
		
		position.x += m_Delta * overflow;
		
		m_Content.anchoredPosition = position;
		
		m_Reposition?.Invoke();
	}

	void IEndDragHandler.OnEndDrag(PointerEventData _EventData)
	{
		if (m_Pointer != _EventData.pointerId)
			return;
		
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

	float MinPosition => -Height * (1.0f - m_Content.pivot.x);
	float MaxPosition => Height * m_Content.pivot.x;

	float Height => Mathf.Max(m_Viewport.rect.height, m_Content.rect.height) - m_Viewport.rect.height;

	float GetOverflow(Vector2 _Position)
	{
		float minOverflow = Mathf.Max(0, MinPosition - _Position.x);
		float maxOverflow = Mathf.Max(0, _Position.x - MaxPosition);
		
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
				() => speed >= 1.0f,
				() =>
				{
					speed *= GetOverflow(position);
					
					position.x += speed * direction;
					
					m_Content.anchoredPosition = position;
					
					m_Reposition?.Invoke();
					
					speed *= friction;
				},
				token
			);
			
			await Spring(min, max, token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		
		m_TokenSource.Dispose();
		m_TokenSource = null;
	}

	void Clamp()
	{
		Vector2 position = m_Content.anchoredPosition;
		float   min      = MinPosition;
		float   max      = MaxPosition;
		
		if (position.x >= min && position.x <= max)
			return;
		
		CancelScroll();
		
		if (position.x < min)
			position = new Vector2(min, position.y);
		
		if (position.x > max)
			position = new Vector2(max, position.y);
		
		m_Content.anchoredPosition = position;
		
		m_Reposition?.Invoke();
	}

	Task Spring(float _Min, float _Max, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.CompletedTask;
		
		Vector2 position = m_Content.anchoredPosition;
		
		if (position.x >= _Min && position.x <= _Max)
			return Task.CompletedTask;
		
		Vector2 target = position;
		
		if (position.x < _Min)
			target = new Vector2(_Min, position.y);
		
		if (position.x > _Max)
			target = new Vector2(_Max, position.y);
		
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