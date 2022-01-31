using TMPro;
using UnityEngine;
using Zenject;

public class UIProfile : UIEntity
{
	[SerializeField] UIRemoteGraphic m_Avatar;
	[SerializeField] TMP_Text        m_Username;
	[SerializeField] UILevel         m_Level;
	[SerializeField] TMP_Text        m_Coins;
	[SerializeField] RectTransform   m_Progress;
	[SerializeField] TMP_Text        m_Discs;
	[SerializeField] float           m_MinProgress;
	[SerializeField] float           m_MaxProgress;

	ProfileProcessor  m_ProfileProcessor;
	ProgressProcessor m_ProgressProcessor;
	SocialProcessor   m_SocialProcessor;

	[Inject]
	public void Construct(
		SignalBus         _SignalBus,
		ProfileProcessor  _ProfileProcessor,
		ProgressProcessor _ProgressProcessor,
		SocialProcessor   _SocialProcessor
	)
	{
		m_ProfileProcessor  = _ProfileProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_SocialProcessor   = _SocialProcessor;
	}

	public void Setup()
	{
		ProcessUsername();
		
		ProcessDiscs();
		
		m_Avatar.Load(m_SocialProcessor.Photo);
		m_Coins.text  = $"{m_ProfileProcessor.Coins}<sprite name=coins_icon>";
		m_Level.Level = m_ProfileProcessor.Level;
		
		Vector2 size = m_Progress.sizeDelta;
		size.x = Mathf.Lerp(m_MinProgress, m_MaxProgress, m_ProfileProcessor.GetProgress());
		m_Progress.sizeDelta = size;
	}

	void ProcessDiscs()
	{
		int level        = m_ProfileProcessor.Level;
		int currentDiscs = m_ProfileProcessor.Discs;
		int targetDiscs  = m_ProgressProcessor.GetMaxLimit(level);
		
		m_Discs.text = level < m_ProgressProcessor.GetMaxLevel()
			? $"{currentDiscs}/{targetDiscs}"
			: currentDiscs.ToString();
	}

	void ProcessUsername()
	{
		m_Username.text = m_SocialProcessor.GetUsername();
	}
}