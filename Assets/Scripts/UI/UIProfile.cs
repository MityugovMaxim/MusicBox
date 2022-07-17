using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIProfile : UIEntity
{
	long Coins
	{
		[Preserve] get => (long)m_Coins.Value;
		set => m_Coins.Value = value;
	}

	int Discs
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

	int Level
	{
		get => m_Level.Level;
		set => m_Level.Level = value;
	}

	[SerializeField] UIProfileImage m_Image;
	[SerializeField] UILevel        m_Level;
	[SerializeField] UIUnitLabel    m_Coins;
	[SerializeField] TMP_Text       m_DiscsLabel;
	[SerializeField] int            m_Discs;

	[Inject] ProfileProcessor  m_ProfileProcessor;
	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] SocialProcessor   m_SocialProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessDiscs();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessDiscs();
	}
	#endif

	public void Setup()
	{
		m_Image.Setup(m_SocialProcessor.Photo);
		
		Coins = m_ProfileProcessor.Coins;
		
		Level = m_ProfileProcessor.Level;
		
		Discs = m_ProfileProcessor.Discs;
	}

	public void Open()
	{
		GUIUtility.systemCopyBuffer = m_SocialProcessor.UserID;
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null && mainMenu.Shown)
			mainMenu.Select(MainMenuPageType.Profile);
	}

	void ProcessDiscs()
	{
		m_DiscsLabel.text = m_ProgressProcessor != null && Level < m_ProgressProcessor.GetMaxLevel()
			? $"{Discs}/{m_ProgressProcessor.GetDiscs(Level + 1)}"
			: Discs.ToString();
	}
}