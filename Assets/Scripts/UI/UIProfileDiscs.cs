using UnityEngine;
using Zenject;

public class UIProfileDiscs : UIEntity
{
	[SerializeField] UIUnitLabel m_BronzeDiscs;
	[SerializeField] UIUnitLabel m_SilverDiscs;
	[SerializeField] UIUnitLabel m_GoldDiscs;
	[SerializeField] UIUnitLabel m_PlatinumDiscs;

	[Inject] DiscsParameter m_DiscsParameter;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessDiscs();
		
		m_DiscsParameter.Subscribe(ProcessDiscs);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_DiscsParameter.Unsubscribe(ProcessDiscs);
	}

	void ProcessDiscs()
	{
		m_BronzeDiscs.Value   = m_DiscsParameter.Value?.Bronze ?? 0;
		m_SilverDiscs.Value   = m_DiscsParameter.Value?.Silver ?? 0;
		m_GoldDiscs.Value     = m_DiscsParameter.Value?.Gold ?? 0;
		m_PlatinumDiscs.Value = m_DiscsParameter.Value?.Platinum ?? 0;
	}
}
