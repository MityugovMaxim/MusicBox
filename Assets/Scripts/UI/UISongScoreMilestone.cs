using System;
using System.Threading;
using TMPro;
using UnityEngine;
using Zenject;

public class UISongScoreMilestone : UIGroup
{
	public int Accuracy
	{
		get => m_Accuracy;
		set
		{
			if (m_Accuracy == value)
				return;
			
			m_Accuracy = value;
			
			ProcessAccuracy();
		}
	}

	[SerializeField] TMP_Text    m_Position;
	[SerializeField] UIUnitLabel m_Score;
	[SerializeField] UIDisc      m_Disc;

	[Inject] ScoreManager    m_ScoreManager;
	[Inject] SongsProcessor m_SongsProcessor;

	string m_SongID;
	int    m_Accuracy;

	CancellationTokenSource m_TokenSource;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Accuracy = m_SongsProcessor.GetThreshold(m_SongID, ScoreRank.Bronze);
		
		ProcessValues();
		
		Show(true);
	}

	async void ProcessAccuracy()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			token.ThrowIfCancellationRequested();
			
			await HideAsync();
			
			ProcessValues();
			
			token.ThrowIfCancellationRequested();
			
			await ShowAsync();
		}
		catch (OperationCanceledException)
		{
			return;
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void ProcessValues()
	{
		int       position = 100 - Accuracy;
		long      score    = m_ScoreManager.GetScore(Accuracy);
		ScoreRank rank     = m_SongsProcessor.GetRank(m_SongID, Accuracy);
		
		m_Position.text = position.ToString();
		m_Score.Value   = score;
		m_Disc.Rank     = rank;
	}
}