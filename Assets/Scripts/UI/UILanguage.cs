using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILanguage : UIEntity
{
	[Serializable]
	public class LanguageData
	{
		public SystemLanguage Language => m_Language;
		public UIGroup        Check    => m_Check;
		public Button         Button   => m_Button;

		[SerializeField] SystemLanguage m_Language;
		[SerializeField] UIGroup        m_Check;
		[SerializeField] Button         m_Button;
	}

	[SerializeField] LanguageData[] m_Languages;

	LanguageProcessor  m_LanguageProcessor;
	MessageProcessor   m_MessageProcessor;
	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	[Inject]
	public void Construct(
		LanguageProcessor  _LanguageProcessor,
		MessageProcessor   _MessageProcessor,
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_LanguageProcessor  = _LanguageProcessor;
		m_MessageProcessor   = _MessageProcessor;
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
		
		foreach (LanguageData language in m_Languages)
		{
			if (language == null)
				continue;
			
			string languageCode = language.Language.GetCode();
			
			language.Button.gameObject.SetActive(m_LanguageProcessor.SupportedLanguages.Contains(languageCode));
			
			if (languageCode == m_LanguageProcessor.Language)
			{
				language.Check.Show(true);
				Scale(language.Button, 1.2f, true);
			}
			else
			{
				language.Check.Hide(true);
				Scale(language.Button, 0.9f, true);
			}
			
			if (language.Button != null)
				language.Button.onClick.AddListener(() => Select(language.Language));
		}
	}

	async void Select(SystemLanguage _Language)
	{
		if (m_LanguageProcessor.Language == _Language.GetCode())
			return;
		
		m_StatisticProcessor.LogMainMenuProfilePageLanguageClick(_Language.GetCode());
		
		m_HapticProcessor.Process(Haptic.Type.Success);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		List<Task> tasks = new List<Task>();
		foreach (LanguageData language in m_Languages)
		{
			if (language.Language == _Language)
			{
				tasks.Add(language.Check.ShowAsync());
				tasks.Add(ScaleAsync(language.Button, 1.2f));
			}
			else
			{
				tasks.Add(language.Check.HideAsync());
				tasks.Add(ScaleAsync(language.Button, 0.9f));
			}
		}
		
		await Task.WhenAll(tasks);
		
		if (m_LanguageProcessor.SelectLanguage(_Language.GetCode()))
		{
			await m_MenuProcessor.Show(MenuType.LoginMenu);
			
			await m_MenuProcessor.Hide(MenuType.MainMenu, true);
			
			await Task.WhenAll(
				m_LanguageProcessor.LoadLocalization(),
				Task.Delay(1500)
			);
			
			await m_MessageProcessor.ProcessTopic();
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			
			await m_MenuProcessor.Hide(MenuType.LoginMenu);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	static async void Scale(Component _Component, float _Scale, bool _Instant = false)
	{
		await ScaleAsync(_Component, _Scale, _Instant);
	}

	static async Task ScaleAsync(Component _Component, float _Scale, bool _Instant = false)
	{
		if (_Component == null)
			return;
		
		Transform transform = _Component.transform;
		Vector3   source    = transform.localScale;
		Vector3   target    = new Vector3(_Scale, _Scale, 1);
		
		void Animation(float _Phase) => transform.localScale = Vector3.Lerp(source, target, _Phase);
		
		if (_Instant)
			Animation(1);
		else
			await UnityTask.Phase(Animation, 0.25f);
	}
}