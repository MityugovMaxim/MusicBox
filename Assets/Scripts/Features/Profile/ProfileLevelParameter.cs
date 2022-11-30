using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ProfileLevelParameter : ProfileParameter<int>, IDataObject
{
	protected override string Name => "level";

	protected override int Create(DataSnapshot _Data) => _Data.GetInt();
}
