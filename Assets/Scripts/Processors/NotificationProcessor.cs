using System;
using Unity.Notifications.iOS;
using UnityEngine;
using Zenject;

// TODO: Merge with MessageProcessor
public abstract class NotificationProcessor : IInitializable
{
	public string LaunchURL { get; private set; }

	[Inject]
	public NotificationProcessor() { }

	protected abstract string GetLaunchURL();

	protected abstract void RemoveScheduledNotifications();

	public abstract void ScheduleNotification(
		string   _ID,
		string   _Title,
		string   _Subtitle,
		string   _URL,
		TimeSpan _Time
	);

	void IInitializable.Initialize()
	{
		LaunchURL = GetLaunchURL();
		
		if (string.IsNullOrEmpty(LaunchURL))
			LaunchURL = Application.absoluteURL;
		
		RemoveScheduledNotifications();
	}
}

public class iOSNotificationProcessor : NotificationProcessor
{
	[Inject]
	public iOSNotificationProcessor() { }

	protected override string GetLaunchURL()
	{
		iOSNotification notification = iOSNotificationCenter.GetLastRespondedNotification();
		
		return notification?.Data;
	}

	protected override void RemoveScheduledNotifications()
	{
		iOSNotificationCenter.RemoveAllScheduledNotifications();
	}

	public override void ScheduleNotification(
		string   _ID,
		string   _Title,
		string   _Subtitle,
		string   _URL,
		TimeSpan _Time
	)
	{
		iOSNotification notification = new iOSNotification()
		{
			Identifier                   = _ID,
			Title                        = _Title,
			Subtitle                     = _Subtitle,
			Trigger                      = new iOSNotificationTimeIntervalTrigger() { Repeats = false, TimeInterval = _Time },
			ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Badge | PresentationOption.Sound,
			ShowInForeground             = true,
			Badge                        = 1,
			Data                         = _URL,
		};
		
		iOSNotificationCenter.ScheduleNotification(notification);
	}
}