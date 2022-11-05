using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIBeatHandle : UIEntity
{
	[Inject] UIBeat m_Beat;

	[SerializeField] UIRepeatButton m_AddOriginButton;
	[SerializeField] UIRepeatButton m_RemoveOriginButton;
	[SerializeField] UIRepeatButton m_AddBPMButton;
	[SerializeField] UIRepeatButton m_RemoveBPMButton;
	[SerializeField] UIRepeatButton m_AddBarButton;
	[SerializeField] UIRepeatButton m_RemoveBarButton;
	[SerializeField] UIUnitLabel    m_BPMLabel;
	[SerializeField] UIUnitLabel    m_BarLabel;
	[SerializeField] UIUnitLabel    m_OriginLabel;

	protected override void Awake()
	{
		base.Awake();
		
		m_AddOriginButton.Subscribe(AddOrigin);
		m_RemoveOriginButton.Subscribe(RemoveOrigin);
		m_AddBPMButton.Subscribe(AddBPM);
		m_RemoveBPMButton.Subscribe(RemoveBPM);
		m_AddBarButton.Subscribe(AddBar);
		m_RemoveBarButton.Subscribe(RemoveBar);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_AddOriginButton.Unsubscribe(AddOrigin);
		m_RemoveOriginButton.Unsubscribe(RemoveOrigin);
		m_AddBPMButton.Unsubscribe(AddBPM);
		m_RemoveBPMButton.Unsubscribe(RemoveBPM);
		m_AddBarButton.Unsubscribe(AddBar);
		m_RemoveBarButton.Unsubscribe(RemoveBar);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_BPMLabel.Value    = m_Beat.BPM;
		m_BarLabel.Value    = m_Beat.Bar;
		m_OriginLabel.Value = m_Beat.Origin;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_Beat.Upload();
	}

	void AddOrigin()
	{
		m_Beat.Origin -= 0.001;
		
		m_OriginLabel.Value =  m_Beat.Origin;
	}

	void RemoveOrigin()
	{
		m_Beat.Origin += 0.001;
		
		m_OriginLabel.Value = m_Beat.Origin;
	}

	void AddBPM()
	{
		m_Beat.BPM += 0.05f;
		
		m_BPMLabel.Value = m_Beat.BPM;
	}

	void RemoveBPM()
	{
		m_Beat.BPM = Mathf.Max(0, m_Beat.BPM - 0.05f);
		
		m_BPMLabel.Value = m_Beat.BPM;
	}

	void AddBar()
	{
		m_Beat.Bar += 1;
		
		m_BarLabel.Value = m_Beat.Bar;
	}

	void RemoveBar()
	{
		m_Beat.Bar = Mathf.Max(1, m_Beat.Bar - 1);
		
		m_BarLabel.Value = m_Beat.Bar;
	}
}
