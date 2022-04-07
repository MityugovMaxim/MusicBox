using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class HealthRestoreSignal
{
	public int Health { get; }

	public HealthRestoreSignal(int _Health)
	{
		Health = _Health;
	}
}

[Preserve]
public class HealthIncreaseSignal
{
	public int Health { get; }

	public HealthIncreaseSignal(int _Health)
	{
		Health = _Health;
	}
}

[Preserve]
public class HealthDecreaseSignal
{
	public int Health { get; }

	public HealthDecreaseSignal(int _Health)
	{
		Health = _Health;
	}
}

[Preserve]
public class HealthManager
{
	const int MAX_HEALTH = 4;

	[Inject] SignalBus      m_SignalBus;
	[Inject] SongsProcessor m_SongsProcessor;

	int    m_Health;
	float  m_InvincibilityDuration;
	float  m_InvincibilityTime;

	public void Setup(string _SongID)
	{
		m_InvincibilityTime     = 0;
		m_InvincibilityDuration = m_SongsProcessor.GetInvincibility(_SongID);
	}

	public void Restore()
	{
		m_Health = MAX_HEALTH;
		
		m_InvincibilityTime = 0;
		
		m_SignalBus.Fire(new HealthRestoreSignal(m_Health));
	}

	public void Decrease()
	{
		if (Time.time < m_InvincibilityTime)
			return;
		
		m_InvincibilityTime = Time.time + m_InvincibilityDuration;
		
		m_Health -= 1;
		
		m_SignalBus.Fire(new HealthDecreaseSignal(m_Health));
	}

	public void Increase()
	{
		m_InvincibilityTime = 0;
		
		m_Health = Mathf.Min(m_Health + 1, MAX_HEALTH);
		
		m_SignalBus.Fire(new HealthIncreaseSignal(m_Health));
	}
}
