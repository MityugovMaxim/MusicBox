using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIOffersPageItem : UIEntity
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIOffersPageItem> { }

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");

	[SerializeField] UILevelThumbnail    m_LevelThumbnail;
	[SerializeField] Image               m_CoinsLargeIcon;
	[SerializeField] Image               m_CoinsSmallIcon;
	[SerializeField] UILevelLabel m_LevelLabel;
	[SerializeField] UICoinsLabel        m_CoinsLabel;
	[SerializeField] TMP_Text            m_Label;

	OfferProcessor m_OfferProcessor;
	AdsProcessor   m_AdsProcessor;
	MenuProcessor  m_MenuProcessor;

	Animator m_Animator;
	bool     m_Collected;
	string   m_OfferID;
	string   m_LevelID;
	long     m_Coins;
	int      m_Progress;
	int      m_Target;

	[Inject]
	public void Construct(
		OfferProcessor _OfferProcessor,
		AdsProcessor   _AdsProcessor,
		MenuProcessor  _MenuProcessor
	)
	{
		m_OfferProcessor = _OfferProcessor;
		m_AdsProcessor   = _AdsProcessor;
		m_MenuProcessor  = _MenuProcessor;
	}

	public void Setup(string _OfferID)
	{
		Restore();
		
		m_Collected = false;
		m_OfferID   = _OfferID;
		m_LevelID   = m_OfferProcessor.GetLevelID(m_OfferID);
		m_Coins     = m_OfferProcessor.GetCoins(m_OfferID);
		m_Target    = m_OfferProcessor.GetRewardedCount(m_OfferID);
		m_Progress  = GetProgress(m_OfferID);
		
		if (string.IsNullOrEmpty(m_LevelID))
			ProcessCoins();
		else if (m_Coins <= 0)
			ProcessLevel();
		else
			ProcessLevelAndCoins();
		
		ProcessProgress();
	}

	public async void Progress()
	{
		if (m_Collected)
			return;
		
		if (m_Progress >= m_Target)
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			bool completeSuccess = await m_OfferProcessor.CompleteOffer(m_OfferID);
			
			if (completeSuccess)
			{
				m_Collected = true;
				
				await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
				
				Collect();
			}
			else
			{
				// TODO: Setup error menu
				
				await m_MenuProcessor.Show(MenuType.ErrorMenu);
				
				await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			}
		}
		else
		{
			await m_MenuProcessor.Show(MenuType.ProcessingMenu);
			
			bool progressSuccess = await m_AdsProcessor.ShowRewardedAsync(this);
			
			if (progressSuccess)
			{
				m_Progress++;
				
				SetProgress(m_OfferID, m_Progress);
				
				ProcessProgress();
				
				await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			}
			else
			{
				// TODO: Setup error menu
				
				await m_MenuProcessor.Show(MenuType.ErrorMenu);
				
				await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	void Collect()
	{
		m_Animator.SetTrigger(m_CollectParameterID);
	}

	void Restore()
	{
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	int GetProgress(string _OfferID)
	{
		return PlayerPrefs.GetInt("offer_progress_" + _OfferID, 0);
	}

	void SetProgress(string _OfferID, int _Progress)
	{
		PlayerPrefs.SetInt("offer_progress_" + _OfferID, _Progress);
	}

	void ProcessLevelAndCoins()
	{
		m_CoinsLargeIcon.gameObject.SetActive(false);
		m_CoinsSmallIcon.gameObject.SetActive(true);
		m_LevelThumbnail.gameObject.SetActive(true);
		m_LevelLabel.gameObject.SetActive(true);
		
		m_LevelThumbnail.Setup(m_LevelID);
		m_LevelLabel.Setup(m_LevelID);
		
		m_CoinsLabel.Coins = m_Coins;
	}

	void ProcessLevel()
	{
		m_CoinsLargeIcon.gameObject.SetActive(false);
		m_CoinsSmallIcon.gameObject.SetActive(false);
		m_LevelThumbnail.gameObject.SetActive(true);
		m_LevelLabel.gameObject.SetActive(true);
		
		m_LevelThumbnail.Setup(m_LevelID);
		m_LevelLabel.Setup(m_LevelID);
	}

	void ProcessCoins()
	{
		m_CoinsLargeIcon.gameObject.SetActive(true);
		m_CoinsSmallIcon.gameObject.SetActive(false);
		m_LevelThumbnail.gameObject.SetActive(false);
		m_LevelLabel.gameObject.SetActive(false);
		
		m_CoinsLabel.Coins = m_Coins;
	}

	void ProcessProgress()
	{
		m_Label.text = m_Progress < m_Target
			? $"WATCH {m_Progress}/{m_Target}"
			: "COLLECT";
	}
}
