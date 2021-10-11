using System;
using Unity.Notifications.iOS;
using UnityEngine;
using Zenject;

// TODO: Merge with MessageProcessor
public abstract class NotificationProcessor : IInitializable
{
	public string LaunchURL { get; private set; }

	readonly OffersProcessor m_OffersProcessor;

	[Inject]
	public NotificationProcessor(OffersProcessor _OffersProcessor)
	{
		m_OffersProcessor = _OffersProcessor;
	}

	public void OfferNotification(string _OfferID)
	{
		string levelID = m_OffersProcessor.GetLevelID(_OfferID);
		long   coins   = m_OffersProcessor.GetCoins(_OfferID);
		string title   = m_OffersProcessor.GetTitle(_OfferID);
		
		bool hasLevelID = !string.IsNullOrEmpty(levelID);
		bool hasCoins   = coins > 0;
		
		string text;
		if (hasLevelID && hasCoins)
			text = "Get your new track and free coins right now!";
		else if (hasLevelID)
			text = "Get your new track right now!";
		else if (hasCoins)
			text = "Get your free coins right now!";
		else
			text = "Get your reward right now!";
		
		ScheduleNotification(
			"offer_" + _OfferID,
			title,
			text,
			"audiobox://offers",
			TimeSpan.FromDays(1)
		);
	}

	protected abstract string GetLaunchURL();

	protected abstract void RemoveScheduledNotifications();

	protected abstract void ScheduleNotification(
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
	public iOSNotificationProcessor(OffersProcessor _OffersProcessor) : base(_OffersProcessor) { }

	protected override string GetLaunchURL()
	{
		iOSNotification notification = iOSNotificationCenter.GetLastRespondedNotification();
		
		return notification?.Data;
	}

	protected override void RemoveScheduledNotifications()
	{
		iOSNotificationCenter.RemoveAllScheduledNotifications();
	}

	protected override void ScheduleNotification(
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