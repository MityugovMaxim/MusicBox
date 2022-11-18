using UnityEngine.Scripting;

[Preserve]
public class OffersCollection : DataCollection<OfferSnapshot>
{
	protected override string Path => "offers";
}
