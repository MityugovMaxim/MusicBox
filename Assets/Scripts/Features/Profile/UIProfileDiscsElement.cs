using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProfileDiscsElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIProfileDiscsElement> { }

	[SerializeField] UIUnitLabel m_Bronze;
	[SerializeField] UIUnitLabel m_Silver;
	[SerializeField] UIUnitLabel m_Gold;
	[SerializeField] UIUnitLabel m_Platinum;

	[Inject] ProfileDiscsParameter m_ProfileDiscs;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessDiscs();
		
		m_ProfileDiscs.Subscribe(ProcessDiscs);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_ProfileDiscs.Unsubscribe(ProcessDiscs);
	}

	void ProcessDiscs()
	{
		m_Bronze.Value   = m_ProfileDiscs.GetBronze();
		m_Silver.Value   = m_ProfileDiscs.GetSilver();
		m_Gold.Value     = m_ProfileDiscs.GetGold();
		m_Platinum.Value = m_ProfileDiscs.GetPlatinum();
	}
}
