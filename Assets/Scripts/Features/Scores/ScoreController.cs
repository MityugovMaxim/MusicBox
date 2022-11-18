public class ScoreController
{
	public DynamicDelegate<int>                   OnMultiplierChange;
	public DynamicDelegate<float>                 OnProgressChange;
	public DynamicDelegate<long>                  OnScoreChange;
	public DynamicDelegate<ScoreType, ScoreGrade> OnHit;
	public DynamicDelegate<int, ScoreGrade>       OnComboChange;

	public int   Multiplier { get; private set; }
	public int   Combo      { get; private set; }
	public float Progress   { get; private set; }
	public long  Score      { get; private set; }

	public ScoreGrade GetGrade(ScoreType _ScoreType, float _Progress)
	{
		return ScoreGrade.None;
	}

	public void Setup(string _SongID)
	{
		
	}

	public void Restore()
	{
		
	}

	public void TapHit(float _Progress)
	{
		
	}

	public void DoubleHit(float _Progress)
	{
		
	}

	public void HoldHit(float _Progress)
	{
		
	}

	public void HoldRelease(float _Progress, float _Length)
	{
		
	}

	public void Miss()
	{
		
	}

	public void TapFail()
	{
		
	}

	public void DoubleFail()
	{
		
	}

	public void HoldFail()
	{
		
	}

	public ScoreRank GetRank()
	{
		return ScoreRank.None;
	}

	public int GetAccuracy()
	{
		return 0;
	}

	public long GetScore()
	{
		return 0;
	}

	public long GetScore(int _Accuracy)
	{
		return 0;
	}

	public int GetStatistics(ScoreGrade _ScoreGrade)
	{
		return 0;
	}
}
