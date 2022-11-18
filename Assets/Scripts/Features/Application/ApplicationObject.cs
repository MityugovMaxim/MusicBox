using Firebase.Database;

public class ApplicationObject
{
	public string Version             { get; }
	public string AppStoreReviewURL   { get; }
	public string GooglePlayReviewURL { get; }

	public ApplicationObject(DataSnapshot _Data)
	{
		Version = _Data.GetString("version");
		#if UNITY_IOS
		Version = _Data.GetString("app_store_version", Version);
		#elif UNITY_ANDROID
		Version = _Data.GetString("google_play_version", Version);
		#endif
		AppStoreReviewURL   = _Data.GetString("app_store_review_url");
		GooglePlayReviewURL = _Data.GetString("google_play_review_url");
	}
}