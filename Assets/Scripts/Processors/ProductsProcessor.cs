using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Scripting;
using Zenject;

public class ProductSnapshot : Snapshot
{
	public string       AppStoreID   { get; }
	public string       GooglePlayID { get; }
	public bool         Active       { get; }
	public string       Image        { get; }
	public ProductType  Type         { get; }
	public bool         Promo        { get; }
	public bool         Special      { get; }
	public bool         NoAds        { get; }
	public long         Coins        { get; }
	public string       Badge        { get; }
	public string       Color        { get; }
	public List<string> SongIDs      { get; }

	public ProductSnapshot() : base("new_product", 0)
	{
		Active       = false;
		Image        = "Thumbnails/Products/new_product.jpg";
		AppStoreID   = string.Empty;
		GooglePlayID = string.Empty;
		Type         = ProductType.Consumable;
		Promo        = false;
		Special      = false;
		NoAds        = false;
		Coins        = 0;
		Badge        = string.Empty;
		Color        = "#0081FFFF";
		SongIDs      = new List<string>();
	}

	public ProductSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active       = _Data.GetBool("active");
		Image        = _Data.GetString("image", $"Thumbnails/Products/{ID}.jpg");
		AppStoreID   = _Data.GetString("app_store", _Data.Key);
		GooglePlayID = _Data.GetString("google_play", _Data.Key);
		Type         = _Data.GetEnum<ProductType>("type");
		Promo        = _Data.GetBool("promo");
		Special      = _Data.GetBool("special");
		Coins        = _Data.GetLong("coins");
		Badge        = _Data.GetString("badge");
		Color        = _Data.GetString("color", "#0081FFFF");
		NoAds        = _Data.GetBool("no_ads");
		SongIDs      = _Data.GetChildKeys("song_ids");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]         = Active;
		_Data["image"]          = Image;
		_Data["app_store_id"]   = AppStoreID;
		_Data["google_play_id"] = GooglePlayID;
		_Data["type"]           = (int)Type;
		_Data["promo"]          = Promo;
		_Data["special"]        = Special;
		_Data["coins"]          = Coins;
		_Data["badge"]          = Badge;
		_Data["color"]          = Color;
		_Data["song_ids"]       = SongIDs;
	}
}

[Preserve]
public class ProductsDataUpdateSignal { }

[Preserve]
public class ProductsDescriptor : DescriptorProcessor<ProductsDataUpdateSignal>
{
	protected override string Path => "products_descriptors";
}

[Preserve]
public class ProductsProcessor : DataProcessor<ProductSnapshot, ProductsDataUpdateSignal>
{
	protected override string Path => "products";

	[Inject] ProductsDescriptor m_ProductsDescriptor;

	protected override Task OnFetch()
	{
		return m_ProductsDescriptor.Load();
	}

	public List<string> GetProductIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public IDs GetStoreIDs(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null)
			return null;
		
		return new IDs()
		{
			{ snapshot.AppStoreID, AppleAppStore.Name },
			{ snapshot.GooglePlayID, GooglePlay.Name },
		};
	}

	public string GetImage(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _ProductID) => m_ProductsDescriptor.GetTitle(_ProductID);

	public string GetDescription(string _ProductID) => m_ProductsDescriptor.GetDescription(_ProductID);

	public ProductType GetType(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Type ?? ProductType.Consumable;
	}

	public long GetCoins(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Coins ?? 0;
	}

	public string GetBadge(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Badge ?? string.Empty;
	}

	public Color GetColor(string _ProductID)
	{
		Color fallback = new Color(0, 0.5f, 1);
		
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		if (snapshot == null || string.IsNullOrEmpty(snapshot.Color) || !snapshot.Color.StartsWith('#'))
			return fallback;
		
		return ColorUtility.TryParseHtmlString(snapshot.Color, out Color color) ? color : fallback;
	}

	public List<string> GetSongIDs(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.SongIDs != null
			? snapshot.SongIDs.ToList()
			: new List<string>();
	}

	public bool IsPromo(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Promo ?? false;
	}

	public bool IsSpecial(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.Special ?? false;
	}

	public bool IsNoAds(string _ProductID)
	{
		ProductSnapshot snapshot = GetSnapshot(_ProductID);
		
		return snapshot?.NoAds ?? false;
	}

	public string GetCoinsProductID(long _Coins)
	{
		ProductSnapshot snapshot = Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Type == ProductType.Consumable)
			.OrderBy(_Snapshot => _Snapshot.Coins)
			.Aggregate((_A, _B) => _A.Coins < _B.Coins && _A.Coins >= _Coins ? _A : _B);
		
		return snapshot?.ID;
	}
}