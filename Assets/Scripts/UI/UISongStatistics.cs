using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public class UISongStatistics : UIGroup
{
	[SerializeField] TMP_Text    m_Label;
	[SerializeField] UIUnitLabel m_Count;

	[SerializeField, Sound] string m_TitleSound;
	[SerializeField, Sound] string m_UnitSound;

	[Inject] ScoreManager    m_ScoreManager;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	protected override void OnShowStarted()
	{
		m_Label.alpha = 0;
		m_Label.text  = string.Empty;
		
		m_Count.Value = 0;
	}

	public async Task PlayAsync()
	{
		int bad     = m_ScoreManager.GetStatistics(ScoreGrade.Bad);
		int good    = m_ScoreManager.GetStatistics(ScoreGrade.Good);
		int great   = m_ScoreManager.GetStatistics(ScoreGrade.Great);
		int perfect = m_ScoreManager.GetStatistics(ScoreGrade.Perfect);
		
		int total = bad + good + great + perfect;
		if (total == 0)
			return;
		
		await ShowAsync();
		await ProcessAsync("BAD", bad);
		await ProcessAsync("GOOD", good);
		await ProcessAsync("GREAT", great);
		await ProcessAsync("PERFECT", perfect);
		await HideAsync();
	}

	async Task ProcessAsync(string _Label, int _Count)
	{
		if (_Count == 0)
			return;
		
		await Task.WhenAll(
			AlphaAsync(m_Label, 0, 0.15f),
			ScaleAsync(m_Label.rectTransform, 1, 0.5f, 0.15f, EaseFunction.EaseIn),
			UnitAsync(0, 0.15f)
		);
		
		m_Label.text = _Label;
		
		m_SoundProcessor.Play(m_TitleSound);
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		await Task.WhenAll(
			AlphaAsync(m_Label, 1, 0.2f),
			ScaleAsync(m_Label.rectTransform, 1.5f, 1, 0.2f, EaseFunction.EaseOutBack)
		);
		
		await Task.Delay(250);
		
		await UnitAsync(_Count, 0.3f);
		
		await Task.Delay(500);
	}

	Task AlphaAsync(TMP_Text _Text, float _Alpha, float _Duration)
	{
		if (_Text == null)
			return Task.CompletedTask;
		
		float source = _Text.alpha;
		float target = _Alpha;
		return UnityTask.Phase(
			_Phase => _Text.alpha = Mathf.LerpUnclamped(source, target, _Phase),
			_Duration
		);
	}

	Task ScaleAsync(RectTransform _RectTransform, float _Source, float _Target, float _Duration, EaseFunction _Function)
	{
		if (_RectTransform == null)
			return Task.CompletedTask;
		
		Vector3 source = new Vector3(_Source, _Source, 1);
		Vector3 target = new Vector3(_Target, _Target, 1);
		
		return UnityTask.Phase(
			_Phase => _RectTransform.localScale = Vector3.LerpUnclamped(source, target, _Phase),
			_Duration,
			_Function
		);
	}

	Task UnitAsync(int _Count, float _Duration)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		IEnumerator routine = UnitRoutine(_Count, _Duration, () => completionSource.TrySetResult(true));
		
		StartCoroutine(routine);
		
		return completionSource.Task;
	}

	IEnumerator UnitRoutine(long _Count, float _Duration, Action _Finished)
	{
		EaseFunction function = EaseFunction.EaseOut;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			long source = MathUtility.Lerp(0, _Count, function.Get(time));
			
			time += Time.deltaTime;
			
			long target = MathUtility.Lerp(0, _Count, function.Get(time));
			
			m_Count.Value = target;
			
			if (source >= target)
				continue;
			
			m_SoundProcessor.Play(m_UnitSound);
			m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		}
		
		m_Count.Value = _Count;
		
		_Finished?.Invoke();
	}
}