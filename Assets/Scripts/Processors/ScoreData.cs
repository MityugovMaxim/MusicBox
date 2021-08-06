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
	const float TAP_PERFECT_MULTIPLIER    = 10;
	const float TAP_GOOD_MULTIPLIER       = 6;
	const float TAP_BAD_MULTIPLIER        = 4;
	const float DOUBLE_PERFECT_MULTIPLIER = 20;
	const float DOUBLE_GOOD_MULTIPLIER    = 12;
	const float DOUBLE_BAD_MULTIPLIER     = 8;
	const float TAP_PERFECT_THRESHOLD     = 0.65f;
	const float TAP_GOOD_THRESHOLD        = 0.35f;
	const float DOUBLE_PERFECT_THRESHOLD  = 0.65f;
	const float DOUBLE_GOOD_THRESHOLD     = 0.35f;
	const int   S_RANK                    = 98;
	const int   A_RANK                    = 85;
	const int   B_RANK                    = 50;
	const int   C_RANK                    = 5;
	const int   X4_COMBO                  = 25;
	const int   X3_COMBO                  = 15;
	const int   X2_COMBO                  = 10;

	public long Score => m_Score;

	public int Accuracy
	{
		get
		{
			float score = (float)(SourceAccuracyScore / TargetAccuracyScore);
			
			return Mathf.RoundToInt(Mathf.Clamp01(score) * 100);
		}
	}

	public ScoreRank Rank
	{
		get
		{
			int accuracy = Accuracy;
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

	double SourceAccuracyScore
	{
		get
		{
			double score = 0;
			
			double holdScore = 0;
			holdScore += m_HoldSuccess * HOLD_SUCCESS_MULTIPLIER;
			holdScore += m_HoldFail * HOLD_FAIL_MULTIPLIER;
			holdScore += m_HoldHit * HOLD_HIT_MULTIPLIER;
			holdScore += m_HoldMiss * HOLD_MISS_MULTIPLIER;
			holdScore *= m_HoldSuccessScore + m_HoldFailScore;
			score     += (long)holdScore;
			
			double tapScore = 0;
			tapScore += m_TapPerfect * TAP_PERFECT_MULTIPLIER;
			tapScore += m_TapGood * TAP_GOOD_MULTIPLIER;
			tapScore += m_TapBad * TAP_BAD_MULTIPLIER;
			score    += (long)tapScore;
			
			double doubleScore = 0;
			doubleScore += m_DoublePerfect * DOUBLE_PERFECT_MULTIPLIER;
			doubleScore += m_DoubleGood * DOUBLE_GOOD_MULTIPLIER;
			doubleScore += m_DoubleBad * DOUBLE_BAD_MULTIPLIER;
			score       += (long)doubleScore;
			
			return (long)score;
		}
	}

	double TargetAccuracyScore
	{
		get
		{
			double score = 0;
			
			double holdCount   = m_HoldSuccess + m_HoldFail;
			double tapCount    = m_TapPerfect + m_TapGood + m_TapBad + m_TapFail;
			double doubleCount = m_DoublePerfect + m_DoubleGood + m_DoubleBad + m_DoubleFail;
			
			score += holdCount * HOLD_SUCCESS_MULTIPLIER;
			score += holdCount * HOLD_HIT_MULTIPLIER;
			score *= holdCount;
			
			score += tapCount * TAP_PERFECT_MULTIPLIER;
			
			score += doubleCount * DOUBLE_PERFECT_MULTIPLIER;
			
			return score;
		}
	}

	int Multiplier
	{
		get
		{
			if (m_Combo >= X4_COMBO)
				return 4;
			else if (m_Combo >= X3_COMBO)
				return 3;
			else if (m_Combo >= X2_COMBO)
				return 2;
			else
				return 1;
		}
	}

	[SerializeField] int m_HoldSuccess;
	[SerializeField] int m_HoldFail;
	[SerializeField] int m_HoldHit;
	[SerializeField] int m_HoldMiss;
	[SerializeField] int m_TapPerfect;
	[SerializeField] int m_TapGood;
	[SerializeField] int m_TapBad;
	[SerializeField] int m_TapFail;
	[SerializeField] int m_DoublePerfect;
	[SerializeField] int m_DoubleGood;
	[SerializeField] int m_DoubleBad;
	[SerializeField] int m_DoubleFail;

	[SerializeField] float m_HoldSuccessScore;
	[SerializeField] float m_HoldFailScore;
	[SerializeField] long  m_Score;

	int m_Combo;

	public void RegisterHoldSuccess(float _Progress)
	{
		m_Combo++;
		
		m_Score            += (long)(_Progress * HOLD_SUCCESS_MULTIPLIER * Multiplier);
		m_HoldSuccessScore += _Progress;
		m_HoldSuccess++;
	}

	public void RegisterHoldFail(float _Progress)
	{
		m_Combo = 0;
		
		m_Score         += (long)(_Progress * HOLD_SUCCESS_MULTIPLIER * Multiplier);
		m_HoldFailScore += _Progress;
		m_HoldFail++;
	}

	public void RegisterHoldHit()
	{
		m_HoldHit++;
	}

	public void RegisterHoldMiss()
	{
		m_Combo = 0;
		
		m_HoldMiss++;
	}

	public void RegisterTapSuccess(float _Progress)
	{
		m_Combo++;
		
		m_Score += (long)(_Progress * HOLD_SUCCESS_MULTIPLIER * Multiplier);
		
		if (_Progress >= TAP_PERFECT_THRESHOLD)
			m_TapPerfect++;
		else if (_Progress >= TAP_GOOD_THRESHOLD)
			m_TapGood++;
		else
			m_TapBad++;
	}

	public void RegisterTapFail()
	{
		m_Combo = 0;
		
		m_TapFail++;
	}

	public void RegisterDoubleSuccess(float _Progress)
	{
		m_Combo++;
		
		m_Score += (long)(_Progress * HOLD_SUCCESS_MULTIPLIER * Multiplier);
		
		if (_Progress >= DOUBLE_PERFECT_THRESHOLD)
			m_DoublePerfect++;
		else if (_Progress >= DOUBLE_GOOD_THRESHOLD)
			m_DoubleGood++;
		else
			m_DoubleBad++;
	}

	public void RegisterDoubleFail()
	{
		m_Combo = 0;
		
		m_DoubleFail++;
	}
}