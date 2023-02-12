using Firebase.Database;

public class ComboSnapshot : Snapshot
{
	public int Multiplier { get; }
	public int Count      { get; }

	public ComboSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Multiplier = _Data.GetInt("multiplier");
		Count      = _Data.GetInt("count");
	}

	public override string ToString()
	{
		return $"X{Multiplier} Combo";
	}
}
