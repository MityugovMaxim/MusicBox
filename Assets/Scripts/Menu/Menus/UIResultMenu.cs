using System.Threading.Tasks;
using UnityEngine;

[Menu(MenuType.ResultMenu)]
public class UIResultMenu : UIMenu
{
	[SerializeField] UISongBackground   m_Background;
	[SerializeField] UIResultStatistics m_Statistics;
	[SerializeField] UIResultScores     m_Scores;
	[SerializeField] UIResultDiscs      m_Discs;
	[SerializeField] UIResultCoins      m_Coins;
	[SerializeField] UIResultPoints     m_Points;
	[SerializeField] UIResultChests     m_Chests;
	[SerializeField] UIResultControl    m_Control;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Scores.Setup(m_SongID);
		m_Discs.Setup(m_SongID);
		m_Chests.Setup(m_SongID);
		m_Coins.Setup(m_SongID);
		m_Points.Setup(m_SongID);
		m_Control.Setup(m_SongID);
		
		m_Background.SongID = m_SongID;
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_Statistics.Hide(true);
		m_Scores.Hide(true);
		m_Discs.Hide(true);
		m_Coins.Hide(true);
		m_Points.Hide(true);
		m_Chests.Hide(true);
		m_Control.Hide(true);
		
		m_Scores.OnAccuracyChange += OnAccuracyChange;
	}

	protected override async void OnShowFinished()
	{
		base.OnShowFinished();
		
		await m_Statistics.ShowAsync();
		
		await m_Statistics.PlayAsync();
		
		await m_Statistics.HideAsync();
		
		await m_Scores.ShowAsync();
		
		await Task.WhenAll(
			m_Coins.ShowAsync(),
			m_Points.ShowAsync()
		);
		
		await m_Discs.ShowAsync();
		
		await m_Scores.PlayAsync();
		
		await m_Scores.HideAsync();
		
		await m_Chests.ShowAsync();
		
		await m_Chests.PlayAsync();
		
		await m_Control.ShowAsync();
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		m_Scores.OnAccuracyChange -= OnAccuracyChange;
	}

	void OnAccuracyChange(int _Accuracy)
	{
		m_Coins.Accuracy  = _Accuracy;
		m_Discs.Accuracy  = _Accuracy;
		m_Points.Accuracy = _Accuracy;
	}
}
