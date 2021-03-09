using UnityEngine;

public class EventClip : Clip
{
	Component m_Component;
	string    m_MethodName;

	public void Initialize(Component _Component, string _MethodName)
	{
		m_Component  = _Component;
		m_MethodName = _MethodName;
	}

	protected override void OnEnter(float _Time) { }

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		if (m_Component != null)
			m_Component.SendMessage(m_MethodName);
	}

	protected override void OnStop(float _Time) { }
}