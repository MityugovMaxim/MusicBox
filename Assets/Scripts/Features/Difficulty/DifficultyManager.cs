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

	public long GetPoints(RankType _SongRank, RankType _SourceScoreRank, RankType _TargetScoreRank)
	{
		long points = 0;
		for (RankType scoreRank = _SourceScoreRank + 1; scoreRank <= _TargetScoreRank; scoreRank++)
			points += GetPoints(_SongRank, scoreRank);
		return points;
	}

	public long GetPoints(RankType _SongRank, RankType _ScoreRank)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongRank);
		
		if (snapshot == null)
			return 0;
		
		switch (_ScoreRank)
		{
			case RankType.Bronze:   return snapshot.BronzePoints;
			case RankType.Silver:   return snapshot.SilverPoints;
			case RankType.Gold:     return snapshot.GoldPoints;
			case RankType.Platinum: return snapshot.PlatinumPoints;
			default:                return 0;
		}
	}

	public float GetInputExpand(RankType _SongType)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongType);
		
		return snapshot?.InputExpand ?? 0;
	}

	public float GetInputOffset(RankType _SongRank)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongRank);
		
		return snapshot?.InputOffset ?? 0;
	}

	public long GetCoins(RankType _SongRank, RankType _ScoreRank)
	{
		DifficultySnapshot snapshot = Collection.GetSnapshot(_SongRank);
		
		if (snapshot == null)
			return 0;
		
		switch (_ScoreRank)
		{
			case RankType.Bronze:   return snapshot.BronzeCoins;
			case RankType.Silver:   return snapshot.SilverCoins;
			case RankType.Gold:     return snapshot.GoldCoins;
			case RankType.Platinum: return snapshot.PlatinumCoins;
			default:                return 0;
		}
	}

	public long GetPayout(RankType _SongRank, RankType _SourceRank, RankType _TargetRank)
	{
		long payout = 0;
		for (RankType scoreRank = _SourceRank + 1; scoreRank <= _TargetRank; scoreRank++)
			payout += GetCoins(_SongRank, scoreRank);
		return payout;
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
