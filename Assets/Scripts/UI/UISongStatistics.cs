using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UISongStatistics : UIGroup
{
	[SerializeField] UIGroup     m_PerfectGroup;
	[SerializeField] UIGroup     m_GreatGroup;
	[SerializeField] UIGroup     m_GoodGroup;
	[SerializeField] UIGroup     m_BadGroup;
	[SerializeField] UIUnitLabel m_Perfect;
	[SerializeField] UIUnitLabel m_Great;
	[SerializeField] UIUnitLabel m_Good;
	[SerializeField] UIUnitLabel m_Bad;

	[SerializeField, Sound] string m_Sound;

	[Inject] ScoreController m_ScoreController;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	protected override void OnShowStarted()
	{
		m_PerfectGroup.Hide(true);
		m_GreatGroup.Hide(true);
		m_GoodGroup.Hide(true);
		m_BadGroup.Hide(true);
		
		m_Perfect.Value = 0;
		m_Great.Value   = 0;
		m_Good.Value    = 0;
		m_Bad.Value     = 0;
	}

	public async Task PlayAsync()
	{
		int bad     = m_ScoreController.GetStatistics(ScoreGrade.Bad);
		int good    = m_ScoreController.GetStatistics(ScoreGrade.Good);
		int great   = m_ScoreController.GetStatistics(ScoreGrade.Great);
		int perfect = m_ScoreController.GetStatistics(ScoreGrade.Perfect);
		
		int total = bad + good + great + perfect;
		
		if (total == 0)
			return;
		
		await ShowAsync();
		
		await ProcessAsync(m_BadGroup, m_Bad, bad);
		
		await ProcessAsync(m_GoodGroup, m_Good, good);
		
		await ProcessAsync(m_GreatGroup, m_Great, great);
		
		await ProcessAsync(m_PerfectGroup, m_Perfect, perfect);
		
		await UnityTask.Delay(2);
		
		await HideAsync();
	}

	Task ProcessAsync(UIGroup _Group, UIUnitLabel _Label, int _Count)
	{
		_Label.Value = _Count;
		
		m_SoundProcessor.Play(m_Sound);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactMedium);
		
		_Group.Show();
		
		return UnityTask.Delay(0.1f);
	}
}
