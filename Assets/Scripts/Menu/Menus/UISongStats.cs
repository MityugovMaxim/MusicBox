using TMPro;
using UnityEngine;
using Zenject;

public class UISongStats : UIEntity
{
	[SerializeField] TMP_Text m_MissCountLabel;
	[SerializeField] TMP_Text m_BadCountLabel;
	[SerializeField] TMP_Text m_GoodCountLabel;
	[SerializeField] TMP_Text m_PerfectCountLabel;

	[Inject] ScoreManager m_ScoreManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessStats();
	}

	void ProcessStats()
	{
		m_MissCountLabel.text    = m_ScoreManager.MissCount.ToString();
		m_BadCountLabel.text     = m_ScoreManager.BadCount.ToString();
		m_GoodCountLabel.text    = m_ScoreManager.GoodCount.ToString();
		m_PerfectCountLabel.text = m_ScoreManager.PerfectCount.ToString();
	}
}