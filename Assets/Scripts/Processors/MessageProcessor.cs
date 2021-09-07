using Firebase.Messaging;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class MessageProcessor : IInitializable
{
	void IInitializable.Initialize()
	{
		FirebaseMessaging.TokenReceived   += OnTokenReceived;
		FirebaseMessaging.MessageReceived += OnMessageReceived;
		
		iOSNotificationCenter.ApplicationBadge = 0;
	}

	static void OnTokenReceived(object _Sender, TokenReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received registration token: '{0}'.", _Args.Token);
	}

	static void OnMessageReceived(object _Sender, MessageReceivedEventArgs _Args)
	{
		Debug.LogFormat("[MessageProcessor] Received message. Sender: {0}.", _Args.Message.From);
	}
}