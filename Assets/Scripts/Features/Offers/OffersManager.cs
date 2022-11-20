using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class OffersManager : ProfileCollection<DataSnapshot<long>>
{
	public OffersCollection Collection => m_OffersCollection;
	public OffersDescriptor Descriptor => m_OffersDescriptor;

	protected override string Name => "offers";

	readonly DataEventHandler m_CollectHandler = new DataEventHandler();

	[Inject] OffersCollection m_OffersCollection;
	[Inject] OffersDescriptor m_OffersDescriptor;
	[Inject] AdsProcessor     m_AdsProcessor;

	readonly Dictionary<string, int> m_Progress = new Dictionary<string, int>();

	public Task Preload()
	{
		return Task.WhenAll(
			m_OffersCollection.Load(),
			m_OffersDescriptor.Load()
		);
	}

	public void SubscribeCollect(string _OfferID, Action _Action)
	{
		m_CollectHandler.AddListener(_OfferID, _Action);
	}

	public void UnsubscribeCollect(string _OfferID, Action _Action)
	{
		m_CollectHandler.RemoveListener(_OfferID, _Action);
	}

	public string GetImage(string _OfferID)
	{
		OfferSnapshot snapshot = m_OffersCollection.GetSnapshot(_OfferID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _OfferID) => m_OffersDescriptor.GetTitle(_OfferID);

	public string GetDescription(string _OfferID)
	{
		StringBuilder builder = new StringBuilder();
		
		string description = m_OffersDescriptor.GetDescription(_OfferID);
		if (!string.IsNullOrEmpty(description))
			builder.AppendLine(description);
		
		long coins = GetCoins(_OfferID);
		if (coins > 0)
			builder.AppendLine($"<b>+{coins}</b><sprite name=coins_icon>");
		
		return builder.ToString().TrimEnd();
	}

	public string GetSongID(string _OfferID)
	{
		OfferSnapshot snapshot = m_OffersCollection.GetSnapshot(_OfferID);
		
		return snapshot?.SongID ?? string.Empty;
	}

	public long GetCoins(string _OfferID)
	{
		OfferSnapshot snapshot = m_OffersCollection.GetSnapshot(_OfferID);
		
		return snapshot?.Coins ?? 0;
	}

	public int GetSource(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID))
			return 0;
		
		if (m_Progress.TryGetValue(_OfferID, out int progress))
			return progress;
		
		progress = LoadProgress(_OfferID);
		
		m_Progress[_OfferID] = progress;
		
		return progress;
	}

	public int GetTarget(string _OfferID)
	{
		OfferSnapshot snapshot = m_OffersCollection.GetSnapshot(_OfferID);
		
		return snapshot?.AdsCount ?? 0;
	}

	public List<string> GetAvailableOfferIDs()
	{
		return m_OffersCollection.GetIDs()
			.Where(IsAvailable)
			.ToList();
	}

	public List<string> GetCollectedOfferIDs()
	{
		return m_OffersCollection.GetIDs()
			.Where(Contains)
			.ToList();
	}

	public async Task Collect(string _OfferID)
	{
		if (string.IsNullOrEmpty(_OfferID))
			throw new UnityException();
		
		if (Contains(_OfferID))
			return;
		
		bool progress = await m_AdsProcessor.Rewarded("offer");
		
		int source = GetSource(_OfferID);
		int target = GetTarget(_OfferID);
		
		if (progress)
		{
			source += 1;
			
			SaveProgress(_OfferID, source);
			
			m_Progress[_OfferID] = source;
			
			m_CollectHandler.Invoke(_OfferID);
		}
		else
		{
			Log.Error(this, "Collect offer failed. Rewarded video error.");
			
			throw new UnityException();
		}
		
		if (source < target)
			return;
		
		OfferCollectRequest request = new OfferCollectRequest(_OfferID);
		
		bool collect = await request.SendAsync();
		
		if (collect)
		{
			DeleteProgress(_OfferID);
			
			m_CollectHandler.Invoke(_OfferID);
		}
		else
		{
			Log.Error(this, "Collect offer failed. Collect request error.");
			
			throw new UnityException();
		}
	}

	bool IsAvailable(string _OfferID)
	{
		OfferSnapshot snapshot = m_OffersCollection.GetSnapshot(_OfferID);
		
		return snapshot != null && snapshot.Active && !Contains(_OfferID);
	}

	static int LoadProgress(string _OfferID)
	{
		return PlayerPrefs.GetInt($"{_OfferID}_PROGRESS", 0);
	}

	static void SaveProgress(string _OfferID, int _Progress)
	{
		PlayerPrefs.SetInt($"{_OfferID}_PROGRESS", _Progress);
	}

	static void DeleteProgress(string _OfferID)
	{
		PlayerPrefs.DeleteKey($"{_OfferID}_PROGRESS");
	}
}
