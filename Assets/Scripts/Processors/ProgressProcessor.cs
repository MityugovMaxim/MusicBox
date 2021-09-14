using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using Zenject;

public class WalletDataUpdateSignal { }

public class ProgressProcessor : IInitializable, IDisposable
{
	public long Coins { get; private set; }

	readonly SignalBus         m_SignalBus;
	readonly SocialProcessor   m_SocialProcessor;
	readonly LevelProcessor    m_LevelProcessor;
	readonly ScoreProcessor    m_ScoreProcessor;
	readonly PurchaseProcessor m_PurchaseProcessor;

	readonly List<string> m_LevelIDs = new List<string>();

	DatabaseReference m_WalletData;

	[Inject]
	public ProgressProcessor(
		SignalBus       _SignalBus,
		SocialProcessor _SocialProcessor,
		LevelProcessor  _LevelProcessor,
		ScoreProcessor  _ScoreProcessor
	)
	{
		m_SignalBus       = _SignalBus;
		m_LevelProcessor  = _LevelProcessor;
		m_ScoreProcessor  = _ScoreProcessor;
		m_SocialProcessor = _SocialProcessor;
	}

	public async Task LoadWallet()
	{
		if (m_WalletData == null)
			m_WalletData = FirebaseDatabase.DefaultInstance.RootReference.Child("wallets").Child(m_SocialProcessor.UserID);
		
		await FetchWallet();
		
		m_WalletData.ValueChanged += OnWalletUpdate;
	}

	async void OnWalletUpdate(object _Sender, EventArgs _Args)
	{
		Debug.Log("[ProgressProcessor] Updating wallet data...");
		
		await FetchWallet();
		
		Debug.Log("[ProgressProcessor] Update wallet data complete.");
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelFinishSignal>(RegisterLevelFinish);
	}

	async void RegisterLevelFinish(LevelFinishSignal _Signal)
	{
		long payout = GetPayout(_Signal.LevelID);
		
		foreach (ScoreRank rank in Enum.GetValues(typeof(ScoreRank)))
		{
			if (rank != ScoreRank.None && rank <= m_ScoreProcessor.Rank)
				Coins += payout * GetPayoutMultiplier(m_ScoreProcessor.Rank);
		}
		
		await SaveWallet();
		
		m_SignalBus.Fire<WalletDataUpdateSignal>();
	}

	public long GetPayout(string _LevelID)
	{
		return m_LevelProcessor.GetPayout(_LevelID);
	}

	public long GetPayout(string _LevelID, ScoreRank _Rank)
	{
		return GetPayout(_LevelID) * GetPayoutMultiplier(_Rank);
	}

	public long GetPrice(string _LevelID)
	{
		return m_LevelProcessor.GetPrice(_LevelID);
	}

	public bool IsLevelLocked(string _LevelID)
	{
		return m_LevelIDs.Contains(_LevelID);
	}

	public bool IsLevelUnlocked(string _LevelID)
	{
		return !m_LevelIDs.Contains(_LevelID);
	}

	public async Task UnlockLevel(string _LevelID)
	{
		long price = GetPrice(_LevelID);
		
		if (price > Coins)
		{
			Debug.LogErrorFormat("[ProgressProcessor] Unlock level failed. Not enough coins. Required: {0}. Current: {1}.", price, Coins);
			return;
		}
		
		Coins -= price;
		
		m_LevelIDs.Add(_LevelID);
		
		try
		{
			await SaveWallet();
			
			m_SignalBus.Fire(new LevelUnlockSignal(_LevelID));
			
			m_SignalBus.Fire<WalletDataUpdateSignal>();
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[ProgressProcessor] Unlock level failed. Error: {0}.", exception.Message);
			
			Coins += price;
			
			m_LevelIDs.Remove(_LevelID);
		}
	}

	async Task FetchWallet()
	{
		m_LevelIDs.Clear();
		
		DataSnapshot walletSnapshot = await m_WalletData.GetValueAsync();
		
		Coins = walletSnapshot.GetLong("coins");
		
		List<string> levelIDs = walletSnapshot.GetChildKeys("levels");
		foreach (string levelID in levelIDs)
			m_LevelIDs.Add(levelID);
	}

	async Task SaveWallet()
	{
		Dictionary<string, object> wallet = new Dictionary<string, object>();
		
		wallet["coins"] = Coins;
		
		Dictionary<string, object> levels = new Dictionary<string, object>();
		foreach (string levelID in m_LevelIDs)
			levels[levelID] = true;
		
		wallet["levels"] = levels;
		
		await m_WalletData.SetValueAsync(wallet);
	}

	static int GetPayoutMultiplier(ScoreRank _Rank)
	{
		switch (_Rank)
		{
			case ScoreRank.S:
				return 3;
			case ScoreRank.A:
				return 2;
			case ScoreRank.B:
				return 1;
			case ScoreRank.C:
				return 1;
			default:
				return 0;
		}
	}
}
