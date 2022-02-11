using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class UIGroupLayout : UIGroup
{
	public enum Mode
	{
		Both   = 0,
		Width  = 1,
		Height = 2,
	}

	LayoutElement LayoutElement
	{
		get
		{
			if (m_LayoutElement == null)
				m_LayoutElement = GetComponent<LayoutElement>();
			return m_LayoutElement;
		}
	}

	[SerializeField] float          m_ShowResizeDuration;
	[SerializeField] float          m_HideResizeDuration;
	[SerializeField] AnimationCurve m_ResizeCurve = AnimationCurve.Linear(0, 0, 1, 1);
	[SerializeField] Mode           m_ResizeMode;
	[SerializeField] Vector2        m_Size;

	LayoutElement m_LayoutElement;

	protected override async Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		await ResizeAnimation(m_Size, m_ShowResizeDuration, _Instant, _Token);
		
		await base.ShowAnimation(_Duration, _Instant, _Token);
	}

	protected override async Task HideAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		await base.HideAnimation(_Duration, _Instant, _Token);
		
		await ResizeAnimation(Vector2.zero, m_HideResizeDuration, _Instant, _Token);
	}

	Task ResizeAnimation(Vector2 _Size, float _Duration, bool _Instant, CancellationToken _Token = default)
	{
		switch (m_ResizeMode)
		{
			case Mode.Both:
				return SizeAnimation(_Size, _Duration, m_ResizeCurve, _Instant, _Token);
			
			case Mode.Width:
				return WidthAnimation(_Size.x, _Duration, m_ResizeCurve, _Instant, _Token);
			
			case Mode.Height:
				return HeightAnimation(_Size.y, _Duration, m_ResizeCurve, _Instant, _Token);
			
			default:
				return null;
		}
	}

	async Task WidthAnimation(float _Width, float _Duration, AnimationCurve _Curve, bool _Instant, CancellationToken _Token)
	{
		float source = LayoutElement.preferredWidth;
		float target = _Width;
		
		if (Mathf.Approximately(source, target))
			return;
		
		void Animation(float _Phase)
		{
			float phase = _Curve.Evaluate(_Phase);
			
			LayoutElement.preferredWidth = Mathf.Lerp(source, target, phase);
		}
		
		if (_Instant)
			Animation(1);
		else
			await UnityTask.Phase(Animation, _Duration, _Token);
	}

	async Task HeightAnimation(float _Height, float _Duration, AnimationCurve _Curve, bool _Instant, CancellationToken _Token)
	{
		float source = LayoutElement.preferredHeight;
		float target = _Height;
		
		if (Mathf.Approximately(source, target))
			return;
		
		void Animation(float _Phase)
		{
			float phase = _Curve.Evaluate(_Phase);
			
			LayoutElement.preferredHeight = Mathf.Lerp(source, target, phase);
		}
		
		if (_Instant)
			Animation(1);
		else
			await UnityTask.Phase(Animation, _Duration, _Token);
	}

	async Task SizeAnimation(Vector2 _Size, float _Duration, AnimationCurve _Curve, bool _Instant, CancellationToken _Token)
	{
		await Task.WhenAll(
			WidthAnimation(_Size.x, _Duration, _Curve, _Instant, _Token),
			HeightAnimation(_Size.y, _Duration, _Curve, _Instant, _Token)
		);
	}
}