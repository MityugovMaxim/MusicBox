using UnityEngine;

public abstract class Clip : ScriptableObject
{
	public float MinTime  => m_MinTime;
	public float MaxTime => m_MaxTime;

	[SerializeField] float m_MinTime;
	[SerializeField] float m_MaxTime;

	bool m_Playing;

	public void Sample(float _Time)
	{
		if (_Time >= MinTime && !m_Playing)
		{
			m_Playing = true;
			OnEnter(_Time);
		}
		
		if (m_Playing)
			OnUpdate(_Time);
		
		if (_Time >= MaxTime && m_Playing)
		{
			m_Playing = false;
			OnExit(_Time);
		}
	}

	public void Stop(float _Time)
	{
		m_Playing = false;
		
		OnStop(_Time);
	}

	protected abstract void OnEnter(float _Time);

	protected abstract void OnUpdate(float _Time);

	protected abstract void OnExit(float _Time);

	protected abstract void OnStop(float _Time);

	protected float GetNormalizedTime(float _Time)
	{
		return Mathf.InverseLerp(MinTime, MaxTime, _Time);
	}

	protected float GetLocalTime(float _Time)
	{
		return _Time - MinTime;
	}
}