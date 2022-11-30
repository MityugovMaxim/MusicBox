using Firebase.Database;

public class ProfileTransaction : Snapshot
{
	public string ProductID     { get; }
	public string TransactionID { get; }
	public long   Timestamp     { get; }

	public ProfileTransaction(DataSnapshot _Data) : base(_Data)
	{
		ProductID     = _Data.GetString("product_id");
		TransactionID = _Data.GetString("transaction_id");
		Timestamp     = _Data.GetLong("timestamp");
	}
}