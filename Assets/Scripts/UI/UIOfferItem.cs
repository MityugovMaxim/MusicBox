using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIOfferItem : UIGroupLayout
{
	[Preserve]
	public class Pool : MonoMemoryPool<UIOfferItem> { }

	[SerializeField] UIRemoteImage    m_Icon;
	[SerializeField] TMP_Text         m_Title;
	[SerializeField] TMP_Text         m_Label;

	OffersProcessor   m_OffersProcessor;
	StorageProcessor  m_StorageProcessor;
	LanguageProcessor m_LanguageProcessor;
	AdsProcessor      m_AdsProcessor;
	MenuProcessor     m_MenuProcessor;

	string m_OfferID;
	int    m_Progress;
	int    m_Target;

	[Inject]
	public void Construct(
		OffersProcessor   _OffersProcessor,
		StorageProcessor  _StorageProcessor,
		LanguageProcessor _LanguageProcessor,
		AdsProcessor      _AdsProcessor,
		MenuProcessor     _MenuProcessor
	)
	{
		m_OffersProcessor   = _OffersProcessor;
		m_StorageProcessor  = _StorageProcessor;
		m_LanguageProcessor = _LanguageProcessor;
		m_AdsProcessor      = _AdsProcessor;
		m_MenuProcessor     = _MenuProcessor;
	}

	public void Setup(string _OfferID)
	{
		m_OfferID  = _OfferID;
		m_Target   = m_OffersProcessor.GetAdsCount(m_OfferID);
		m_Progress = GetProgress(m_OfferID);
		
		m_Title.text = m_OffersProcessor.GetTitle(m_OfferID);
		
		m_Icon.Load(m_StorageProcessor.LoadOfferThumbnail(m_OfferID));
		
		ProcessProgress();
	}

	public async void Progress()
	{
		if (m_Progress >= m_Target)
			await CollectOffer();
		else
			await ProgressOffer();
	}

	async Task ProgressOffer()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded();
		
		if (success)
		{
			m_Progress++;
			
			SetProgress(m_OfferID, m_Progress);
			
			ProcessProgress();
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		else
		{
			UIErrorMenu errorMenu = m_MenuProcessor.GetMenu<UIErrorMenu>();
			if (errorMenu != null)
			{
				errorMenu.Setup(
					m_LanguageProcessor.Get("OFFER_PROGRESS_ERROR_TITLE"),
					m_LanguageProcessor.Get("OFFER_PROGRESS_ERROR_MESSAGE")
				);
			}
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
	}

	async Task CollectOffer()
	{
		await HideAsync();
		
		await m_OffersProcessor.CollectOffer(m_OfferID);
	}

	static int GetProgress(string _OfferID)
	{
		return PlayerPrefs.GetInt("offer_progress_" + _OfferID, 0);
	}

	static void SetProgress(string _OfferID, int _Progress)
	{
		PlayerPrefs.SetInt("offer_progress_" + _OfferID, _Progress);
	}

	void ProcessProgress()
	{
		m_Label.text = m_Progress < m_Target
			? m_LanguageProcessor.Format("OFFER_PROGRESS", m_Progress, m_Target)
			: m_LanguageProcessor.Get("OFFER_COLLECT");
	}
}
