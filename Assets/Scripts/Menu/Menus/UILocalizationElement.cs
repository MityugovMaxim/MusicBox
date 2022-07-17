using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UILocalizationElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UILocalizationElement> { }

	[SerializeField] TMP_Text m_KeyLabel;
	[SerializeField] TMP_Text m_ValueLabel;
	[SerializeField] Button   m_RemoveButton;

	[Inject] MenuProcessor m_MenuProcessor;

	string           m_Key;
	LocalizationData m_Localization;

	protected override void Awake()
	{
		base.Awake();
		
		m_RemoveButton.onClick.AddListener(Remove);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_RemoveButton.onClick.RemoveListener(Remove);
	}

	public void Setup(string _Key, LocalizationData _Localization)
	{
		m_Key          = _Key;
		m_Localization = _Localization;
		
		m_KeyLabel.text   = m_Key;
		m_ValueLabel.text = m_Localization.GetValue(m_Key);
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	void Open()
	{
		UILocalizationEditMenu localizationEditMenu = m_MenuProcessor.GetMenu<UILocalizationEditMenu>();
		
		if (localizationEditMenu == null)
			return;
		
		localizationEditMenu.Setup(m_Key, m_Localization);
		
		localizationEditMenu.Show();
	}

	void Remove()
	{
		m_Localization.Remove(m_Key);
	}
}