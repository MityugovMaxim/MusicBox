using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class LevelDataUpdateSignal { }

[Preserve]
public class LevelProcessor : IInitializable, IDisposable
{
	public bool Initialized { get; private set; }

	Level  m_Level;
	string m_LevelID;

	readonly SignalBus         m_SignalBus;
	readonly PurchaseProcessor m_PurchaseProcessor;
	readonly ProgressProcessor m_ProgressProcessor;
	readonly Level.Factory     m_LevelFactory;
	readonly ProductInfo       m_NoAdsProduct;

	readonly List<string>                      m_LevelIDs        = new List<string>();
	readonly Dictionary<string, LevelSnapshot> m_LevelSnapshots  = new Dictionary<string, LevelSnapshot>();
	readonly List<ISampleReceiver>             m_SampleReceivers = new List<ISampleReceiver>();

	DatabaseReference m_LevelsData;

	[Inject]
	public LevelProcessor(
		SignalBus         _SignalBus,
		PurchaseProcessor _PurchaseProcessor,
		ProgressProcessor _ProgressProcessor,
		Level.Factory     _LevelFactory,
		ProductInfo       _NoAdsProduct 
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_ProgressProcessor = _ProgressProcessor;
		m_LevelFactory      = _LevelFactory;
		m_NoAdsProduct      = _NoAdsProduct;
	}

	public List<string> GetLevelIDs()
	{
		return m_LevelIDs.Where(_LevelID => m_PurchaseProcessor.IsLevelPurchased(_LevelID))
			.OrderByDescending(m_ProgressProcessor.IsLevelUnlocked)
			.ThenBy(m_ProgressProcessor.GetExpRequired)
			.ToList();
	}

	public bool Contains(string _LevelID)
	{
		return m_LevelSnapshots.ContainsKey(_LevelID);
	}

	public string GetArtist(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get artist failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelSnapshot.Artist;
	}

	public string GetTitle(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get title failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelSnapshot.Title;
	}

	public string GetLeaderboardID(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get leaderboard ID failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		// TODO: Remove
		//return levelSnapshot.LeaderboardID;
		return string.Empty;
	}

	public string GetAchievementID(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get achievement ID failed. Level info with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		// TODO: Remove
		// return levelSnapshot.AchievementID;
		return string.Empty;
	}

	public string GetNextLevelID(string _LevelID)
	{
		List<string> levelIDs = GetLevelIDs();
		
		if (levelIDs == null || levelIDs.Count == 0)
			return _LevelID;
		
		int index = levelIDs.IndexOf(_LevelID);
		
		if (index < 0)
			return _LevelID;
		
		index = MathUtility.Repeat(index + 1, levelIDs.Count);
		
		return levelIDs[index];
	}

	public string GetPreviousLevelID(string _LevelID)
	{
		List<string> levelIDs = GetLevelIDs();
		
		if (levelIDs == null || levelIDs.Count == 0)
			return _LevelID;
		
		int index = levelIDs.IndexOf(_LevelID);
		
		if (index < 0)
			return _LevelID;
		
		index = MathUtility.Repeat(index - 1, levelIDs.Count);
		
		return levelIDs[index];
	}

	public LevelMode GetLevelMode(string _LevelID)
	{
		if (m_NoAdsProduct != null && m_PurchaseProcessor.IsProductPurchased(m_NoAdsProduct.ID))
			return LevelMode.Free;
		
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get mode failed. Level info with ID '{0}' is null.", _LevelID);
			return LevelMode.Free;
		}
		
		return levelSnapshot.Mode;
	}

	public void Create(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogError("[LevelProvider] Create level failed. Level info is null.");
			return;
		}
		
		if (m_Level != null)
		{
			Debug.LogErrorFormat("[LevelProvider] Create level failed. Level instance '{0}' already created.", m_Level.name);
			return;
		}
		
		m_LevelFactory.Create(
			$"{_LevelID}/level",
			_Level =>
			{
				m_Level   = _Level;
				m_LevelID = _LevelID;
				
				m_Level.RegisterSampleReceivers(m_SampleReceivers.ToArray());
				
				m_SignalBus.Fire(new LevelStartSignal(m_LevelID));
			}
		);
	}

	public void Remove()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Remove level failed. Level is null.");
			return;
		}
		
		m_SignalBus.Fire(new LevelExitSignal(m_LevelID));
		
		GameObject.Destroy(m_Level.gameObject);
		
		m_Level   = null;
		m_LevelID = null;
	}

	public void Play()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Play level failed. Level is null.");
			return;
		}
		
		m_Level.Play(() => m_SignalBus.Fire(new LevelFinishSignal(m_LevelID)));
		
		m_SignalBus.Fire(new LevelPlaySignal(m_LevelID));
	}

	public void Pause()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Pause level failed. Level is null.");
			return;
		}
		
		m_Level.Pause();
	}

	public void Restart()
	{
		if (m_Level == null)
		{
			Debug.LogError("[LevelProvider] Restart level failed. Level is null.");
			return;
		}
		
		m_Level.Stop();
		
		m_SignalBus.Fire(new LevelRestartSignal(m_LevelID));
	}

	public void AddSampleReceiver(ISampleReceiver _SampleReceiver)
	{
		m_SampleReceivers.Add(_SampleReceiver);
	}

	public void RemoveSampleReceiver(ISampleReceiver _SampleReceiver)
	{
		m_SampleReceivers.Remove(_SampleReceiver);
	}

	async void IInitializable.Initialize()
	{
		m_LevelsData = FirebaseDatabase.DefaultInstance.RootReference.Child("levels");
		
		await LoadLevels();
		
		m_LevelsData.ChildChanged += OnLevelsUpdate;
		
		Initialized = true;
	}

	void IDisposable.Dispose()
	{
		m_LevelsData.ChildChanged -= OnLevelsUpdate;
		
		Initialized = false;
	}

	async void OnLevelsUpdate(object _Sender, EventArgs _Args)
	{
		await LoadLevels();
		
		m_SignalBus.Fire<LevelDataUpdateSignal>();
	}

	async Task LoadLevels()
	{
		DataSnapshot levelsSnapshot = await m_LevelsData.GetValueAsync();
		
		m_LevelIDs.Clear();
		m_LevelSnapshots.Clear();
		
		foreach (DataSnapshot levelSnapshot in levelsSnapshot.Children)
		{
			string levelID = levelSnapshot.Key;
			LevelSnapshot level = new LevelSnapshot(
				levelSnapshot.Child("title").GetString(),
				levelSnapshot.Child("artist").GetString(),
				levelSnapshot.Child("mode").GetEnum<LevelMode>()
			);
			
			m_LevelIDs.Add(levelID);
			m_LevelSnapshots[levelID] = level;
		}
	}

	LevelSnapshot GetLevelSnapshot(string _LevelID)
	{
		if (string.IsNullOrEmpty(_LevelID))
		{
			Debug.LogError("[ProgressProcessor] Get level info failed. Level ID is null or empty.");
			return null;
		}
		
		if (!m_LevelSnapshots.ContainsKey(_LevelID))
		{
			Debug.LogErrorFormat("[ProgressProcessor] Get level info failed. Level with ID '{0}' not found.", _LevelID);
			return null;
		}
		
		return m_LevelSnapshots[_LevelID];
	}
}