using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ProfileOffer : Snapshot
{
	public long Timestamp { get; }

	public ProfileOffer(DataSnapshot _Data) : base(_Data)
	{
		Timestamp = _Data.GetLong("timestamp");
	}
}
