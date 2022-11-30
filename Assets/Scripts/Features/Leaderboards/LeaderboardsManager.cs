using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Compression;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.Scripting;
using UnityEngine.SocialPlatforms.Impl;
using Zenject;

public class LeaderboardEntry
{
	public string Name   { get; }
	public string Image  { get; }
	public int    Place  { get; }
	public long   Score  { get; }

	public LeaderboardEntry(IDictionary<string, object> _Data)
	{
		Name  = _Data.GetString("name");
		Image = _Data.GetString("image");
		Place = _Data.GetInt("place");
		Score = _Data.GetLong("score");
	}
}

public class Leaderboard
{
	public string                 SongID  { get; }
	public List<LeaderboardEntry> Entries { get; }

	public Leaderboard(IDictionary<string, object> _Data)
	{
		SongID = _Data.GetString("song_id");
		
		Entries = _Data.GetKeys()
			.Select(_Key => _Data.GetDictionary(_Key))
			.Select(_Entry => new LeaderboardEntry(_Entry))
			.ToList();
	}
}

public class LeaderboardRequest : FunctionRequest<Leaderboard>
{
	protected override string Command => "Leaderboard";

	readonly string m_SongID;

	public LeaderboardRequest(string _SongID)
	{
		m_SongID = _SongID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["song_id"] = m_SongID;
	}

	protected override Leaderboard Success(object _Data)
	{
		if (_Data == null)
			return null;
		
		IDictionary<string, object> data = MiniJson.JsonDecode((string)_Data) as IDictionary<string, object>;
		
		return new Leaderboard(data);
	}

	protected override Leaderboard Fail()
	{
		return null;
	}
}

public class LeaderboardsManager
{
	[Inject] MenuProcessor m_MenuProcessor;

	public async Task<Leaderboard> GetLeaderboardAsync(string _SongID)
	{
		LeaderboardRequest request = new LeaderboardRequest(_SongID);
		
		Leaderboard leaderboard = await request.SendAsync();
		
		if (leaderboard != null)
			return leaderboard;
		
		m_MenuProcessor.ErrorAsync("leaderboard");
		
		return null;
	}
}

public class UILeaderboardMenu : UIMenu
{
	[SerializeField] UILayout m_Content;
	[SerializeField] UIGroup  m_ContentGroup;
	[SerializeField] UIGroup  m_LoaderGroup;

	[Inject] LeaderboardsManager m_LeaderboardsManager;

	[Inject] UILeaderboardGapElement.Pool   m_GapPool;
	[Inject] UILeaderboardEntryElement.Pool m_EntryPool;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Refresh();
	}

	async void Refresh()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Hide(true);
		
		await Task.Delay(1000);
		
		Leaderboard leaderboard = await m_LeaderboardsManager.GetLeaderboardAsync(m_SongID);
		
		if (leaderboard == null || leaderboard.Entries == null)
			return;
		
		m_Content.Clear();
		
		if (leaderboard.Entries.Count > 0)
			m_Content.Add(new LeaderboardEntryEntity(leaderboard.Entries[0], m_EntryPool));
		
		for (int i = 1; i < leaderboard.Entries.Count; i++)
		{
			LeaderboardEntry source = leaderboard.Entries[i - 1];
			LeaderboardEntry target = leaderboard.Entries[i];
			
			int gap = Mathf.Abs(source.Place - target.Place);
			
			if (gap > 1)
				m_Content.Add(new LeaderboardGapEntity(m_GapPool));
			
			m_Content.Add(new LeaderboardEntryEntity(target, m_EntryPool));
		}
		
		m_Content.Reposition();
	}
}

public class LeaderboardGapEntity : LayoutEntity
{
	public override string  ID   => "leaderboard_gap";
	public override Vector2 Size => m_Pool.Size;

	readonly UILeaderboardGapElement.Pool m_Pool;

	UILeaderboardGapElement m_Item;

	public LeaderboardGapEntity(UILeaderboardGapElement.Pool _Pool)
	{
		m_Pool = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}

public class LeaderboardEntryEntity : LayoutEntity
{
	public override string  ID                               { get; }
	public override Vector2 Size                             { get; }

	readonly LeaderboardEntry               m_Entry;
	readonly UILeaderboardEntryElement.Pool m_Pool;

	UILeaderboardEntryElement m_Item;
	
	public LeaderboardEntryEntity(LeaderboardEntry _Entry, UILeaderboardEntryElement.Pool _Pool)
	{
		m_Entry = _Entry;
		m_Pool  = _Pool;
	}

	public override void Create(RectTransform _Container)
	{
		if (m_Item == null)
			m_Item = m_Pool.Spawn(_Container);
		
		m_Item.SetRect(Rect);
		
		m_Item.Setup(m_Entry);
	}

	public override void Remove()
	{
		if (m_Item != null)
			m_Pool.Despawn(m_Item);
		m_Item = null;
	}
}

public class UILeaderboardGapElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UILeaderboardGapElement> { }
}

public class UILeaderboardEntryElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UILeaderboardEntryElement> { }

	[SerializeField] TMP_Text    m_Place;
	[SerializeField] GameObject  m_FirstPlace;
	[SerializeField] GameObject  m_SecondPlace;
	[SerializeField] GameObject  m_ThirdPlace;
	[SerializeField] WebImage    m_Image;
	[SerializeField] TMP_Text    m_Name;
	[SerializeField] UIUnitLabel m_Score;

	public void Setup(LeaderboardEntry _Entry)
	{
		m_Place.text  = _Entry.Place.ToString();
		m_Image.Path  = _Entry.Image;
		m_Name.text   = _Entry.Name;
		m_Score.Value = _Entry.Score;
		
		m_FirstPlace.SetActive(_Entry.Place == 1);
		m_SecondPlace.SetActive(_Entry.Place == 2);
		m_ThirdPlace.SetActive(_Entry.Place == 3);
	}
}
