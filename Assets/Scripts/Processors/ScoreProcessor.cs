using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreProcessor : MonoBehaviour
{
	readonly List<float> m_Success = new List<float>();
	readonly List<float> m_Fail    = new List<float>();

	public void Restore()
	{
		m_Success.Clear();
		m_Fail.Clear();
	}

	[ContextMenu("Display score")]
	public void DisplayScore()
	{
		Debug.ClearDeveloperConsole();
		Debug.LogError("---> SCORE: " + GetScore());
		Debug.LogError("---> MAX POSSIBLE SCORE: " + GetMaxScore());
		Debug.LogError("---> ACCURACY: " + (GetScore() / GetMaxScore() * 100));
		Debug.LogError("---> HIT COUNT: " + m_Success.Count);
		Debug.LogError("---> MISS COUNT: " + m_Fail.Count);
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
