using UnityEngine;

public class EventClip : Clip
{
	[SerializeField] string m_MethodName;

	GameObject m_GameObject;

	public void Initialize(GameObject _GameObject)
	{
		m_GameObject = _GameObject;
	}

	protected override void OnEnter(float _Time) { }

	protected override void OnUpdate(float _Time) { }

	protected override void OnExit(float _Time)
	{
		if (m_GameObject != null)
			m_GameObject.SendMessage(m_MethodName);
	}

	protected override void OnStop(float _Time) { }
}