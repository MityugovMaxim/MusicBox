using UnityEngine;
using Zenject;

public class UIProfileLevel : UIEntity
{
	[SerializeField] UILevel m_Level;

	[Inject] ProfileLevelParameter m_ProfileLevel;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessLevel();
		
		m_ProfileLevel.Subscribe(ProcessLevel);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_ProfileLevel.Unsubscribe(ProcessLevel);
	}

	void ProcessLevel()
	{
		m_Level.Level = m_ProfileLevel.Value;
	}
}
