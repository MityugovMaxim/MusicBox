using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class DifficultyManager
{
	public DifficultyCollection Collection => m_DifficultyCollection;

	[Inject] DifficultyCollection m_DifficultyCollection;

	public float GetSpeed(DifficultyType _DifficultyType)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_DifficultyType);
		
		return snapshot?.Speed ?? 500;
	}

	public float GetInputExpand(DifficultyType _DifficultyType)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_DifficultyType);
		
		return snapshot?.InputExpand ?? 0;
	}

	public float GetInputOffset(DifficultyType _DifficultyType)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_DifficultyType);
		
		return snapshot?.InputOffset ?? 0;
	}

	public long GetCoins(DifficultyType _DifficultyType, ScoreRank _ScoreRank)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_DifficultyType);
		
		if (snapshot == null)
			return 0;
		
		switch (_ScoreRank)
		{
			case ScoreRank.Bronze:   return snapshot.BronzeCoins;
			case ScoreRank.Silver:   return snapshot.SilverCoins;
			case ScoreRank.Gold:     return snapshot.GoldCoins;
			case ScoreRank.Platinum: return snapshot.PlatinumCoins;
			default:                 return 0;
		}
	}

	public int GetThreshold(DifficultyType _DifficultyType, ScoreRank _ScoreRank)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_DifficultyType);
		
		if (snapshot == null)
			return 0;
		
		switch (_ScoreRank)
		{
			case ScoreRank.Bronze:   return snapshot.BronzeThreshold;
			case ScoreRank.Silver:   return snapshot.SilverThreshold;
			case ScoreRank.Gold:     return snapshot.GoldThreshold;
			case ScoreRank.Platinum: return snapshot.PlatinumThreshold;
			default:                 return 0;
		}
	}

	public ScoreRank GetRank(DifficultyType _DifficultyType, int _Accuracy)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_DifficultyType);
		
		if (snapshot == null)
			return ScoreRank.None;
		
		if (_Accuracy >= snapshot.PlatinumThreshold)
			return ScoreRank.Platinum;
		if (_Accuracy >= snapshot.GoldThreshold)
			return ScoreRank.Gold;
		if (_Accuracy >= snapshot.SilverThreshold)
			return ScoreRank.Silver;
		if (_Accuracy >= snapshot.BronzeThreshold)
			return ScoreRank.Bronze;
		
		return ScoreRank.None;
	}
}
