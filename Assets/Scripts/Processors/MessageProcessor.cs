using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Messaging;
using ModestTree;
using Unity.Notifications.iOS;
using UnityEngine;
using Zenject;

public abstract class MessageProcessor : IInitializable
{
	const string TOPICS_LANGUAGE_KEY = "TOPICS_LANGUAGE";

	static string TopicsLanguage
	{
		get => PlayerPrefs.GetString(TOPICS_LANGUAGE_KEY, string.Empty);
		set => PlayerPrefs.SetString(TOPICS_LANGUAGE_KEY, value);
	}

	static readonly string[] m_Topics =
	{
		"news",
		"offers",
		"version",
	};

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

	protected abstract string GetLaunchURL();

	protected abstract void RemoveScheduledNotifications();

	void IInitializable.Initialize()
	{
		RemoveScheduledNotifications();
		
		iOSNotificationCenter.ApplicationBadge = 0;
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

	public async Task ProcessTopics()
	{
		if (TopicsLanguage == m_LanguageProcessor.Language)
			return;
		
		TopicsLanguage = m_LanguageProcessor.Language;
		
		List<Task> tasks = new List<Task>();
		foreach (string language in m_LanguageProcessor.SupportedLanguages.Except(TopicsLanguage))
		foreach (string topic in m_Topics)
			tasks.Add(FirebaseMessaging.UnsubscribeAsync($"{topic}_{language}"));
		
		await Task.WhenAll(tasks);
		
		tasks.Clear();
		
		foreach (string topic in m_Topics)
			tasks.Add(FirebaseMessaging.UnsubscribeAsync($"{topic}_{TopicsLanguage}"));
		
		await Task.WhenAll(tasks);
	}

	public async Task ProcessLaunchURL()
	{
		await m_UrlProcessor.ProcessURL(GetLaunchURL());
	}

	static void OnTokenReceived(object _Sender, TokenReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received notification token: '{0}'.", _Args.Token);
	}

	static void OnMessageReceived(object _Sender, MessageReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received message. Sender: {0}.", _Args.Message.From);
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
		
		return notification?.Data;
	}

	protected override void RemoveScheduledNotifications()
	{
		iOSNotificationCenter.RemoveAllScheduledNotifications();
	}
}