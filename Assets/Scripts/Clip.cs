using System;
using UnityEngine;

[Serializable]
public abstract class Clip
{
	public float StartTime  => m_StartTime;
	public float FinishTime => m_FinishTime;

	[SerializeField] float m_StartTime;
	[SerializeField] float m_FinishTime;

	bool m_Playing;

	public void Sample(float _Time)
	{
		if (_Time >= StartTime && !m_Playing)
		{
			m_Playing = true;
			OnEnter(_Time);
		}
		
		if (m_Playing)
			OnUpdate(_Time);
		
		if (_Time >= FinishTime && m_Playing)
		{
			m_Playing = false;
			OnExit(_Time);
		}
	}

	protected abstract void OnEnter(float _Time);

	protected abstract void OnUpdate(float _Time);

	protected abstract void OnExit(float _Time);
}