using System;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class HealthController : IInitializable, IDisposable
{
	const int MAX_HEALTH = 4;

	public DynamicDelegate<int> OnDamage;
	public DynamicDelegate<int> OnRestore;

	[Inject] ScoreController m_ScoreController;
	[Inject] ConfigProcessor m_ConfigProcessor;

	Action m_Death;
	int    m_Health;
	float  m_InvincibilityDuration;
	float  m_InvincibilityTime;

	void IInitializable.Initialize()
	{
		m_ScoreController.OnComboChange += OnComboChange;
	}

	void IDisposable.Dispose()
	{
		m_ScoreController.OnComboChange -= OnComboChange;
	}

	public void Setup(Action _Death)
	{
		m_Death                 = _Death;
		m_InvincibilityTime     = 0;
		m_InvincibilityDuration = m_ConfigProcessor.SongIFrames;
	}

	public void Restore()
	{
		m_Health = MAX_HEALTH;
		
		m_InvincibilityTime = 0;
		
		OnRestore?.Invoke(m_Health);
	}

	void OnComboChange(int _Combo, ScoreGrade _ScoreGrade)
	{
		if (_ScoreGrade == ScoreGrade.Fail || _ScoreGrade == ScoreGrade.Miss)
			Damage();
	}

	void Damage()
	{
		if (Time.time < m_InvincibilityTime)
			return;
		
		m_InvincibilityTime = Time.time + m_InvincibilityDuration;
		
		m_Health = Mathf.Max(0, m_Health - 1);
		
		OnDamage?.Invoke(m_Health);
		
		if (m_Health == 0)
			m_Death?.Invoke();
	}
}
