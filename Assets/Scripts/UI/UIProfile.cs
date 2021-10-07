using TMPro;
using UnityEngine;
using Zenject;

public class UIProfile : UIEntity
{
	[SerializeField, Range(0, 1)] float         m_Phase;
	[SerializeField]              TMP_Text      m_Username;
	[SerializeField]              RectTransform m_Progress;

	ProfileProcessor m_ProfileProcessor;
	SocialProcessor  m_SocialProcessor;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessPhase();
	}
	#endif

	[Inject]
	public void Construct(
		ProfileProcessor _ProfileProcessor,
		SocialProcessor  _SocialProcessor
	)
	{
		m_ProfileProcessor = _ProfileProcessor;
		m_SocialProcessor  = _SocialProcessor;
	}

	void ProcessPhase()
	{
		Vector2 anchor = m_Progress.anchorMax;
		anchor.x             = m_Phase;
		m_Progress.anchorMax = anchor;
	}
}