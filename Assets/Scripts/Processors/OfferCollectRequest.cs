using System.Collections.Generic;

public class OfferCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "OfferCollect";

	string OfferID { get; }

	public OfferCollectRequest(string _OfferID)
	{
		OfferID = _OfferID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["offer_id"] = OfferID;
	}

	protected override bool Success(object _Data)
	{
		return _Data != null && (bool)_Data;
	}

	protected override bool Fail()
	{
		return false;
	}
}