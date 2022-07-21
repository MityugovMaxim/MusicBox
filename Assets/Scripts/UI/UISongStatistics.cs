using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UISongStatistics : UIGroup
{
	[SerializeField] UIUnitLabel m_Perfect;
	[SerializeField] UIUnitLabel m_Great;
	[SerializeField] UIUnitLabel m_Good;
	[SerializeField] UIUnitLabel m_Bad;

	[SerializeField, Sound] string m_TitleSound;
	[SerializeField, Sound] string m_UnitSound;

	[Inject] ScoreManager    m_ScoreManager;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	public async Task PlayAsync()
	{
		m_Perfect.Value = 0;
		m_Great.Value   = 0;
		m_Good.Value    = 0;
		m_Bad.Value     = 0;
		
		int bad     = m_ScoreManager.GetStatistics(ScoreGrade.Bad);
		int good    = m_ScoreManager.GetStatistics(ScoreGrade.Good);
		int great   = m_ScoreManager.GetStatistics(ScoreGrade.Great);
		int perfect = m_ScoreManager.GetStatistics(ScoreGrade.Perfect);
		
		int total = bad + good + great + perfect;
		if (total == 0)
			return;
		
		await ShowAsync();
		await ProcessAsync(m_Bad, bad);
		await ProcessAsync(m_Good, good);
		await ProcessAsync(m_Great, great);
		await ProcessAsync(m_Perfect, perfect);
		await HideAsync();
	}

	async Task ProcessAsync(UIUnitLabel _Label, int _Count)
	{
		const float speed = 0.0075f;
		
		if (_Count == 0)
			return;
		
		await Task.Delay(250);
		
		await UnitAsync(_Label, _Count, _Count * speed);
		
		await Task.Delay(500);
	}

	Task UnitAsync(UIUnitLabel _Label, int _Count, float _Duration)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		IEnumerator routine = UnitRoutine(_Label, _Count, _Duration, () => completionSource.TrySetResult(true));
		
		StartCoroutine(routine);
		
		return completionSource.Task;
	}

	IEnumerator UnitRoutine(UIUnitLabel _Label, long _Count, float _Duration, Action _Finished)
	{
		if (_Label == null)
			yield break;
		
		EaseFunction function = EaseFunction.EaseOut;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			long source = MathUtility.Lerp(0, _Count, function.Get(time));
			
			time += Time.deltaTime;
			
			long target = MathUtility.Lerp(0, _Count, function.Get(time));
			
			_Label.Value = target;
			
			if (source >= target)
				continue;
			
			m_SoundProcessor.Play(m_UnitSound);
			m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		}
		
		_Label.Value = _Count;
		
		m_SoundProcessor.Play(m_TitleSound);
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		_Finished?.Invoke();
	}
}