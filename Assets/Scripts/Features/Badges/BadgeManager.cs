using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class BadgeManager
{
	const int    READ_DELAY     = 500;
	const string SONGS_GROUP    = "SONGS_GROUP";
	const string NEWS_GROUP     = "NEWS_GROUP";
	const string PRODUCTS_GROUP = "PRODUCTS_GROUP";
	const string CHESTS_GROUP   = "SEASONS_GROUP";
	const string VOUCHERS_GROUP = "VOUCHERS_GROUP";

	readonly Dictionary<string, bool> m_Cache = new Dictionary<string, bool>();

	string UserID => m_SocialProcessor != null ? m_SocialProcessor.UserID : string.Empty;

	[Inject] SocialProcessor m_SocialProcessor;

	readonly DataEventHandler m_BadgeHandler = new DataEventHandler();

	public void SubscribeNews(Action _Action) => Subscribe(NEWS_GROUP, _Action);

	public void UnsubscribeNews(Action _Action) => Unsubscribe(NEWS_GROUP, _Action);

	public void SubscribeProducts(Action _Action) => Subscribe(PRODUCTS_GROUP, _Action);

	public void UnsubscribeProducts(Action _Action) => Unsubscribe(PRODUCTS_GROUP, _Action);

	public void SubscribeSongs(Action _Action) => Subscribe(SONGS_GROUP, _Action);

	public void UnsubscribeSongs(Action _Action) => Unsubscribe(SONGS_GROUP, _Action);

	public void SubscribeChests(Action _Action) => Subscribe(CHESTS_GROUP, _Action);

	public void UnsubscribeChests(Action _Action) => Unsubscribe(CHESTS_GROUP, _Action);

	public void SubscribeVouchers(Action _Action) => Subscribe(VOUCHERS_GROUP, _Action);

	public void UnsubscribeVouchers(Action _Action) => Unsubscribe(VOUCHERS_GROUP, _Action);

	public void ReadSong(string _SongID) => Read(SONGS_GROUP, _SongID);

	public void UnreadSongs(IEnumerable<string> _SongIDs) => Unread(SONGS_GROUP, _SongIDs);

	public bool IsSongUnread(string _SongID) => IsUnread(SONGS_GROUP, _SongID);

	public void ReadProduct(string _ProductID) => Read(PRODUCTS_GROUP, _ProductID);

	public void UnreadProduct(IEnumerable<string> _ProductIDs) => Unread(PRODUCTS_GROUP, _ProductIDs);

	public bool IsProductUnread(string _ProductID) => IsUnread(PRODUCTS_GROUP, _ProductID);

	public void ReadChest(string _ChestID) => Read(CHESTS_GROUP, _ChestID);

	public void UnreadChest(IEnumerable<string> _ChestIDs) => Unread(CHESTS_GROUP, _ChestIDs);

	public bool IsChestUnread(string _ChestID) => IsUnread(CHESTS_GROUP, _ChestID);

	public void ReadNews(string _NewsID) => Read(NEWS_GROUP, _NewsID);

	public bool IsNewsUnread(string _NewsID) => IsUnread(NEWS_GROUP, _NewsID);

	public void ReadVoucher(string _VoucherID) => Read(VOUCHERS_GROUP, _VoucherID);

	public bool IsVoucherUnread(string _VoucherID) => IsUnread(VOUCHERS_GROUP, _VoucherID);

	void Subscribe(string _Group, Action _Action) => m_BadgeHandler.AddListener(_Group, _Action);

	void Unsubscribe(string _Group, Action _Action) => m_BadgeHandler.RemoveListener(_Group, _Action);

	async void Read(string _Group, string _ID)
	{
		if (string.IsNullOrEmpty(_Group) || string.IsNullOrEmpty(_ID))
			return;
		
		string key = GetKey(_Group, _ID);
		
		if (m_Cache.TryGetValue(key, out bool value) && value)
			return;
		
		m_Cache[key] = true;
		
		PlayerPrefs.SetInt(key, 1);
		
		await Task.Delay(READ_DELAY);
		
		m_BadgeHandler.Invoke(_Group);
	}

	void Unread(string _Group, IEnumerable<string> _IDs)
	{
		if (string.IsNullOrEmpty(_Group) || _IDs == null)
			return;
		
		foreach (string id in _IDs)
		{
			if (string.IsNullOrEmpty(id))
				continue;
			
			string key = GetKey(_Group, id);
			
			if (m_Cache.TryGetValue(key, out bool value) && !value)
				return;
			
			m_Cache[key] = false;
			
			PlayerPrefs.SetInt(key, 0);
		}
		
		m_BadgeHandler.Invoke(_Group);
	}

	bool IsUnread(string _Group, string _ID)
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
