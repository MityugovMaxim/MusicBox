using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialOverlay : UIGroup
{
	[SerializeField] Graphic m_Top;
	[SerializeField] Graphic m_Center;
	[SerializeField] Graphic m_Bottom;
	[SerializeField] float   m_SourceHeight;
	[SerializeField] float   m_TargetHeight;
	[SerializeField] float   m_Ratio;

	[SerializeField, Range(0, 1)] float m_Phase;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessRatio();
		
		ProcessPhase();
	}
	#endif

	public void Setup(float _Ratio)
	{
		m_Ratio = _Ratio;
		
		ProcessRatio();
		
		ProcessPhase();
	}

	void ProcessPhase()
	{
		float height = Mathf.Lerp(m_SourceHeight, m_TargetHeight, m_Phase);
		
		m_Top.rectTransform.offsetMin = new Vector2(0, height * 0.5f);
		
		Vector2 size = m_Center.rectTransform.sizeDelta;
		size.y = height;
		m_Center.rectTransform.sizeDelta = size;
		
		m_Bottom.rectTransform.offsetMax = new Vector2(0, -height * 0.5f);
	}

	void ProcessRatio()
	{
		float position = 1.0f - m_Ratio;
		
		m_Top.rectTransform.anchorMin = new Vector2(0, position);
		m_Top.rectTransform.anchorMax = new Vector2(1, 1);
		
		m_Center.rectTransform.anchorMin = new Vector2(0, position);
		m_Center.rectTransform.anchorMax = new Vector2(1, position);
		
		m_Bottom.rectTransform.anchorMin = new Vector2(0, 0);
		m_Bottom.rectTransform.anchorMax = new Vector2(1, position);
	}

	protected override Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return Task.WhenAll(
			base.ShowAnimation(_Duration, _Instant, _Token),
			HeightAnimation(_Duration, 1, _Instant, _Token)
		);
	}

	protected override Task HideAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return Task.WhenAll(
			base.HideAnimation(_Duration, _Instant, _Token),
			HeightAnimation(_Duration, 0, _Instant, _Token)
		);
	}

	Task HeightAnimation(float _Duration, float _Target, bool _Instant = false, CancellationToken _Token = default)
	{
		if (_Instant)
		{
			m_Phase = _Target;
			ProcessPhase();
			return Task.CompletedTask;
		}
		
		float source = m_Phase;
		float target = _Target;
		
		return UnityTask.Phase(
			_Phase =>
			{
				m_Phase = EaseFunction.EaseInQuad.Get(source, target, _Phase);
				
				ProcessPhase();
			},
			_Duration,
			_Token
		);
	}
}