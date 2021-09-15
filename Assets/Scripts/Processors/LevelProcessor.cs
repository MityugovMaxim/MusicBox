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
public class LevelProcessor
{
	Level  m_Level;
	string m_LevelID;

	readonly SignalBus         m_SignalBus;
	readonly PurchaseProcessor m_PurchaseProcessor;
	[Preserve]
	readonly StorageProcessor  m_StorageProcessor;
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
		StorageProcessor  _StorageProcessor,
		Level.Factory     _LevelFactory,
		ProductInfo       _NoAdsProduct 
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_StorageProcessor  = _StorageProcessor;
		m_LevelFactory      = _LevelFactory;
		m_NoAdsProduct      = _NoAdsProduct;
	}

	public async Task LoadLevels()
	{
		if (m_LevelsData == null)
			m_LevelsData = FirebaseDatabase.DefaultInstance.RootReference.Child("levels");
		
		await FetchLevels();
		
		m_LevelsData.ValueChanged += OnLevelsUpdate;
	}

	public List<string> GetLevelIDs()
	{
		return m_LevelIDs.Where(_LevelID => m_PurchaseProcessor.IsLevelPurchased(_LevelID)).ToList();
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
			Debug.LogErrorFormat("[LevelProcessor] Get title failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return string.Empty;
		}
		
		return levelSnapshot.Title;
	}

	public long GetPayout(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get payout failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelSnapshot.Payout;
	}

	public long GetPrice(string _LevelID)
	{
		LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
		
		if (levelSnapshot == null)
		{
			Debug.LogErrorFormat("[LevelProcessor] Get price failed. Level snapshot with ID '{0}' is null.", _LevelID);
			return 0;
		}
		
		return levelSnapshot.Price;
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

	public async void Create(string _LevelID)
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
		
		Track[] tracks = await m_StorageProcessor.LoadTracks(_LevelID);
		
		Level prefab = Resources.Load<Level>(levelSnapshot.Skin);
		
		m_LevelID = _LevelID;
		m_Level   = m_LevelFactory.Create(prefab);
		
		m_Level.Setup(
			levelSnapshot.Length,
			levelSnapshot.BPM,
			levelSnapshot.Speed,
			tracks
		);
		
		m_Level.RegisterSampleReceivers(m_SampleReceivers.ToArray());
		
		m_SignalBus.Fire(new LevelStartSignal(m_LevelID));
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

	async void OnLevelsUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[LevelProcessor] Updating levels data...");
		
		await FetchLevels();
		
		Debug.Log("[LevelProcessor] Update levels data complete.");
		
		m_SignalBus.Fire<LevelDataUpdateSignal>();
	}

	async Task FetchLevels()
	{
		DataSnapshot levelsSnapshot = await m_LevelsData.GetValueAsync();
		
		m_LevelIDs.Clear();
		m_LevelSnapshots.Clear();
		
		foreach (DataSnapshot levelSnapshot in levelsSnapshot.Children)
		{
			#if DEVELOPMENT_BUILD || UNITY_EDITOR
			bool active = true;
			#else
			bool active = levelSnapshot.GetBool("active");
			#endif
			
			if (!active)
				continue;
			
			string levelID = levelSnapshot.Key;
			
			LevelSnapshot level = new LevelSnapshot(
				true,
				levelSnapshot.GetString("title", string.Empty),
				levelSnapshot.GetString("artist", string.Empty),
				levelSnapshot.GetEnum<LevelMode>("mode"),
				levelSnapshot.GetFloat("length"),
				levelSnapshot.GetFloat("bpm"),
				levelSnapshot.GetFloat("speed"),
				levelSnapshot.GetBool("locked"),
				levelSnapshot.GetLong("payout"),
				levelSnapshot.GetLong("price"),
				levelSnapshot.GetString("skin", "level")
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