using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UITutorialOverlay : UIOrder
{
	float Phase
	{
		get => m_Phase;
		set
		{
			if (Mathf.Approximately(m_Phase, value))
				return;
			
			m_Phase = value;
			
			ProcessPhase();
		}
	}

	[SerializeField, Range(0, 1)] float m_Phase;

	[SerializeField] UISprite m_Top;
	[SerializeField] UISprite m_Center;
	[SerializeField] UISprite m_Bottom;
	[SerializeField] float    m_SourceAlpha = 0;
	[SerializeField] float    m_TargetAlpha = 0.7f;
	[SerializeField] float    m_SourceHeight;
	[SerializeField] float    m_TargetHeight;
	[SerializeField] float    m_Ratio;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
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
		float alpha  = Mathf.Lerp(m_SourceAlpha, m_TargetAlpha, Phase);
		float height = Mathf.Lerp(m_SourceHeight, m_TargetHeight, Phase);
		
		m_Top.Alpha    = alpha;
		m_Center.Alpha = alpha;
		m_Bottom.Alpha = alpha;
		
		m_Top.RectTransform.offsetMin = new Vector2(0, height * 0.5f);
		
		Vector2 size = m_Center.RectTransform.sizeDelta;
		size.y = height;
		m_Center.RectTransform.sizeDelta = size;
		
		m_Bottom.RectTransform.offsetMax = new Vector2(0, -height * 0.5f);
	}

	void ProcessRatio()
	{
		float position = 1.0f - m_Ratio;
		
		m_Top.RectTransform.anchorMin = new Vector2(0, position);
		m_Top.RectTransform.anchorMax = new Vector2(1, 1);
		
		m_Center.RectTransform.anchorMin = new Vector2(0, position);
		m_Center.RectTransform.anchorMax = new Vector2(1, position);
		
		m_Bottom.RectTransform.anchorMin = new Vector2(0, 0);
		m_Bottom.RectTransform.anchorMax = new Vector2(1, position);
	}

	public Task ShowAsync(CancellationToken _Token = default)
	{
		float source = Phase;
		float target = 1;
		
		return UnityTask.Phase(
			_Phase => Phase = Mathf.Lerp(source, target, _Phase),
			0.3f,
			EaseFunction.EaseOutCubic,
			_Token
		);
	}

	public Task HideAsync(CancellationToken _Token = default)
	{
		float source = Phase;
		float target = 0;
		
		return UnityTask.Phase(
			_Phase => Phase = Mathf.Lerp(source, target, _Phase),
			0.15f,
			EaseFunction.EaseIn,
			_Token
		);
	}
}