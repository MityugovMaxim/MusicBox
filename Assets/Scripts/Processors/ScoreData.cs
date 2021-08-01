using System;
using UnityEngine;

public enum ScoreRank
{
	None = 0,
	C    = 1,
	B    = 2,
	A    = 3,
	S    = 4,
}

[Serializable]
public class ScoreData
{
	const float HOLD_SUCCESS_MULTIPLIER   = 50;
	const float HOLD_FAIL_MULTIPLIER      = 30;
	const float HOLD_HIT_MULTIPLIER       = 5;
	const float HOLD_MISS_MULTIPLIER      = -10;
	const float TAP_SUCCESS_MULTIPLIER    = 10;
	const float TAP_FAIL_MULTIPLIER       = -5;
	const float DOUBLE_SUCCESS_MULTIPLIER = 20;
	const float DOUBLE_FAIL_MULTIPLIER    = -10;
	const float S_RANK                    = 0.95f;
	const float A_RANK                    = 0.8f;
	const float B_RANK                    = 0.5f;
	const float C_RANK                    = 0.05f;

	public int   HoldSuccess      => m_HoldSuccess;
	public int   HoldFail         => m_HoldFail;
	public int   HoldHit          => m_HoldHit;
	public int   HoldMiss         => m_HoldMiss;
	public int   TapSuccess       => m_TapSuccess;
	public int   TapFail          => m_TapFail;
	public int   DoubleSuccess    => m_DoubleSuccess;
	public int   DoubleFail       => m_DoubleFail;
	public float HoldSuccessScore => m_HoldSuccessScore;
	public float HoldFailScore    => m_HoldFailScore;
	public float TapScore         => m_TapScore;
	public float DoubleScore      => m_DoubleScore;

	public double Score
	{
		get
		{
			double score = 0;
			
			double holdScore = 0;
			holdScore += HoldSuccess * HOLD_SUCCESS_MULTIPLIER;
			holdScore += HoldFail * HOLD_FAIL_MULTIPLIER;
			holdScore += HoldHit * HOLD_HIT_MULTIPLIER;
			holdScore += HoldMiss * HOLD_MISS_MULTIPLIER;
			holdScore *= HoldSuccessScore + HoldFailScore;
			score     += (long)holdScore;
			
			double tapScore = 0;
			tapScore += TapSuccess * TAP_SUCCESS_MULTIPLIER;
			tapScore += TapFail * TAP_FAIL_MULTIPLIER;
			tapScore *= TapScore;
			score    += (long)tapScore;
			
			double doubleScore = 0;
			doubleScore += DoubleSuccess * DOUBLE_SUCCESS_MULTIPLIER;
			doubleScore += DoubleFail * DOUBLE_FAIL_MULTIPLIER;
			doubleScore *= DoubleScore;
			score       += (long)doubleScore;
			
			return score;
		}
	}

	public double Accuracy
	{
		get
		{
			const float coefficient = 0.95f;
			
			double holdCount   = HoldSuccess + HoldFail;
			double tapCount    = TapSuccess + TapFail;
			double doubleCount = DoubleSuccess + DoubleFail;
			
			double source = Score;
			double target = 0;
			
			double holdScore = 0;
			holdScore += holdCount * HOLD_SUCCESS_MULTIPLIER;
			holdScore += holdCount * HOLD_HIT_MULTIPLIER;
			holdScore *= holdCount;
			target    += holdScore;
			
			double tapScore = 0;
			tapScore += tapCount * TAP_SUCCESS_MULTIPLIER;
			tapScore *= tapCount;
			target   += tapScore;
			
			double doubleScore = 0;
			doubleScore += doubleCount * DOUBLE_SUCCESS_MULTIPLIER;
			doubleScore *= doubleCount;
			target      += doubleScore;
			
			target *= coefficient;
			
			return Math.Min(1, source / target);
		}
	}

	public ScoreRank Rank
	{
		get
		{
			double accuracy = Accuracy;
			if (accuracy >= S_RANK)
				return ScoreRank.S;
			else if (accuracy >= A_RANK)
				return ScoreRank.A;
			else if (accuracy >= B_RANK)
				return ScoreRank.B;
			else if (accuracy >= C_RANK)
				return ScoreRank.C;
			else
				return ScoreRank.None;
		}
	}

	[SerializeField] int m_HoldSuccess;
	[SerializeField] int m_HoldFail;
	[SerializeField] int m_HoldHit;
	[SerializeField] int m_HoldMiss;
	[SerializeField] int m_TapSuccess;
	[SerializeField] int m_TapFail;
	[SerializeField] int m_DoubleSuccess;
	[SerializeField] int m_DoubleFail;

	[SerializeField] float m_HoldSuccessScore;
	[SerializeField] float m_HoldFailScore;
	[SerializeField] float m_TapScore;
	[SerializeField] float m_DoubleScore;

	public void RegisterHoldSuccess(float _Progress)
	{
		m_HoldSuccessScore += _Progress;
		m_HoldSuccess++;
	}

	public void RegisterHoldFail(float _Progress)
	{
		m_HoldFailScore += _Progress;
		m_HoldFail++;
	}

	public void RegisterHoldHit()
	{
		m_HoldHit++;
	}

	public void RegisterHoldMiss()
	{
		m_HoldMiss++;
	}

	public void RegisterTapSuccess(float _Progress)
	{
		m_TapScore += _Progress;
		m_TapSuccess++;
	}

	public void RegisterTapFail()
	{
		m_TapFail++;
	}

	public void RegisterDoubleSuccess(float _Progress)
	{
		m_DoubleScore += _Progress;
		m_DoubleSuccess++;
	}

	public void RegisterDoubleFail()
	{
		m_DoubleFail++;
	}
}