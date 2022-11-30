using UnityEngine.Purchasing;

public class StoreProduct
{
	public string ProductID { get; }
	public IDs    StoreIDs  { get; }

	public StoreProduct(string _ProductID, string _AppStoreID, string _GooglePlayID)
	{
		ProductID = _ProductID;
		StoreIDs = new IDs
		{
			{ _AppStoreID, AppleAppStore.Name },
			{ _GooglePlayID, GooglePlay.Name },
		};
	}
}