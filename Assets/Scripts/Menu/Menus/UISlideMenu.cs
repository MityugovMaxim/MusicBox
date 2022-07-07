using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISlideMenu : UIMenu, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerExitHandler
{
	[SerializeField] RectTransform m_Content;
	[SerializeField] RectTransform m_Parallax;

	bool    m_Drag;
	bool    m_Pressed;
	Vector2 m_Delta;

	CancellationTokenSource m_TokenSource;

	async void Expand(bool _Instant = false)
	{
		CancelSlide();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		float phase    = 1 - m_Content.anchorMax.y;
		float duration = ShowDuration * phase;
		
		try
		{
			await ExpandAsync(duration, _Instant, token);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		Show(true);
	}

	async void Shrink(bool _Instant = false)
	{
		CancelSlide();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		float phase    = m_Content.anchorMax.y;
		float duration = ShowDuration * phase;
		
		try
		{
			await ShrinkAsync(duration, _Instant, token);
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		Hide(true);
	}

	Task ExpandAsync(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return Task.WhenAll(
			MoveAsync(m_Content, 0, 0, _Duration, _Instant, EaseFunction.EaseOut, _Token),
			MoveAsync(m_Parallax, 0, 0.1f, _Duration * 1.5f, _Instant, EaseFunction.EaseOutBack, _Token)
		);
	}

	async Task ShrinkAsync(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		await MoveAsync(m_Content, -1, 0, _Duration, _Instant, EaseFunction.EaseOut, _Token);
		
		await MoveAsync(m_Parallax, -0.5f, 0, 0, true, EaseFunction.EaseOut, _Token);
	}

	protected override Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return ExpandAsync(_Duration, _Instant, _Token);
	}

	protected override Task HideAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return ShrinkAsync(_Duration, _Instant, _Token);
	}

	static Task MoveAsync(RectTransform _Transform, float _Position, float _Delay, float _Duration, bool _Instant, EaseFunction _Function, CancellationToken _Token = default)
	{
		if (_Transform == null)
			return Task.CompletedTask;
		
		Vector2 sourceMin = _Transform.anchorMin;
		Vector2 sourceMax = _Transform.anchorMax;
		Vector2 targetMin = new Vector2(0, 0 + _Position);
		Vector2 targetMax = new Vector2(1, 1 + _Position);
		
		void Process(float _Phase)
		{
			_Transform.anchorMin = Vector2.LerpUnclamped(sourceMin, targetMin, _Phase);
			_Transform.anchorMax = Vector2.LerpUnclamped(sourceMax, targetMax, _Phase);
		}
		
		if (_Instant)
		{
			Process(1);
			return Task.CompletedTask;
		}
		
		return UnityTask.Phase(
			Process,
			_Delay,
			_Duration,
			_Function,
			_Token
		);
	}

	void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData _EventData)
	{
		CancelSlide();
		
		m_Pressed = true;
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData _EventData)
	{
		CancelSlide();
		
		m_Pressed = true;
		m_Drag    = true;
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		m_Drag = true;
		
		float delta = _EventData.delta.y / Screen.height;
		
		Vector2 min = m_Content.anchorMin;
		Vector2 max = m_Content.anchorMax;
		
		min.y = Mathf.Clamp(min.y + delta, -1, 0);
		max.y = Mathf.Clamp(max.y + delta, 0, 1);
		
		m_Delta = _EventData.delta;
		
		m_Content.anchorMin = min;
		m_Content.anchorMax = max;
	}

	void IEndDragHandler.OnEndDrag(PointerEventData _EventData)
	{
		m_Pressed = false;
		
		const float anchorThreshold = 0.7f;
		const float speedThreshold  = 0.4f;
		
		float speed = m_Delta.y / Screen.height / Time.deltaTime;
		
		Vector2 anchor = m_Content.anchorMax;
		
		if (speed > speedThreshold)
			Expand();
		else if (speed < -speedThreshold)
			Shrink();
		else if (anchor.y > anchorThreshold)
			Expand();
		else
			Shrink();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		if (m_Drag || !m_Pressed)
			return;
		
		const float anchorThreshold = 0.7f;
		
		Vector2 anchor = m_Content.anchorMax;
		
		if (anchor.y > anchorThreshold)
			Expand();
		else
			Shrink();
	}

	void CancelSlide()
	{
		m_Drag    = false;
		m_Pressed = false;
		m_Delta   = Vector2.zero;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}