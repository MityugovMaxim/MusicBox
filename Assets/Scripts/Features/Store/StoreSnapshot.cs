using Firebase.Database;

public class StoreSnapshot : Snapshot
{
	public string AppStoreID   { get; }
	public string GooglePlayID { get; }

	public StoreSnapshot(DataSnapshot _Data) : base(_Data)
	{
		AppStoreID   = _Data.GetString("app_store_id");
		GooglePlayID = _Data.GetString("google_play_id");
	}
}