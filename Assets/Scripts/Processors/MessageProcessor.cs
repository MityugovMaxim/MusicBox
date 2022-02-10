using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Messaging;
using ModestTree;
using Unity.Notifications.iOS;
using UnityEngine;
using Zenject;

public abstract class MessageProcessor
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
		foreach (string language in m_LanguageProcessor.SupportedLanguages.Except(Topic))
			tasks.Add(FirebaseMessaging.UnsubscribeAsync(language));
		
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

public class iOSMessageProcessor : MessageProcessor
{
	[Inject]
	public iOSMessageProcessor(
		LanguageProcessor _LanguageProcessor,
		UrlProcessor      _UrlProcessor
	) : base(_LanguageProcessor, _UrlProcessor) { }

	protected override void ClearBadges()
	{
		iOSNotificationCenter.RemoveAllScheduledNotifications();
		
		iOSNotificationCenter.ApplicationBadge = 0;
	}
}