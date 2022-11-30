using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BadgeManager
{
	readonly Dictionary<string, bool> m_Cache = new Dictionary<string, bool>();

	string UserID => m_SocialProcessor != null ? m_SocialProcessor.UserID : string.Empty;

	[Inject] SocialProcessor m_SocialProcessor;

	readonly DataEventHandler m_ReadHandler   = new DataEventHandler();
	readonly DataEventHandler m_UnreadHandler = new DataEventHandler();

	public void SubscribeRead(string _Group, Action _Action) => m_ReadHandler.AddListener(_Group, _Action);

	public void SubscribeUnread(string _Group, Action _Action) => m_UnreadHandler.AddListener(_Group, _Action);

	public void UnsubscribeRead(string _Group, Action _Action) => m_ReadHandler.RemoveListener(_Group, _Action);

	public void UnsubscribeUnread(string _Group, Action _Action) => m_UnreadHandler.RemoveListener(_Group, _Action);

	public void Read(string _Group, string _ID)
	{
		if (string.IsNullOrEmpty(_Group) || string.IsNullOrEmpty(_ID))
			return;
		
		string key = GetKey(_Group, _ID);
		
		if (m_Cache.TryGetValue(key, out bool value) && value)
			return;
		
		m_Cache[key] = true;
		
		PlayerPrefs.SetInt(key, 1);
		
		m_ReadHandler.Invoke(_Group);
	}

	public void Unread(string _Group, string _ID)
	{
		if (string.IsNullOrEmpty(_Group) || string.IsNullOrEmpty(_ID))
			return;
		
		string key = GetKey(_Group, _ID);
		
		if (m_Cache.TryGetValue(key, out bool value) && !value)
			return;
		
		m_Cache[key] = false;
		
		PlayerPrefs.SetInt(key, 0);
		
		m_UnreadHandler.Invoke(_Group);
	}

	public bool IsRead(string _Group, string _ID)
	{
		if (string.IsNullOrEmpty(_Group) || string.IsNullOrEmpty(_ID))
			return false;
		
		string key = GetKey(_Group, _ID);
		
		if (m_Cache.TryGetValue(key, out bool value))
			return value;
		
		value = PlayerPrefs.GetInt(key, 0) > 0;
		
		m_Cache[key] = value;
		
		return value;
	}

	public bool IsUnread(string _Group, string _ID)
	{
		if (string.IsNullOrEmpty(_Group) || string.IsNullOrEmpty(_ID))
			return false;
		
		string key = GetKey(_Group, _ID);
		
		if (m_Cache.TryGetValue(key, out bool value))
			return !value;
		
		value = PlayerPrefs.GetInt(key, 0) > 0;
		
		m_Cache[key] = value;
		
		return !value;
	}

	string GetKey(string _Group, string _ID) => $"BADGE_{UserID}_{_Group}_{_ID}";
}
