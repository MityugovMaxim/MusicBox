using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class OfferSnapshot : Snapshot
{
	public bool   Active    { get; }
	public string Image     { get; }
	public string SongID    { get; }
	public long   Coins     { get; }
	public int    AdsCount  { get; }
	public long   Timestamp { get; }

	public OfferSnapshot() : base("new_offer", 0)
	{
		Active    = false;
		Image     = "Thumbnails/Offers/new_offer.jpg";
		SongID    = string.Empty;
		Coins     = 0;
		AdsCount  = 0;
		Timestamp = 0;
	}

	public OfferSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/Offers/{ID}.jpg");
		SongID    = _Data.GetString("song_id");
		Coins     = _Data.GetLong("coins");
		AdsCount  = _Data.GetInt("ads_count");
		Timestamp = _Data.GetLong("timestamp");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]    = Active;
		_Data["image"]     = Image;
		_Data["song_id"]   = SongID;
		_Data["coins"]     = Coins;
		_Data["ads_count"] = AdsCount;
		_Data["timestamp"] = Timestamp;
	}
}

[Preserve]
public class OffersDataUpdateSignal { }

[Preserve]
public class OffersDescriptor : DescriptorProcessor<OffersDataUpdateSignal>
{
	protected override string Path => "offers_descriptors";
}

[Preserve]
public class OffersProcessor : DataProcessor<OfferSnapshot, OffersDataUpdateSignal>
{
	protected override string Path => "offers";

	[Inject] OffersDescriptor m_OffersDescriptor;

	protected override Task OnFetch() => m_OffersDescriptor.Load();

	public List<string> GetOfferIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.OrderBy(_Snapshot => _Snapshot.Order)
			.ThenByDescending(_Snapshot => _Snapshot.Timestamp)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetImage(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
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
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		return snapshot?.SongID ?? string.Empty;
	}

	public long GetCoins(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		return snapshot?.Coins ?? 0;
	}

	public int GetAdsCount(string _OfferID)
	{
		OfferSnapshot snapshot = GetSnapshot(_OfferID);
		
		if (snapshot == null)
		{
			Log.Error(this, "Get rewarded count failed. Snapshot with ID '{0}' is null.", _OfferID);
			return 0;
		}
		
		return snapshot.AdsCount;
	}
}