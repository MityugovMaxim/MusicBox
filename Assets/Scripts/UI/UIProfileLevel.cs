using UnityEngine;
using Zenject;

public class UIProfileLevel : UIEntity
{
	[SerializeField] UILevel m_Level;

	[Inject] LevelParameter m_LevelParameter;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessLevel();
		
		m_LevelParameter.Subscribe(ProcessLevel);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_LevelParameter.Unsubscribe(ProcessLevel);
	}

	void ProcessLevel()
	{
		m_Level.Level = m_LevelParameter.Value;
	}
}
