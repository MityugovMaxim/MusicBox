using UnityEngine.Scripting;

[Preserve]
public class OffersCollection : DataCollection<OfferSnapshot>, IDataCollection
{
	public DataCollectionPriority Priority => DataCollectionPriority.Low;

	protected override string Path => "offers";
}
