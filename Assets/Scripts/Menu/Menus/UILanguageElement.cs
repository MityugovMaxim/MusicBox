using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UILanguageElement : UIOverlayButton
{
	[Preserve]
	public class Pool : UIEntityPool<UILanguageElement> { }

	[SerializeField] TMP_Text m_Label;

	[Inject] LanguagesManager m_LanguagesManager;
	[Inject] MenuProcessor    m_MenuProcessor;

	LocalizationData m_Localization;

	public void Setup(LocalizationData _Localization)
	{
		m_Localization = _Localization;
		
		m_Label.text = m_LanguagesManager.GetName(m_Localization.Language);
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	void Open()
	{
		UILocalizationMenu localizationMenu = m_MenuProcessor.GetMenu<UILocalizationMenu>();
		
		if (localizationMenu == null)
			return;
		
		localizationMenu.Setup(m_Localization);
		
		localizationMenu.Show();
	}
}
