using System;
using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CanvasGroup))]
public class UIComboIndicator : UIEntity, IInitializable, IDisposable
{
	[SerializeField] int m_MinMultiplier = 2;
	[SerializeField] int m_MaxMultiplier = 4;

	SignalBus m_SignalBus;

	bool        m_Shown;
	CanvasGroup m_CanvasGroup;
	IEnumerator m_AlphaRoutine;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus   = _SignalBus;
		m_CanvasGroup = GetComponent<CanvasGroup>();
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelComboSignal>(RegisterCombo);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelComboSignal>(RegisterCombo);
	}

	void RegisterCombo(LevelComboSignal _Signal)
	{
		int multiplier = _Signal.Multiplier;
		
		if (multiplier >= m_MinMultiplier && multiplier <= m_MaxMultiplier)
			Show();
		else
			Hide();
	}

	void Show()
	{
		if (m_Shown)
			return;
		
		m_Shown = true;
		
		if (m_AlphaRoutine != null)
			StopCoroutine(m_AlphaRoutine);
		
		m_AlphaRoutine = AlphaRoutine(m_CanvasGroup, 1, 0.3f);
		
		StartCoroutine(m_AlphaRoutine);
	}

	void Hide()
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		if (m_AlphaRoutine != null)
			StopCoroutine(m_AlphaRoutine);
		
		m_AlphaRoutine = AlphaRoutine(m_CanvasGroup, 0, 0.3f);
		
		StartCoroutine(m_AlphaRoutine);
	}

	static IEnumerator AlphaRoutine(CanvasGroup _CanvasGroup, float _Alpha, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = Mathf.Clamp01(_Alpha);
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = target;
	}
}