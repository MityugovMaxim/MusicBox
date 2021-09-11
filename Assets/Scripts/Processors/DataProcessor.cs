using System.Collections.Generic;

public class LevelSnapshot
{
	public string    Title  { get; }
	public string    Artist { get; }
	public LevelMode Mode   { get; }
	public bool      Locked { get; }
	public long      Payout { get; }
	public long      Price  { get; }

	public LevelSnapshot(
		string    _Title,
		string    _Artist,
		LevelMode _LevelMode,
		bool      _Locked,
		long      _Payout,
		long      _Price
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

public class PurchaseSnapshot
{
	public string ID        { get; }
	public string ProductID { get; }
	public string Receipt   { get; }

	public PurchaseSnapshot(
		string _ID,
		string _ProductID,
		string _Receipt
	)
	{
		ID        = _ID;
		ProductID = _ProductID;
		Receipt   = _Receipt;
	}
}

public class ProductSnapshot
{
	public IReadOnlyList<string> LevelIDs => m_LevelIDs;

	readonly List<string> m_LevelIDs;

	public ProductSnapshot(List<string> _LevelIDs)
	{
		m_LevelIDs = _LevelIDs;
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