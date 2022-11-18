using Firebase.Database;

public class DataSnapshot<TValue> : Snapshot
{
	public TValue Value { get; }

	public DataSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Value = Parse(_Data);
	}

	protected virtual TValue Parse(DataSnapshot _Data) => _Data.GetValue<TValue>();
}