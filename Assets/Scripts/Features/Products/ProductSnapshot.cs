using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine.Purchasing;

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
	public bool         BattlePass   { get; }
	public long         Coins        { get; }
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
		SongIDs      = new List<string>();
	}

	public ProductSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active       = _Data.GetBool("active");
		Image        = _Data.GetString("image", $"Thumbnails/Products/{ID}.jpg");
		AppStoreID   = _Data.GetString("app_store_id", _Data.Key);
		GooglePlayID = _Data.GetString("google_play_id", _Data.Key);
		Type         = _Data.GetEnum<ProductType>("type");
		Promo        = _Data.GetBool("promo");
		Special      = _Data.GetBool("special");
		Coins        = _Data.GetLong("coins");
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
		_Data["song_ids"]       = SongIDs.ToDictionary(_SongID => _SongID, _SongID => true);
	}
}
