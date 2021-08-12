public class LevelComboSignal
{
	public int Multiplier { get; }

	public LevelComboSignal(int _Multiplier)
	{
		Multiplier = _Multiplier;
	}
}

public class LevelScoreSignal
{
	public long Score { get; }

	public LevelScoreSignal(long _Score)
	{
		Score = _Score;
	}
}