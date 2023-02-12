using System.Collections.Generic;

public class StoreValidateRequest : FunctionRequest<bool>
{
	protected override string Command => "StoreValidate";

	string StoreID { get; }
	string Receipt { get; }
	string Store   { get; }

	public StoreValidateRequest(string _StoreID, string _Receipt, string _Store)
	{
		StoreID = _StoreID;
		Receipt = _Receipt;
		Store   = _Store;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["store_id"] = StoreID;
		_Data["receipt"]  = Receipt;
		_Data["store"]    = Store;
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
