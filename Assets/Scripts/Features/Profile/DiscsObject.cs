using Firebase.Database;

public class DiscsObject
{
	public int Bronze   { get; }
	public int Silver   { get; }
	public int Gold     { get; }
	public int Platinum { get; }
	public int Count    { get; }

	public DiscsObject(DataSnapshot _Data)
	{
		Bronze   = _Data.GetInt("bronze");
		Silver   = _Data.GetInt("silver");
		Gold     = _Data.GetInt("gold");
		Platinum = _Data.GetInt("platinum");
		Count    = Bronze + Silver + Gold + Platinum;
	}
}
