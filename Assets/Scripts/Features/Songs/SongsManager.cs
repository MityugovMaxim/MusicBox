using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongsManager : IDataManager
{
	public SongsCollection Collection => m_SongsCollection;

	public ProfileSongs Profile => m_ProfileSongs;

	[Inject] SongsCollection   m_SongsCollection;
	[Inject] ProfileSongs      m_ProfileSongs;
	[Inject] DifficultyManager m_DifficultyManager;
	[Inject] ScoresManager     m_ScoresManager;
	[Inject] MenuProcessor     m_MenuProcessor;
	[Inject] AudioProcessor    m_AudioProcessor;

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			Collection.Load,
			Profile.Load,
			CreateChannel
		);
	}

	public void SubscribeState(Action _Action) => m_AudioProcessor.SubscribeState(AudioChannelType.Preview, _Action);

	public void UnsubscribeState(Action _Action) => m_AudioProcessor.UnsubscribeState(AudioChannelType.Preview, _Action);

	public void SubscribeTrack(Action _Action) => m_AudioProcessor.SubscribeTrack(AudioChannelType.Preview, _Action);

	public void UnsubscribeTrack(Action _Action) => m_AudioProcessor.UnsubscribeTrack(AudioChannelType.Preview, _Action);

	public void Play(string _SongID)
	{
		if (!Collection.Contains(_SongID))
			return;
		
		m_AudioProcessor.Play(
			AudioChannelType.Preview,
			new AudioTrack(
				_SongID,
				GetTitle(_SongID),
				GetArtist(_SongID),
				GetPreview(_SongID)
			)
		);
	}

	public void Stop() => m_AudioProcessor.Stop(AudioChannelType.Preview);

	public string GetID() => m_AudioProcessor.GetID(AudioChannelType.Preview);

	public AudioChannelState GetState() => m_AudioProcessor.GetState(AudioChannelType.Preview);

	public List<string> GetSongIDs()
	{
		return Collection
			.GetIDs()
			.ToList();
	}

	public List<string> GetAvailableSongIDs()
	{
		return Collection.GetIDs()
			.Where(IsFree)
			.OrderBy(m_ScoresManager.GetRank)
			.ThenBy(GetRank)
			.ThenBy(Collection.GetOrder)
			.ToList();
	}

	public List<string> GetPaidSongIDs()
	{
		return Collection.GetIDs()
			.Where(IsPaid)
			.OrderBy(GetPrice)
			.ThenBy(GetRank)
			.ThenBy(Collection.GetOrder)
			.ToList();
	}

	public List<string> GetChestSongIDs()
	{
		return Collection.GetIDs()
			.Where(IsChest)
			.OrderBy(GetPrice)
			.ThenBy(GetRank)
			.ThenBy(Collection.GetOrder)
			.ToList();
	}

	public string GetSongID(string _SongHash)
	{
		if (string.IsNullOrEmpty(_SongHash))
			return null;
		
		foreach (string songID in Collection.GetIDs())
		{
			if (string.IsNullOrEmpty(songID))
				continue;
			
			string songHash = CRC32.Get(songID);
			
			if (songHash == _SongHash)
				return songID;
		}
		
		return null;
	}

	public bool IsFree(string _SongID) => Profile.Contains(_SongID) || GetMode(_SongID) == SongMode.Free;

	public bool IsPaid(string _SongID) => !IsFree(_SongID) && GetMode(_SongID) == SongMode.Paid;

	public bool IsChest(string _SongID) => !IsFree(_SongID) && GetMode(_SongID) == SongMode.Chest;

	public string GetArtist(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Artist ?? string.Empty;
	}

	public string GetTitle(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Title ?? string.Empty;
	}

	public string GetImage(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetPreview(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Preview ?? string.Empty;
	}

	public string GetMusic(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Music ?? string.Empty;
	}

	public string GetASF(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.ASF ?? string.Empty;
	}

	public float GetSpeed(string _SongID) => m_DifficultyManager.GetSpeed(GetRank(_SongID));

	public RankType GetRank(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Rank ?? RankType.None;
	}

	public long GetCoins(string _SongID, RankType _ScoreRank) => m_DifficultyManager.GetCoins(GetRank(_SongID), _ScoreRank);

	public int GetThreshold(string _SongID, RankType _ScoreRank) => m_DifficultyManager.GetThreshold(GetRank(_SongID), _ScoreRank);

	public RankType GetRank(string _SongID, int _Accuracy) => m_DifficultyManager.GetRank(GetRank(_SongID), _Accuracy);

	public long GetPrice(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Price ?? 0;
	}

	public SongMode GetMode(string _SongID)
	{
		SongSnapshot snapshot = Collection.GetSnapshot(_SongID);
		
		return snapshot?.Mode ?? SongMode.Free;
	}

	public async Task<bool> Collect(string _SongID)
	{
		if (string.IsNullOrEmpty(_SongID))
			return false;
		
		if (Profile.Contains(_SongID))
			return false;
		
		if (IsPaid(_SongID) || IsChest(_SongID))
			return false;
		
		Log.Info(this, "Collecting song with ID '{0}'...", _SongID);
		
		SongCollectRequest request = new SongCollectRequest(_SongID);
		
		bool success = await request.SendAsync();
		
		if (success)
			return true;
		
		await m_MenuProcessor.ErrorAsync("song_collect");
		
		return false;
	}

	Task CreateChannel()
	{
		AudioChannelSettings settings = new AudioChannelSettings();
		settings.Shuffle = false;
		settings.Repeat  = false;
		settings.Loop    = true;
		
		m_AudioProcessor.RegisterChannel(AudioChannelType.Preview, settings);
		
		return Task.CompletedTask;
	}
}
