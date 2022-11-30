using System.Collections.Generic;
using System.Linq;
using Firebase.Database;

public class ProductSnapshot : Snapshot
{
	public bool         Active       { get; }
	public string       AppStoreID   { get; }
	public string       GooglePlayID { get; }
	public string       Image        { get; }
	public ProductType  Type         { get; }
	public long         Coins        { get; }
	public string       SeasonID     { get; }
	public List<string> SongIDs      { get; }

	public ProductSnapshot() : base("PRODUCT", 0)
	{
		Active       = false;
		Image        = "Thumbnails/Products/PRODUCT.jpg";
		AppStoreID   = string.Empty;
		GooglePlayID = string.Empty;
		Type         = ProductType.Coins;
		Coins        = 0;
		SeasonID     = string.Empty;
		SongIDs      = new List<string>();
	}

	public ProductSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active       = _Data.GetBool("active");
		Image        = _Data.GetString("image", $"Thumbnails/Products/{ID}.jpg");
		AppStoreID   = _Data.GetString("app_store_id", _Data.Key);
		GooglePlayID = _Data.GetString("google_play_id", _Data.Key);
		Type         = _Data.GetEnum<ProductType>("type");
		Coins        = _Data.GetLong("coins");
		SeasonID     = _Data.GetString("season_id");
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
		_Data["coins"]          = Coins;
		_Data["season_id"]      = SeasonID;
		_Data["song_ids"]       = SongIDs.ToDictionary(_SongID => _SongID, _ => true);
	}
}
