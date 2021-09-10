using UnityEngine.Scripting;

public class LevelSnapshot
{
	public string Title { get; }

	public string Artist { get; }

	public LevelMode Mode { get; }

	public LevelSnapshot(
		string    _Title,
		string    _Artist,
		LevelMode _LevelMode
	)
	{
		Title  = _Title;
		Artist = _Artist;
		Mode   = _LevelMode;
	}
}

public class ScoreSnapshot
{
	public int       Accuracy { get; }
	public long      Score    { get; }
	public ScoreRank Rank     { get; }

	public ScoreSnapshot(
		int       _Accuracy,
		long      _Score,
		ScoreRank _Rank
	)
	{
		Accuracy = _Accuracy;
		Score    = _Score;
		Rank     = _Rank;
	}
}

public class WalletSnapshot
{
	public long Coins => m_Coins;

	readonly long m_Coins;

	public WalletSnapshot(long _Coins)
	{
		m_Coins = _Coins;
	}
}

[Preserve]
public class DataProcessor// : IInitializable
{
	// readonly SignalBus m_SignalBus;
	//
	// DatabaseReference m_Database;
	// DatabaseReference m_Levels;
	// DatabaseReference m_Scores;
	// DatabaseReference m_Wallet;
	//
	// LevelSnapshot[] m_LevelSnapshots;
	// ScoreSnapshot[] m_ScoreSnapshots;
	// WalletSnapshot  m_WalletSnapshot;
	//
	// [Inject]
	// public DataProcessor(SignalBus _SignalBus)
	// {
	// 	m_SignalBus = _SignalBus;
	// }
	//
	// public string GetLevelArtist(string _LevelID)
	// {
	// 	LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
	// 	
	// 	return levelSnapshot.Artist;
	// }
	//
	// public string GetLevelTitle(string _LevelID)
	// {
	// 	LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
	// 	
	// 	return levelSnapshot.Title;
	// }
	//
	// public LevelMode GetLevelMode(string _LevelID)
	// {
	// 	LevelSnapshot levelSnapshot = GetLevelSnapshot(_LevelID);
	// 	
	// 	return levelSnapshot.Mode;
	// }
	//
	// public long GetLevelScore(string _LevelID)
	// {
	// 	ScoreSnapshot scoreSnapshot = GetScoreSnapshot(_LevelID);
	// 	
	// 	return scoreSnapshot.Score;
	// }
	//
	// public int GetLevelAccuracy(string _LevelID)
	// {
	// 	ScoreSnapshot scoreSnapshot = GetScoreSnapshot(_LevelID);
	// 	
	// 	return scoreSnapshot.Accuracy;
	// }
	//
	// public ScoreRank GetLevelRank(string _LevelID)
	// {
	// 	ScoreSnapshot scoreSnapshot = GetScoreSnapshot(_LevelID);
	// 	
	// 	return scoreSnapshot.Rank;
	// }
	//
	// LevelSnapshot GetLevelSnapshot(string _LevelID)
	// {
	// 	
	// }
	//
	// ScoreSnapshot GetScoreSnapshot(string _LevelID)
	// {
	// 	
	// }
	//
	// void IInitializable.Initialize()
	// {
	// 	m_Database = FirebaseDatabase.DefaultInstance.RootReference;
	// 	m_Levels   = m_Database.Child("levels");
	// 	m_Scores   = m_Database.Child("scores/$uid");
	// 	m_Wallet   = m_Database.Child("wallet/$uid");
	// 	
	// 	Load();
	// 	
	// 	m_Levels.ChildChanged += OnLevelChange;
	// 	m_Scores.ValueChanged += OnScoresChange;
	// 	m_Wallet.ValueChanged += OnWalletChange;
	// }
	//
	// async void Load()
	// {
	// 	var loadLevelsTask = LoadLevelsAsync();
	// 	var loadScoresTask = LoadScoresAsync();
	// 	var loadWalletTask = LoadWalletAsync();
	// 	
	// 	List<Task> tasks = new List<Task>();
	// 	tasks.Add(loadLevelsTask);
	// 	tasks.Add(loadScoresTask);
	// 	tasks.Add(loadWalletTask);
	// 	
	// 	while (tasks.Count > 0)
	// 	{
	// 		Task task = await Task.WhenAny(tasks);
	// 		
	// 		if (task == loadLevelsTask)
	// 		{
	// 			Debug.Log("[DataProcessor] Load levels complete.");
	// 			m_LevelSnapshots = loadLevelsTask.Result;
	// 		}
	// 		else if (task == loadScoresTask)
	// 		{
	// 			Debug.Log("[DataProcessor] Load scores complete.");
	// 			m_ScoreSnapshots = loadScoresTask.Result;
	// 		}
	// 		else if (task == loadWalletTask)
	// 		{
	// 			Debug.Log("[DataProcessor] Load wallet complete.");
	// 			m_WalletSnapshot = loadWalletTask.Result;
	// 		}
	// 		
	// 		tasks.Remove(task);
	// 	}
	// 	
	// 	m_SignalBus.Fire<DataLoadSignal>();
	// }
	//
	// async void OnLevelChange(object _Sender, EventArgs _Args)
	// {
	// 	m_LevelSnapshots = await LoadLevelsAsync();
	// 	
	// 	m_SignalBus.Fire<DataLevelsUpdateSignal>();
	// }
	//
	// async void OnScoresChange(object _Sender, EventArgs _Args)
	// {
	// 	m_ScoreSnapshots = await LoadScoresAsync();
	// 	
	// 	m_SignalBus.Fire<DataScoresUpdateSignal>();
	// }
	//
	// async void OnWalletChange(object _Sender, EventArgs _Args)
	// {
	// 	m_WalletSnapshot = await LoadWalletAsync();
	// 	
	// 	m_SignalBus.Fire<DataWalletUpdateSignal>();
	// }
	//
	// async Task<LevelSnapshot[]> LoadLevelsAsync()
	// {
	// 	DataSnapshot levelsSnapshot = await m_Levels.GetValueAsync();
	// 	
	// 	List<LevelSnapshot> levels = new List<LevelSnapshot>();
	// 	foreach (DataSnapshot levelSnapshot in levelsSnapshot.Children)
	// 	{
	// 		LevelSnapshot level = new LevelSnapshot(
	// 			levelSnapshot.Key,
	// 			(string)levelSnapshot.Child("title").Value,
	// 			(string)levelSnapshot.Child("artist").Value,
	// 			(LevelMode)levelSnapshot.Child("mode").Value
	// 		);
	// 		levels.Add(level);
	// 	}
	// 	
	// 	return levels.ToArray();
	// }
	//
	// async Task<ScoreSnapshot[]> LoadScoresAsync()
	// {
	// 	DataSnapshot scoresSnapshot = await m_Scores.GetValueAsync();
	// 	
	// 	List<ScoreSnapshot> scores = new List<ScoreSnapshot>();
	// 	foreach (DataSnapshot scoreSnapshot in scoresSnapshot.Children)
	// 	{
	// 		ScoreSnapshot score = new ScoreSnapshot(
	// 			scoreSnapshot.Key,
	// 			(long)scoreSnapshot.Child("score").Value,
	// 			(int)scoreSnapshot.Child("accuracy").Value,
	// 			(ScoreRank)scoreSnapshot.Child("rank").Value
	// 		);
	// 		scores.Add(score);
	// 	}
	// 	
	// 	return scores.ToArray();
	// }
	//
	// async Task<WalletSnapshot> LoadWalletAsync()
	// {
	// 	DataSnapshot walletSnapshot = await m_Wallet.GetValueAsync();
	// 	
	// 	WalletSnapshot wallet = new WalletSnapshot(
	// 		(long)walletSnapshot.Child("coins").Value
	// 	);
	// 	
	// 	return wallet;
	// }
	//
	// async Task SaveCoinsAsync(long _Coins)
	// {
	// 	DatabaseReference coinsReference = m_Wallet.Child("coins");
	// 	
	// 	await coinsReference.SetValueAsync(_Coins);
	// }
	//
	// async Task SaveScoreAsync(string _LevelID, long _Score, int _Accuracy, ScoreRank _Rank)
	// {
	// 	DatabaseReference scoreReference = m_Scores.Child(_LevelID);
	// 	
	// 	await Task.WhenAll(
	// 		scoreReference.Child("score").SetValueAsync(_Score),
	// 		scoreReference.Child("accuracy").SetValueAsync(_Accuracy),
	// 		scoreReference.Child("rank").SetValueAsync(_Rank)
	// 	);
	// }
}