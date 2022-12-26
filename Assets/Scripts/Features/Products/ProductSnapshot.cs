using System.Collections.Generic;
using System.Linq;
using Firebase.Database;

public class ProductSnapshot : Snapshot
{
	public bool         Active   { get; }
	public string       StoreID  { get; }
	public string       Image    { get; }
	public ProductType  Type     { get; }
	public long         Coins    { get; }
	public string       SeasonID { get; }
	public List<string> SongIDs  { get; }

	public ProductSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active   = _Data.GetBool("active");
		Image    = _Data.GetString("image", $"Thumbnails/Products/{ID}.jpg");
		StoreID  = _Data.GetString("store_id", _Data.Key);
		Type     = _Data.GetEnum<ProductType>("type");
		Coins    = _Data.GetLong("coins");
		SeasonID = _Data.GetString("season_id");
		SongIDs  = _Data.GetChildKeys("song_ids");
	}
}
