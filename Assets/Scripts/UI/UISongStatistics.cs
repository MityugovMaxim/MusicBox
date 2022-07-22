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
		
		await Task.Delay(500);
		
		await ProcessAsync(m_Bad, bad);
		
		await Task.Delay(500);
		
		await ProcessAsync(m_Good, good);
		
		await Task.Delay(500);
		
		await ProcessAsync(m_Great, great);
		
		await Task.Delay(500);
		
		await ProcessAsync(m_Perfect, perfect);
		
		await Task.Delay(1000);
		
		await HideAsync();
	}

	Task ProcessAsync(UIUnitLabel _Label, int _Count)
	{
		if (_Count == 0)
			return Task.CompletedTask;
		
		const float speed = 0.0125f;
		
		return UnitAsync(_Label, _Count, _Count * speed);
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
		{
			_Finished?.Invoke();
			yield break;
		}
		
		EaseFunction function = EaseFunction.EaseOut;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			long value = MathUtility.Lerp(0, _Count, function.Get(time));
			
			if (value <= _Label.Value)
				continue;
			
			_Label.Value = value;
			
			m_SoundProcessor.Play(m_UnitSound);
			m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		}
		
		_Label.Value = _Count;
		
		_Finished?.Invoke();
	}
}