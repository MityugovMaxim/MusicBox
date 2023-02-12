using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIResultControl : UIGroup
{
	[SerializeField] TMP_Text      m_CoinsLabel;
	[SerializeField] Button        m_DoubleButton;
	[SerializeField] Button        m_RestartButton;
	[SerializeField] Button        m_ContinueButton;
	[SerializeField] UIResultCoins m_Coins;
	[SerializeField] UIGroup       m_ControlGroup;
	[SerializeField] UIGroup       m_DoubleGroup;
	[SerializeField] UIGroup       m_RestartGroup;
	[SerializeField] UIGroup       m_ContinueGroup;
	[SerializeField] UIGroup       m_LoaderGroup;

	[Inject] SongsManager      m_SongsManager;
	[Inject] ScoresManager     m_ScoresManager;
	[Inject] ScoreController   m_ScoreController;
	[Inject] DifficultyManager m_DifficultyManager;
	[Inject] AdsProcessor      m_AdsProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;
	[Inject] SongController    m_SongController;

	string   m_SongID;
	RankType m_SongRank;
	RankType m_SourceScoreRank;
	RankType m_TargetScoreRank;
	long     m_Payout;
	bool     m_Double;

	protected override void Awake()
	{
		base.Awake();
		
		m_DoubleButton.Subscribe(Double);
		m_RestartButton.Subscribe(Restart);
		m_ContinueButton.Subscribe(Continue);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_DoubleButton.Unsubscribe(Double);
		m_RestartButton.Unsubscribe(Restart);
		m_ContinueButton.Unsubscribe(Continue);
	}

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		m_Double = false;
		
		m_SongRank        = m_SongsManager.GetRank(m_SongID);
		m_SourceScoreRank = m_ScoresManager.GetRank(m_SongID);
		m_TargetScoreRank = m_ScoreController.GetRank();
		m_Payout          = m_DifficultyManager.GetPayout(m_SongRank, m_SourceScoreRank, m_TargetScoreRank);
	}

	protected override void OnShowStarted()
	{
		m_ControlGroup.Show(true);
		m_LoaderGroup.Hide(true);
		
		if (m_Payout > 0)
		{
			m_DoubleGroup.Show(true);
			m_RestartGroup.Hide(true);
			m_ContinueGroup.Hide(true);
		}
		else
		{
			m_DoubleGroup.Hide(true);
			m_RestartGroup.Show(true);
			m_ContinueGroup.Show(true);
		}
		
		m_CoinsLabel.text = $"<sprite name=ads> +{m_Payout}<sprite name=coins>";
	}

	protected override async void OnShowFinished()
	{
		await Task.Delay(250);
		
		await Task.WhenAll(
			m_ContinueGroup.ShowAsync(),
			m_RestartGroup.ShowAsync()
		);
	}

	async void Double()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		m_Double = await m_AdsProcessor.Rewarded("result_double");
		
		if (m_Double)
		{
			m_Coins.Append(m_Payout);
			
			await Task.Delay(250);
		}
		
		Continue();
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}

	async void Restart()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		m_SongController.Restart();
		
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}

	async void Continue()
	{
		await m_ControlGroup.HideAsync();
		
		await m_LoaderGroup.ShowAsync();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		SongFinishRequest request = new SongFinishRequest(
			m_SongID,
			m_ScoreController.Score,
			m_ScoreController.Accuracy,
			m_Double
		);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			Close();
		}
		else
		{
			await m_MenuProcessor.RetryAsync(
				"song_finish",
				Continue,
				Close
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}

	async void Close()
	{
		await Task.WhenAll(
			m_ControlGroup.HideAsync(),
			m_LoaderGroup.HideAsync()
		);
		
		m_SongController.Complete();
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.ResultMenu);
	}
}
