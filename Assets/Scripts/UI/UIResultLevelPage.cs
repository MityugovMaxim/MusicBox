using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultLevelPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Level;

	public override bool Valid => m_Level < m_LevelLimit && m_SourceDiscs < m_TargetDiscs;

	[SerializeField] UILevelList m_LevelList;
	[SerializeField] UIGroup     m_ContinueGroup;

	[Inject] ScoreManager      m_ScoreManager;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	string m_SongID;

	int m_SourceDiscs;
	int m_TargetDiscs;
	int m_Level;
	int m_LevelLimit;

	public override void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_SourceDiscs = m_ScoreManager.GetSourceDiscs();
		m_TargetDiscs = m_ScoreManager.GetTargetDiscs();
		m_Level       = m_ProgressProcessor.GetLevel(m_SourceDiscs);
		m_LevelLimit  = m_ProgressProcessor.GetMaxLevel();
		
		m_LevelList.Setup(m_SongID);
		
		m_ContinueGroup.Hide(true);
	}

	public override async void Play()
	{
		await m_LevelList.PlayAsync();
		
		await Task.Delay(1500);
		
		await m_ContinueGroup.ShowAsync();
	}

	public void Continue()
	{
		m_ContinueGroup.Hide();
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		resultMenu.Next();
	}
}