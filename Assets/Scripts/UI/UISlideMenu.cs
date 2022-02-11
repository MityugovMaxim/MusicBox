using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISlideMenu : UIMenu, IPointerDownHandler, IDragHandler, IDropHandler
{
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] RectTransform  m_Content;

	CancellationTokenSource m_TokenSource;
	Vector2                 m_Delta;

	async void Expand(bool _Instant = false)
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		float phase    = 1 - m_Content.anchorMax.y;
		float duration = ShowDuration * phase;
		
		await ExpandAsync(duration, _Instant, token);
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		Show(true);
	}

	async void Shrink(bool _Instant = false)
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		float phase    = m_Content.anchorMax.y;
		float duration = ShowDuration * phase;
		
		await ShrinkAsync(duration, _Instant, token);
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		Hide(true);
	}

	async Task ExpandAsync(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		Vector2 sourceMin = m_Content.anchorMin;
		Vector2 sourceMax = m_Content.anchorMax;
		Vector2 targetMin = Vector2.zero;
		Vector2 targetMax = Vector2.one;
		
		void Animation(float _Phase)
		{
			float phase = m_Curve.Evaluate(_Phase);
			
			m_Content.anchorMin = Vector2.Lerp(sourceMin, targetMin, phase);
			m_Content.anchorMax = Vector2.Lerp(sourceMax, targetMax, phase);
		}
		
		if (_Instant)
			Animation(1);
		else
			await UnityTask.Phase(Animation, _Duration, _Token);
	}

	async Task ShrinkAsync(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		Vector2 sourceMin = m_Content.anchorMin;
		Vector2 sourceMax = m_Content.anchorMax;
		Vector2 targetMin = new Vector2(0, -1);
		Vector2 targetMax = new Vector2(1, 0);
		
		void Animation(float _Phase)
		{
			float phase = m_Curve.Evaluate(_Phase);
			
			m_Content.anchorMin = Vector2.Lerp(sourceMin, targetMin, phase);
			m_Content.anchorMax = Vector2.Lerp(sourceMax, targetMax, phase);
		}
		
		if (_Instant)
			Animation(1);
		else
			await UnityTask.Phase(Animation, _Duration, _Token);
	}

	protected override Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return ExpandAsync(_Duration, _Instant, _Token);
	}

	protected override Task HideAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return ShrinkAsync(_Duration, _Instant, _Token);
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		float delta = _EventData.delta.y / Screen.height;
		
		Vector2 min = m_Content.anchorMin;
		Vector2 max = m_Content.anchorMax;
		
		min.y = Mathf.Clamp(min.y + delta, -1, 0);
		max.y = Mathf.Clamp(max.y + delta, 0, 1);
		
		m_Delta = _EventData.delta;
		
		m_Content.anchorMin = min;
		m_Content.anchorMax = max;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		m_Delta = Vector2.zero;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
	}

	public void OnDrop(PointerEventData _EventData)
	{
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
}