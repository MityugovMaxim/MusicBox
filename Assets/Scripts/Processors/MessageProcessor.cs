using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Messaging;
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

	public async Task LoadMessages()
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
		
		ClearBadges();
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

	protected abstract void ClearBadges();

	static void OnTokenReceived(object _Sender, TokenReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received token: '{0}'.", _Args.Token);
	}

	async void OnMessageReceived(object _Sender, MessageReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received message: '{0}'.", _Args.Message.MessageType);
		
		if (_Args.Message.NotificationOpened && _Args.Message.Data.TryGetValue("url", out string url))
			await m_UrlProcessor.ProcessURL(url);
	}
}

#if UNITY_ANDROID
[Preserve]
public class AndroidMessageProcessor : MessageProcessor
{
	protected override void ClearBadges() { }
}
#endif

#if UNITY_IOS
[Preserve]
public class iOSMessageProcessor : MessageProcessor
{
	protected override void ClearBadges()
	{
		Unity.Notifications.iOS.iOSNotificationCenter.RemoveAllScheduledNotifications();
		
		Unity.Notifications.iOSiOSNotificationCenter.ApplicationBadge = 0;
	}
}
#endif