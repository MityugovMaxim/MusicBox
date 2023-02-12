using Firebase.Database;
using JetBrains.Annotations;

public class PurchaseSnapshot : Snapshot
{
	public string TransactionID { [UsedImplicitly] get; }
	public string ProductID     { [UsedImplicitly] get; }
	public long   Timestamp     { [UsedImplicitly] get; }

	public PurchaseSnapshot(DataSnapshot _Data) : base(_Data)
	{
		TransactionID = _Data.GetString("transaction_id");
		ProductID     = _Data.GetString("product_id");
		Timestamp     = _Data.GetLong("timestamp");
	}
}
