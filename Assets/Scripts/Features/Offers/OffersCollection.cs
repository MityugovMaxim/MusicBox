using UnityEngine.Scripting;

[Preserve]
public class OffersCollection : DataCollection<OfferSnapshot>, IDataCollection
{
	public DataPriority Priority => DataPriority.Low;

	protected override string Path => "offers";
}
