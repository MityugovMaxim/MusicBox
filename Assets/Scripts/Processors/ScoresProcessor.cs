using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public enum ScoreRank
{
	None     = 0,
	Bronze   = 1,
	Silver   = 2,
	Gold     = 3,
	Platinum = 4,
}

public class ScoreSnapshot
{
	public string    LevelID  { get; }
	public int       Accuracy { get; }
	public long      Score    { get; }
	public ScoreRank Rank     { get; }
	public int       Rating   { get; }

	public ScoreSnapshot(DataSnapshot _Data)
	{
		LevelID  = _Data.Key;
		Accuracy = _Data.GetInt("accuracy");
		Score    = _Data.GetLong("score");
		Rank     = _Data.GetEnum<ScoreRank>("rank");
		Rating   = _Data.GetInt("rating");
	}
}

[Preserve]
public class ScoresDataUpdateSignal { }

[Preserve]
public class ScoresProcessor
{
	bool Loaded { get; set; }

	[Inject] SignalBus       m_SignalBus;
	[Inject] SocialProcessor m_SocialProcessor;

	readonly List<ScoreSnapshot> m_Snapshots = new List<ScoreSnapshot>();

	DatabaseReference m_Data;

	public async Task Load()
	{
		if (m_Data != null && m_Data.Key != m_SocialProcessor.UserID)
		{
			Loaded              =  false;
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("scores").Child(m_SocialProcessor.UserID);
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public int GetAccuracy(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Accuracy ?? 0;
	}

	public long GetScore(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Score ?? 0;
	}

	public ScoreRank GetRank(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Rank ?? ScoreRank.None;
	}

	public int GetRating(string _SongID)
	{
		ScoreSnapshot snapshot = GetSnapshot(_SongID);
		
		return snapshot?.Rating ?? 0;
	}

	void Unload()
	{
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded || m_Data.Key != m_SocialProcessor.UserID)
			return;
		
		if (FirebaseAuth.DefaultInstance.CurrentUser == null)
		{
			Unload();
			return;
		}
		
		Log.Info(this, "Updating scores data...");
		
		await Fetch();
		
		Log.Info(this, "Update scores data complete.");
		
		m_SignalBus.Fire<ScoresDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch scores failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new ScoreSnapshot(_Data)));
	}

	ScoreSnapshot GetSnapshot(string _SongID)
	{
		if (m_Snapshots == null || m_Snapshots.Count == 0)
			return null;
		
		if (string.IsNullOrEmpty(_SongID))
		{
			Log.Error(this, "Get score snapshot failed. Song ID is null or empty.");
			return null;
		}
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.LevelID == _SongID);
	}
}