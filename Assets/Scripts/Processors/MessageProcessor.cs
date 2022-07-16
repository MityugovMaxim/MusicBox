using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Messaging;
using UnityEngine.Purchasing.MiniJSON;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public abstract class MessageProcessor : IInitializable, IDisposable
{
	const string TOPICS_LANGUAGE_KEY = "TOPICS_LANGUAGE";

	static string Topic
	{
		get => PlayerPrefs.GetString(TOPICS_LANGUAGE_KEY, string.Empty);
		set => PlayerPrefs.SetString(TOPICS_LANGUAGE_KEY, value);
	}

	[Inject] SignalBus         m_SignalBus;
	[Inject] LanguageProcessor m_LanguageProcessor;
	[Inject] UrlProcessor      m_UrlProcessor;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LanguageSelectSignal>(RegisterLanguageSelect);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LanguageSelectSignal>(RegisterLanguageSelect);
	}

	async void RegisterLanguageSelect()
	{
		await LoadTopic();
	}

	public async Task LoadMessages(string _URL)
	{
		try
		{
			await FirebaseMessaging.RequestPermissionAsync();
			
			await LoadTopic();
		}
		catch (Exception exception)
		{
			Debug.LogError("[MessageProcessor] Process permission failed.");
			Debug.LogException(exception);
		}
		
		FirebaseMessaging.TokenReceived   += OnTokenReceived;
		FirebaseMessaging.MessageReceived += OnMessageReceived;
		
		LoadNotifications();
		
		if (string.IsNullOrEmpty(_URL))
			return;
		
		await m_UrlProcessor.ProcessURL(_URL);
	}

	public async Task LoadTopic()
	{
		if (Topic == m_LanguageProcessor.Language)
			return;
		
		Topic = m_LanguageProcessor.Language;
		
		List<Task> tasks = new List<Task>();
		foreach (string language in m_LanguageProcessor.GetLanguages())
		{
			if (language != Topic)
				tasks.Add(FirebaseMessaging.UnsubscribeAsync(language));
		}
		
		await Task.WhenAll(tasks);
		
		await FirebaseMessaging.SubscribeAsync(Topic);
		
		Debug.Log("[MessageProcessor] Process topics complete.");
	}

	protected abstract void LoadNotifications();

	public void Schedule(string _Name, string _Title, string _Message, string _URL, TimeSpan _Span)
	{
		long timestamp = DateTimeOffset.Now.Add(_Span).ToUnixTimeMilliseconds();
		
		Schedule(
			_Name,
			_Title,
			_Message,
			_URL,
			timestamp
		);
	}

	public abstract void Schedule(
		string _Name,
		string _Title,
		string _Message,
		string _URL,
		long   _Timestamp
	);

	static void OnTokenReceived(object _Sender, TokenReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received token: '{0}'.", _Args.Token);
	}

	void OnMessageReceived(object _Sender, MessageReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received message: '{0}'.", _Args.Message.MessageType);
		
		if (_Args.Message.NotificationOpened && _Args.Message.Data.TryGetValue("url", out string url))
			OpenURL(url);
	}

	protected async void OpenURL(string _URL)
	{
		if (string.IsNullOrEmpty(_URL))
			return;
		
		await m_UrlProcessor.ProcessURL(_URL);
	}
}

#if UNITY_ANDROID
[Preserve]
public class AndroidMessageProcessor : MessageProcessor
{
	const string COMMON_CHANNEL_ID = "common";

	public override void Schedule(
		string _Name,
		string _Title,
		string _Message,
		string _URL,
		long   _Timestamp
	)
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["url"] = _URL;
		
		AndroidNotification notification = new AndroidNotification()
		{
			Title      = _Title,
			Text       = _Message,
			IntentData = Json.Serialize(data),
			FireTime   = TimeUtility.GetLocalTime(_Timestamp),
			Number     = 1,
		};
		
		CancelNotification(_Name);
		
		ScheduleNotification(_Name, notification);
	}

	protected override void LoadNotifications()
	{
		AndroidNotificationChannel channel = new AndroidNotificationChannel()
		{
			Id                   = COMMON_CHANNEL_ID,
			Name                 = "Common Channel",
			Importance           = Importance.High,
			Description          = "Common notifications",
			CanShowBadge         = true,
			LockScreenVisibility = LockScreenVisibility.Public,
			CanBypassDnd         = false,
			EnableLights         = true,
			EnableVibration      = true
		};
		
		AndroidNotificationCenter.Initialize();
		
		AndroidNotificationCenter.RegisterNotificationChannel(channel);
		
		ProcessNotification();
		
		AndroidNotificationCenter.CancelAllDisplayedNotifications();
		
		AndroidNotificationCenter.OnNotificationReceived += _Intent =>
		{
			if (_Intent == null)
				return;
			
			int notificationID = _Intent.Id;
			
			AndroidNotificationCenter.CancelDisplayedNotification(notificationID);
		};
	}

	void ProcessNotification()
	{
		AndroidNotificationIntentData intent = AndroidNotificationCenter.GetLastNotificationIntent();
		if (intent == null)
			return;
		
		AndroidNotification notification = intent.Notification;
		
		Dictionary<string, object> data = Json.Deserialize(notification.IntentData) as Dictionary<string, object>;
		if (data == null)
			return;
		
		string url = data.GetString("url");
		if (string.IsNullOrEmpty(url))
			return;
		
		OpenURL(url);
	}

	static void CancelNotification(string _Name)
	{
		if (string.IsNullOrEmpty(_Name))
			return;
		
		int notificationID = _Name.GetHashCode();
		
		NotificationStatus status = AndroidNotificationCenter.CheckScheduledNotificationStatus(notificationID);
		
		if (status != NotificationStatus.Scheduled && status != NotificationStatus.Delivered)
			return;
		
		AndroidNotificationCenter.CancelScheduledNotification(notificationID);
	}

	static void ScheduleNotification(string _Name, AndroidNotification _Notification)
	{
		if (string.IsNullOrEmpty(_Name))
			return;
		
		int notificationID = _Name.GetHashCode();
		
		NotificationStatus status = AndroidNotificationCenter.CheckScheduledNotificationStatus(notificationID);
		
		if (status == NotificationStatus.Scheduled)
			AndroidNotificationCenter.UpdateScheduledNotification(notificationID, _Notification, COMMON_CHANNEL_ID);
		else
			AndroidNotificationCenter.SendNotificationWithExplicitID(_Notification, COMMON_CHANNEL_ID, notificationID);
	}
}
#endif

#if UNITY_IOS
[Preserve]
public class iOSMessageProcessor : MessageProcessor
{
	public override void Schedule(
		string _Name,
		string _Title,
		string _Message,
		string _URL,
		long   _Timestamp
	)
	{
		DateTimeOffset date = DateTimeOffset
			.FromUnixTimeMilliseconds(_Timestamp)
			.ToLocalTime();
		
		iOSNotification notification = new iOSNotification()
		{
			Identifier = _Name,
			Title = _Title,
			Body  = _Message,
			Badge = 1,
			ShowInForeground = false,
			Trigger = new iOSNotificationCalendarTrigger()
			{
				Year    = date.Year,
				Month   = date.Month,
				Day     = date.Day,
				Hour    = date.Hour,
				Minute  = date.Minute,
				Second  = date.Second,
				Repeats = false,
				UtcTime = false,
			},
		};
		
		Dictionary<string, object> data = new Dictionary<string, object>()
		{
			{ "url", _URL },
		};
		
		notification.Data = Json.Serialize(data);
		
		iOSNotificationCenter.RemoveScheduledNotification(_Name);
		
		iOSNotificationCenter.ScheduleNotification(notification);
	}

	protected override void LoadNotifications()
	{
		ProcessNotification();
		
		iOSNotificationCenter.ApplicationBadge = 0;
		
		iOSNotificationCenter.RemoveAllDeliveredNotifications();
	}

	void ProcessNotification()
	{
		iOSNotification notification = iOSNotificationCenter.GetLastRespondedNotification();
		if (notification == null)
			return;
		
		Dictionary<string, object> data = Json.Deserialize(notification.Data) as Dictionary<string, object>;
		if (data == null)
			return;
		
		string url = data.GetString("url");
		if (string.IsNullOrEmpty(url))
			return;
		
		OpenURL(url);
	}
}
#endif