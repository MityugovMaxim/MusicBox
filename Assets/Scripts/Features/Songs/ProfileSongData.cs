using Firebase.Database;

public class ProfileSongData : Snapshot
{
	public bool Value { get; }

	public ProfileSongData(DataSnapshot _Data) : base(_Data)
	{
		Value = _Data.GetBool();
	}
}
