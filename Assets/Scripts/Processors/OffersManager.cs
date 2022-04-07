using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class OffersManager
{
	[Inject] AdsProcessor          m_AdsProcessor;
	[Inject] OffersProcessor       m_OffersProcessor;
	[Inject] ProfileProcessor      m_ProfileProcessor;
	[Inject] MenuProcessor         m_MenuProcessor;
	[Inject] LocalizationProcessor m_LocalizationProcessor;
	[Inject] HapticProcessor       m_HapticProcessor;

	readonly Dictionary<string, int> m_Progress = new Dictionary<string, int>();

	public int GetProgress(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID))
			return 0;
		
		return m_Progress.TryGetValue(_OfferID, out int progress) ? progress : 0;
	}

	public int GetTarget(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID))
			return 0;
		
		return m_OffersProcessor.GetAdsCount(_OfferID);
	}

	public bool IsAvailable(string _OfferID)
	{
		return !m_ProfileProcessor.HasOffer(_OfferID);
	}

	public bool IsCollected(string _OfferID)
	{
		return m_ProfileProcessor.HasOffer(_OfferID);
	}

	public List<string> GetAvailableOfferIDs()
	{
		return m_OffersProcessor.GetOfferIDs()
			.Where(_OfferID => !m_ProfileProcessor.HasOffer(_OfferID))
			.ToList();
	}

	public List<string> GetCollectedOfferIDs()
	{
		return m_OffersProcessor.GetOfferIDs()
			.Where(_OfferID => m_ProfileProcessor.HasOffer(_OfferID))
			.ToList();
	}

	public Task<bool> Process(string _OfferID)
	{
		int progress = GetProgress(_OfferID);
		int target   = GetTarget(_OfferID);
		
		return progress >= target
			? CollectOffer(_OfferID)
			: ProgressOffer(_OfferID);
	}

	async Task<bool> ProgressOffer(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID) || m_ProfileProcessor.HasOffer(_OfferID))
			return false;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		bool success = await m_AdsProcessor.Rewarded();
		
		if (success)
		{
			if (m_Progress.ContainsKey(_OfferID))
				m_Progress[_OfferID]++;
			else
				m_Progress[_OfferID] = 1;
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		else
		{
			UIErrorMenu errorMenu = m_MenuProcessor.GetMenu<UIErrorMenu>();
			if (errorMenu != null)
			{
				errorMenu.Setup(
					"offer_progress_error",
					m_LocalizationProcessor.Get("OFFER_PROGRESS_ERROR_TITLE"),
					m_LocalizationProcessor.Get("OFFER_PROGRESS_ERROR_MESSAGE")
				);
			}
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		return success;
	}

	async Task<bool> CollectOffer(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID) || m_ProfileProcessor.HasOffer(_OfferID))
			return false;
		
		HttpsCallableReference function = FirebaseFunctions.DefaultInstance.GetHttpsCallable("CollectOffer");
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		data["offer_id"] = _OfferID;
		
		bool success = false;
		
		try
		{
			HttpsCallableResult result = await function.CallAsync(data);
			
			success = (bool)result.Data;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		if (success)
		{
			m_HapticProcessor.Process(Haptic.Type.Success);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		else
		{
			m_HapticProcessor.Process(Haptic.Type.Failure);
			
			UIErrorMenu errorMenu = m_MenuProcessor.GetMenu<UIErrorMenu>();
			if (errorMenu != null)
			{
				errorMenu.Setup(
					"offer_collect_error",
					m_LocalizationProcessor.Get("OFFER_COLLECT_ERROR_TITLE"),
					m_LocalizationProcessor.Get("OFFER_COLLECT_ERROR_MESSAGE")
				);
			}
			
			await m_MenuProcessor.Show(MenuType.ErrorMenu);
			
			await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
		}
		
		return success;
	}
}