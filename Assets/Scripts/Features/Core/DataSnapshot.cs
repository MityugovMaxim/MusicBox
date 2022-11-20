using Firebase.Database;

public class DataSnapshot<TValue> : Snapshot
{
	public TValue Value { get; }

	public DataSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Value = _Data.GetValue<TValue>();
	}
}
