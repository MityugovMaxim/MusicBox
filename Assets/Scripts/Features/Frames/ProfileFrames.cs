using Firebase.Database;

public class ProfileFrames : ProfileParameter<ProfileFrame>, IDataObject
{
	protected override string Name => "frames";

	protected override ProfileFrame Create(DataSnapshot _Data) => new ProfileFrame(_Data);
}
