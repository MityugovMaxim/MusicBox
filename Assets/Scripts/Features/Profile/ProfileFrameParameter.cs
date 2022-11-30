using Firebase.Database;

public class ProfileFrameParameter : ProfileParameter<string>
{
	protected override string Name => "frame";

	protected override string Create(DataSnapshot _Data) => _Data.GetString();
}