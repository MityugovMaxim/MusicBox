using System;
using UnityEngine;
using Zenject;

public class UINotificationMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UINotification m_Notification;

	NotificationProcessor m_NotificationProcessor;

	bool m_Playing;

	[Inject]
	public void Construct(NotificationProcessor _NotificationProcessor)
	{
		m_NotificationProcessor = _NotificationProcessor;
	}

	void IInitializable.Initialize()
	{
		m_NotificationProcessor.OnNotification += Execute;
	}

	void IDisposable.Dispose()
	{
		m_NotificationProcessor.OnNotification -= Execute;
	}

	protected override void OnShowFinished()
	{
		base.OnShowFinished();
		
		Execute();
	}

	void Execute()
	{
		if (!Shown || m_Playing)
			return;
		
		NotificationProcessor.NotificationInfo notificationInfo = m_NotificationProcessor.GetNotificationInfo();
		
		if (notificationInfo == null)
			return;
		
		m_Playing = true;
		
		m_Notification.Setup(
			notificationInfo.Sprite,
			notificationInfo.Title,
			notificationInfo.Description,
			notificationInfo.Action
		);
		
		m_Notification.Play(
			() =>
			{
				m_Playing = false;
				
				Execute();
			}
		);
	}
}