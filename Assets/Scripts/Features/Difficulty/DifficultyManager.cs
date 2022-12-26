using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class DifficultyManager
{
	public DifficultyCollection Collection => m_DifficultyCollection;

	[Inject] DifficultyCollection m_DifficultyCollection;

	public float GetSpeed(RankType _SongType)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongType);
		
		return snapshot?.Speed ?? 500;
	}

	public float GetInputExpand(RankType _SongType)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongType);
		
		return snapshot?.InputExpand ?? 0;
	}

	public float GetInputOffset(RankType _SongType)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongType);
		
		return snapshot?.InputOffset ?? 0;
	}

	public long GetCoins(RankType _SongType, RankType _ScoreRank)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongType);
		
		if (snapshot == null)
			return 0;
		
		switch (_ScoreRank)
		{
			case RankType.Bronze:   return snapshot.BronzeCoins;
			case RankType.Silver:   return snapshot.SilverCoins;
			case RankType.Gold:     return snapshot.GoldCoins;
			case RankType.Platinum: return snapshot.PlatinumCoins;
			default:                 return 0;
		}
	}

	public int GetThreshold(RankType _SongRank, RankType _ScoreRank)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongRank);
		
		if (snapshot == null)
			return 0;
		
		switch (_ScoreRank)
		{
			case RankType.Bronze:   return snapshot.BronzeThreshold;
			case RankType.Silver:   return snapshot.SilverThreshold;
			case RankType.Gold:     return snapshot.GoldThreshold;
			case RankType.Platinum: return snapshot.PlatinumThreshold;
			default:                 return 0;
		}
	}

	public RankType GetRank(RankType _SongRank, int _Accuracy)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongRank);
		
		if (snapshot == null)
			return RankType.None;
		
		if (_Accuracy >= snapshot.PlatinumThreshold)
			return RankType.Platinum;
		if (_Accuracy >= snapshot.GoldThreshold)
			return RankType.Gold;
		if (_Accuracy >= snapshot.SilverThreshold)
			return RankType.Silver;
		if (_Accuracy >= snapshot.BronzeThreshold)
			return RankType.Bronze;
		
		return RankType.None;
	}
}
