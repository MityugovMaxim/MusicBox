using System.Collections.Generic;
using System.Linq;

public class ScoreProcessor
{
	readonly List<float> m_Success = new List<float>();
	readonly List<float> m_Fail    = new List<float>();

	public void Restore()
	{
		m_Success.Clear();
		m_Fail.Clear();
	}

	public float GetScore()
	{
		return m_Success.Sum() + m_Fail.Sum();
	}

	public float GetMaxScore()
	{
		return m_Success.Count + m_Fail.Count;
	}

	public void RegisterSuccess(float _Progress)
	{
		m_Success.Add(_Progress);
	}

	public void RegisterFail(float _Progress)
	{
		m_Fail.Add(_Progress);
	}
}
