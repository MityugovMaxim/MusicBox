using UnityEngine;
using Zenject;

public class UISongStats : UIEntity
{
	[SerializeField] UIUnitLabel m_MissCountLabel;
	[SerializeField] UIUnitLabel m_BadCountLabel;
	[SerializeField] UIUnitLabel m_GoodCountLabel;
	[SerializeField] UIUnitLabel m_PerfectCountLabel;

	[Inject] ScoreManager m_ScoreManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessStats();
	}

	void ProcessStats()
	{
		m_MissCountLabel.Value    = m_ScoreManager.MissCount;
		m_BadCountLabel.Value     = m_ScoreManager.BadCount;
		m_GoodCountLabel.Value    = m_ScoreManager.GoodCount;
		m_PerfectCountLabel.Value = m_ScoreManager.PerfectCount;
	}
}