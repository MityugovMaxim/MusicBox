using System;
using TMPro;
using UnityEngine;
using Zenject;

[Menu(MenuType.ErrorMenu)]
public class UIErrorMenu : UIMenu
{
	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Message;

	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	Action m_Action;

	[Inject]
	public void Construct(
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup(string _Reason, string _Title, string _Message, Action _Action = null)
	{
		m_Title.text   = _Title;
		m_Message.text = _Message;
		m_Action       = _Action;
		
		m_StatisticProcessor.LogErrorMenuShow(_Reason);
	}

	public async void Close()
	{
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		await m_MenuProcessor.Hide(MenuType.ErrorMenu);
		
		Action action = m_Action;
		m_Action = null;
		action?.Invoke();
	}

	protected override void OnShowStarted()
	{
		m_HapticProcessor.Process(Haptic.Type.Failure);
	}
}