using UnityEngine;
using Zenject;

public class UIProfileDiscs : UIEntity
{
	[SerializeField] UIUnitLabel m_BronzeDiscs;
	[SerializeField] UIUnitLabel m_SilverDiscs;
	[SerializeField] UIUnitLabel m_GoldDiscs;
	[SerializeField] UIUnitLabel m_PlatinumDiscs;

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
		m_BronzeDiscs.Value   = m_ProfileDiscs.Value?.Bronze ?? 0;
		m_SilverDiscs.Value   = m_ProfileDiscs.Value?.Silver ?? 0;
		m_GoldDiscs.Value     = m_ProfileDiscs.Value?.Gold ?? 0;
		m_PlatinumDiscs.Value = m_ProfileDiscs.Value?.Platinum ?? 0;
	}
}
