using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NotificationProcessor : IInitializable, IDisposable
{
	public class NotificationInfo
	{
		public Sprite Sprite      { get; }
		public string Title       { get; }
		public string Description { get; }
		public Action Action      { get; }

		public NotificationInfo(
			Sprite _Sprite,
			string _Title,
			string _Description,
			Action _Action
		)
		{
			Sprite      = _Sprite;
			Title       = _Title;
			Description = _Description;
			Action      = _Action;
		}
	}

	public event Action OnNotification;

	readonly SignalBus              m_SignalBus;
	readonly LevelProcessor         m_LevelProcessor;
	readonly PurchaseProcessor      m_PurchaseProcessor;
	readonly MenuProcessor          m_MenuProcessor;
	readonly List<NotificationInfo> m_NotificationInfos = new List<NotificationInfo>();

	[Inject]
	public NotificationProcessor(
		SignalBus         _SignalBus,
		LevelProcessor    _LevelProcessor,
		PurchaseProcessor _PurchaseProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_SignalBus         = _SignalBus;
		m_LevelProcessor    = _LevelProcessor;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_MenuProcessor     = _MenuProcessor;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelUnlockSignal>(RegisterLevelUnlock);
		m_SignalBus.Subscribe<PurchaseSignal>(RegisterPurchase);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelUnlockSignal>(RegisterLevelUnlock);
		m_SignalBus.Unsubscribe<PurchaseSignal>(RegisterPurchase);
	}

	void RegisterLevelUnlock(LevelUnlockSignal _Signal)
	{
		NotificationInfo notificationInfo = new NotificationInfo(
			m_LevelProcessor.GetPreviewThumbnail(_Signal.LevelID),
			"Level unlocked",
			$"{m_LevelProcessor.GetArtist(_Signal.LevelID)}\n{m_LevelProcessor.GetTitle(_Signal.LevelID)}",
			() =>
			{
				m_LevelProcessor.Remove();
				
				UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>(MenuType.LevelMenu);
				if (levelMenu != null)
					levelMenu.Setup(_Signal.LevelID);
				
				m_MenuProcessor.Show(MenuType.MainMenu)
					.ThenHide(MenuType.ResultMenu, true)
					.ThenHide(MenuType.GameMenu, true)
					.ThenHide(MenuType.PauseMenu, true);
				m_MenuProcessor.Show(MenuType.LevelMenu);
			}
		);
		
		m_NotificationInfos.Add(notificationInfo);
		
		OnNotification?.Invoke();
	}

	void RegisterPurchase(PurchaseSignal _Signal)
	{
		NotificationInfo notificationInfo = new NotificationInfo(
			m_PurchaseProcessor.GetPreviewThumbnail(_Signal.ProductID),
			"Purchase complete",
			m_PurchaseProcessor.GetTitle(_Signal.ProductID),
			() =>
			{
				string[] levelIDs = m_PurchaseProcessor.GetLevelIDs(_Signal.ProductID);
				
				if (levelIDs == null || levelIDs.Length == 0)
					return;
				
				string levelID = levelIDs[0];
				
				m_LevelProcessor.Remove();
				
				UILevelMenu levelMenu = m_MenuProcessor.GetMenu<UILevelMenu>(MenuType.LevelMenu);
				if (levelMenu != null)
					levelMenu.Setup(levelID);
				
				m_MenuProcessor.Show(MenuType.MainMenu)
					.ThenHide(MenuType.ResultMenu, true)
					.ThenHide(MenuType.GameMenu, true)
					.ThenHide(MenuType.PauseMenu, true);
				m_MenuProcessor.Show(MenuType.LevelMenu);
			}
		);
		
		m_NotificationInfos.Add(notificationInfo);
		
		OnNotification?.Invoke();
	}

	public NotificationInfo GetNotificationInfo()
	{
		while (m_NotificationInfos.Count > 0)
		{
			NotificationInfo notificationInfo = m_NotificationInfos[0];
			
			if (notificationInfo == null)
			{
				m_NotificationInfos.RemoveAt(0);
				continue;
			}
			
			m_NotificationInfos.RemoveAt(0);
			
			return notificationInfo;
		}
		return null;
	}
}