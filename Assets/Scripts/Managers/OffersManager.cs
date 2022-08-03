using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class OffersManager
{
	[Inject] AdsProcessor     m_AdsProcessor;
	[Inject] OffersProcessor  m_OffersProcessor;
	[Inject] ProfileProcessor m_ProfileProcessor;

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
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetCollectedOfferIDs()
	{
		return m_OffersProcessor.GetOfferIDs()
			.Where(IsCollected)
			.ToList();
	}

	public async Task<bool> ProgressOffer(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID) || m_ProfileProcessor.HasOffer(_OfferID))
			return false;
		
		bool success = await m_AdsProcessor.Rewarded("offer");
		
		if (success)
		{
			if (m_Progress.ContainsKey(_OfferID))
				m_Progress[_OfferID]++;
			else
				m_Progress[_OfferID] = 1;
		}
		
		return success;
	}

	public async Task<bool> CollectOffer(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID) || m_ProfileProcessor.HasOffer(_OfferID))
			return false;
		
		OfferCollectRequest request = new OfferCollectRequest(_OfferID);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			await m_ProfileProcessor.Load();
		}
		
		return success;
	}
}