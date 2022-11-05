using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongBrushWidget : UISongWidget
{
	[SerializeField] Toggle m_TapToggle;
	[SerializeField] Toggle m_DoubleToggle;
	[SerializeField] Toggle m_HoldToggle;
	[SerializeField] Toggle m_ColorToggle;
	[SerializeField] Button m_PlayButton;

	[Inject] UICreateTapHandle    m_CreateTapHandle;
	[Inject] UICreateDoubleHandle m_CreateDoubleHandle;
	[Inject] UICreateHoldHandle   m_CreateHoldHandle;
	[Inject] UICreateColorHandle  m_CreateColorHandle;

	protected override void Awake()
	{
		base.Awake();
		
		m_TapToggle.onValueChanged.AddListener(ToggleTapCreation);
		m_DoubleToggle.onValueChanged.AddListener(ToggleDoubleCreation);
		m_HoldToggle.onValueChanged.AddListener(ToggleHoldCreation);
		m_ColorToggle.onValueChanged.AddListener(ToggleColorCreation);
		
		m_PlayButton.Subscribe(DisableTools);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TapToggle.onValueChanged.RemoveAllListeners();
		m_DoubleToggle.onValueChanged.RemoveAllListeners();
		m_HoldToggle.onValueChanged.RemoveAllListeners();
		m_ColorToggle.onValueChanged.RemoveAllListeners();
		
		m_PlayButton.Unsubscribe(DisableTools);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		DisableTools();
	}

	void DisableTools()
	{
		m_ColorToggle.isOn  = false;
		m_TapToggle.isOn    = false;
		m_DoubleToggle.isOn = false;
		m_HoldToggle.isOn   = false;
	}

	void ToggleTapCreation(bool _Value)
	{
		if (Expand())
		{
			m_TapToggle.SetIsOnWithoutNotify(false);
			return;
		}
		
		m_CreateTapHandle.gameObject.SetActive(_Value);
	}

	void ToggleDoubleCreation(bool _Value)
	{
		if (Expand())
		{
			m_DoubleToggle.SetIsOnWithoutNotify(false);
			return;
		}
		
		m_CreateDoubleHandle.gameObject.SetActive(_Value);
	}

	void ToggleHoldCreation(bool _Value)
	{
		if (Expand())
		{
			m_HoldToggle.SetIsOnWithoutNotify(false);
			return;
		}
		
		m_CreateHoldHandle.gameObject.SetActive(_Value);
	}

	void ToggleColorCreation(bool _Value)
	{
		if (Expand())
		{
			m_ColorToggle.SetIsOnWithoutNotify(false);
			return;
		}
		
		m_CreateColorHandle.gameObject.SetActive(_Value);
	}
}
