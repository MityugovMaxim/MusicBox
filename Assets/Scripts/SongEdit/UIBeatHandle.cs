using UnityEngine;
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
		
		ButtonExtension.Subscribe(m_AddOriginButton, AddOrigin);
		ButtonExtension.Subscribe(m_RemoveOriginButton, RemoveOrigin);
		ButtonExtension.Subscribe(m_AddBPMButton, AddBPM);
		ButtonExtension.Subscribe(m_RemoveBPMButton, RemoveBPM);
		ButtonExtension.Subscribe(m_AddBarButton, AddBar);
		ButtonExtension.Subscribe(m_RemoveBarButton, RemoveBar);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		ButtonExtension.Unsubscribe(m_AddOriginButton, AddOrigin);
		ButtonExtension.Unsubscribe(m_RemoveOriginButton, RemoveOrigin);
		ButtonExtension.Unsubscribe(m_AddBPMButton, AddBPM);
		ButtonExtension.Unsubscribe(m_RemoveBPMButton, RemoveBPM);
		ButtonExtension.Unsubscribe(m_AddBarButton, AddBar);
		ButtonExtension.Unsubscribe(m_RemoveBarButton, RemoveBar);
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
		
		m_OriginLabel.Value = m_Beat.Origin;
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
