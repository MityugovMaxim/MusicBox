using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class ProfilePointsParameter : ProfileParameter<long>, IDataObject
{
	protected override string Name => "points";

	protected override long Create(DataSnapshot _Data)
	{
		long points = _Data.GetLong();
		
		return points;
	}
}