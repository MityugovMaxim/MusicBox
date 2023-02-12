using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultStatistics : UIGroup
{
	[SerializeField] UIUnitLabel m_Perfect;
	[SerializeField] UIUnitLabel m_Great;
	[SerializeField] UIUnitLabel m_Good;
	[SerializeField] UIUnitLabel m_Bad;
	[SerializeField] UIGroup     m_PerfectGroup;
	[SerializeField] UIGroup     m_GreatGroup;
	[SerializeField] UIGroup     m_GoodGroup;
	[SerializeField] UIGroup     m_BadGroup;

	[Inject] ScoreController m_ScoreController;

	public async Task PlayAsync()
	{
		m_PerfectGroup.Show();
		
		await Task.Delay(150);
		
		m_GreatGroup.Show();
		
		await Task.Delay(150);
		
		m_GoodGroup.Show();
		
		await Task.Delay(150);
		
		await m_BadGroup.ShowAsync();
		
		await Task.Delay(1000);
	}

	protected override void OnShowStarted()
	{
		m_Perfect.Value = m_ScoreController.GetStatistics(ScoreGrade.Perfect);
		m_Great.Value   = m_ScoreController.GetStatistics(ScoreGrade.Great);
		m_Good.Value    = m_ScoreController.GetStatistics(ScoreGrade.Good);
		m_Bad.Value     = m_ScoreController.GetStatistics(ScoreGrade.Bad);
	}
}
