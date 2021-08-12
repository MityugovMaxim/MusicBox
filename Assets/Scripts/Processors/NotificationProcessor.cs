using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class NotificationProcessor : IInitializable, IDisposable
{
	class NotificationData
	{
		public Sprite Sprite      { get; }
		public string Title       { get; }
		public string Description { get; }
		public Action Action      { get; }

		public NotificationData(Sprite _Sprite, string _Title, string _Description, Action _Action)
		{
			Sprite      = _Sprite;
			Title       = _Title;
			Description = _Description;
			Action      = _Action;
		}
	}

	readonly SignalBus               m_SignalBus;
	readonly LevelProcessor          m_LevelProcessor;
	readonly MenuProcessor           m_MenuProcessor;
	readonly UINotification          m_Notification;
	readonly Queue<NotificationData> m_Notifications = new Queue<NotificationData>();

	[Inject]
	public NotificationProcessor(
		SignalBus      _SignalBus,
		Canvas         _Canvas,
		LevelProcessor _LevelProcessor,
		MenuProcessor  _MenuProcessor
	)
	{
		m_SignalBus      = _SignalBus;
		m_LevelProcessor = _LevelProcessor;
		m_MenuProcessor  = _MenuProcessor;
		
		UINotification notification = Resources.Load<UINotification>("notification");
		if (notification != null)
			m_Notification = Object.Instantiate(notification, _Canvas.transform, false);
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelUnlockSignal>(RegisterLevelUnlock);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelUnlockSignal>(RegisterLevelUnlock);
	}

	void RegisterLevelUnlock(LevelUnlockSignal _Signal)
	{
		NotificationData notificationData = new NotificationData(
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
		
		m_Notifications.Enqueue(notificationData);
		
		if (m_Notifications.Count == 1)
			Execute();
	}

	void Execute()
	{
		while (m_Notifications.Count > 0)
		{
			NotificationData notificationData = m_Notifications.Peek();
			
			if (notificationData == null)
				continue;
			
			m_Notification.Setup(
				notificationData.Sprite,
				notificationData.Title,
				notificationData.Description,
				notificationData.Action
			);
			
			m_Notification.Play(
				() =>
				{
					m_Notifications.Dequeue();
					
					Execute();
				}
			);
			
			return;
		}
	}
}