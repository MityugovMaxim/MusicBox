using Firebase.Database;
using UnityEngine.Scripting;

namespace Features.Offers
{
	[Preserve]
	public class ProfileOffer : Snapshot
	{
		public long CollectTimestamp { get; }

		public ProfileOffer(DataSnapshot _Data) : base(_Data)
		{
			CollectTimestamp = _Data.GetLong("collect_timestamp");
		}
	}
}