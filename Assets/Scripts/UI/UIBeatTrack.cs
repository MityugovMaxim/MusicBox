using System;
using System.Collections;
using Coffee.UIExtensions;
using UnityEngine;
using Zenject;

public class UIBeatTrack : UIEntity, IInitializable, IDisposable
{
	[SerializeField] CanvasGroup  m_Group;
	[SerializeField] UIParticle[] m_Particles;

	SignalBus m_SignalBus;

	int         m_Multiplier;
	IEnumerator m_BeatRoutine;

	[Inject]
	public void Construct(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Subscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Subscribe<LevelComboSignal>(RegisterLevelCombo);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
		m_SignalBus.Unsubscribe<LevelRestartSignal>(RegisterLevelRestart);
		m_SignalBus.Unsubscribe<LevelComboSignal>(RegisterLevelCombo);
	}

	void RegisterLevelStart()
	{
		if (m_BeatRoutine != null)
			StopCoroutine(m_BeatRoutine);
		
		m_Multiplier  = 0;
		m_Group.alpha = 0;
	}

	void RegisterLevelRestart()
	{
		if (m_BeatRoutine != null)
			StopCoroutine(m_BeatRoutine);
		
		m_Multiplier  = 0;
		m_Group.alpha = 0;
	}

	void RegisterLevelCombo(LevelComboSignal _Signal)
	{
		m_Multiplier = _Signal.Multiplier;
	}

	public void Beat(float _Duration)
	{
		if (m_BeatRoutine != null)
			StopCoroutine(m_BeatRoutine);
		
		if (m_Multiplier < 4)
			return;
		
		foreach (UIParticle particles in m_Particles)
			particles.Play();
		
		m_BeatRoutine = BeatRoutine(m_Group, _Duration);
		
		StartCoroutine(m_BeatRoutine);
	}

	static IEnumerator BeatRoutine(CanvasGroup _Group, float _Duration)
	{
		if (_Group == null)
			yield break;
		
		const float source = 1;
		const float target = 0;
		
		_Group.alpha = source;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			_Group.alpha = Mathf.Lerp(source, target, time / _Duration);
		}
		
		_Group.alpha = target;
	}
}