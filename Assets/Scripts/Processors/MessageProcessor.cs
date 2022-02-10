using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Messaging;
using ModestTree;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using Zenject;

public abstract class MessageProcessor : IInitializable
{
	const string TOPICS_LANGUAGE_KEY = "TOPICS_LANGUAGE";

	static string Topic
	{
		get => PlayerPrefs.GetString(TOPICS_LANGUAGE_KEY, string.Empty);
		set => PlayerPrefs.SetString(TOPICS_LANGUAGE_KEY, value);
	}

	readonly LanguageProcessor m_LanguageProcessor;
	readonly UrlProcessor      m_UrlProcessor;

	protected MessageProcessor(
		LanguageProcessor _LanguageProcessor,
		UrlProcessor      _UrlProcessor
	)
	{
		m_LanguageProcessor = _LanguageProcessor;
		m_UrlProcessor      = _UrlProcessor;
	}

	protected virtual string GetLaunchURL() => Application.absoluteURL;

	protected abstract void RemoveScheduledNotifications();

	void IInitializable.Initialize()
	{
		RemoveScheduledNotifications();
	}

	public async Task ProcessPermission()
	{
		try
		{
			await FirebaseMessaging.RequestPermissionAsync();
		}
		catch (Exception exception)
		{
			Debug.LogError("[MessageProcessor] Process permission failed.");
			Debug.LogException(exception);
		}
		
		FirebaseMessaging.TokenReceived   += OnTokenReceived;
		FirebaseMessaging.MessageReceived += OnMessageReceived;
	}

	public async Task ProcessTopic()
	{
		if (Topic == m_LanguageProcessor.Language)
			return;
		
		Topic = m_LanguageProcessor.Language;
		
		List<Task> tasks = new List<Task>();
		foreach (string language in m_LanguageProcessor.SupportedLanguages.Except(Topic))
			tasks.Add(FirebaseMessaging.UnsubscribeAsync(language));
		
		await Task.WhenAll(tasks);
		
		await FirebaseMessaging.SubscribeAsync(Topic);
		
		Debug.Log("[MessageProcessor] Process topics complete.");
	}

	public async Task ProcessLaunchURL(bool _Instant = false)
	{
		await m_UrlProcessor.ProcessURL(GetLaunchURL(), _Instant);
	}

	static void OnTokenReceived(object _Sender, TokenReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received notification token: '{0}'.", _Args.Token);
	}

	async void OnMessageReceived(object _Sender, MessageReceivedEventArgs _Args)
	{
		if (!_Args.Message.Data.TryGetValue("url", out string url))
			return;
		
		await m_UrlProcessor.ProcessURL(url);
	}
}

public class iOSMessageProcessor : MessageProcessor
{
	[Inject]
	public iOSMessageProcessor(
		LanguageProcessor _LanguageProcessor,
		UrlProcessor      _UrlProcessor
	) : base(_LanguageProcessor, _UrlProcessor) { }

	protected override string GetLaunchURL()
	{
		iOSNotification notification = iOSNotificationCenter.GetLastRespondedNotification();
		
		if (notification?.Data == null)
			return Application.absoluteURL;
		
		Dictionary<string, object> data = Json.Deserialize(notification.Data) as Dictionary<string, object>;
		
		Debug.LogError("---> DATA: " + notification.Data);
		
		return data.GetString("url", Application.absoluteURL);
	}

	protected override void RemoveScheduledNotifications()
	{
		iOSNotificationCenter.RemoveAllScheduledNotifications();
		
		iOSNotificationCenter.ApplicationBadge = 0;
	}
}