using TMPro;
using UnityEngine;
using Zenject;

public class UIProfile : UIEntity
{
	public string Username
	{
		get => m_Username;
		set
		{
			if (m_Username == value)
				return;
			
			m_Username = value;
			
			ProcessUsername();
		}
	}

	public long Coins
	{
		get => m_Coins;
		set
		{
			if (m_Coins == value)
				return;
			
			m_Coins = value;
			
			ProcessCoins();
		}
	}

	public int Discs
	{
		get => m_Discs;
		set
		{
			if (m_Discs == value)
				return;
			
			m_Discs = value;
			
			ProcessDiscs();
		}
	}

	public int Level
	{
		get => m_Level.Level;
		set => m_Level.Level = value;
	}

	[SerializeField] UIProfileImage m_Image;
	[SerializeField] TMP_Text       m_UsernameLabel;
	[SerializeField] UILevel        m_Level;
	[SerializeField] TMP_Text       m_CoinsLabel;
	[SerializeField] TMP_Text       m_DiscsLabel;

	[SerializeField] string m_Username;
	[SerializeField] int    m_Discs;
	[SerializeField] long   m_Coins;

	[Inject] ProfileProcessor   m_ProfileProcessor;
	[Inject] ProgressProcessor  m_ProgressProcessor;
	[Inject] SocialProcessor    m_SocialProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessUsername();
		
		ProcessDiscs();
		
		ProcessCoins();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessUsername();
		
		ProcessDiscs();
		
		ProcessCoins();
	}
	#endif

	public void Setup()
	{
		m_Image.Setup(m_SocialProcessor.Photo);
		
		Username = m_SocialProcessor.GetUsername();
		
		Discs = m_ProfileProcessor.Discs;
		
		Level = m_ProfileProcessor.Level;
		
		Coins = m_ProfileProcessor.Coins;
	}

	public void Open()
	{
		m_StatisticProcessor.LogMainMenuProfileClick();
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null && mainMenu.Shown)
			mainMenu.Select(MainMenuPageType.Profile);
	}

	void ProcessDiscs()
	{
		m_DiscsLabel.text = m_ProgressProcessor != null && Level < m_ProgressProcessor.GetMaxLevel()
			? $"{Discs}/{m_ProgressProcessor.GetMaxLimit(Level)}"
			: Discs.ToString();
	}

	void ProcessUsername()
	{
		m_UsernameLabel.text = Username;
	}

	void ProcessCoins()
	{
		m_CoinsLabel.text = $"{Coins}<sprite tint=0 name=coins_icon>";
	}
}