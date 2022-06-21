using System.Collections.Generic;

public class ProductPurchaseRequest : FunctionRequest<bool>
{
	protected override string Command => "ProductPurchase";

	string ProductID { get; }
	string Receipt   { get; }
	string Store     { get; }

	public ProductPurchaseRequest(string _ProductID, string _Receipt, string _Store)
	{
		ProductID = _ProductID;
		Receipt   = _Receipt;
		Store     = _Store;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["product_id"] = ProductID;
		_Data["receipt"]    = Receipt;
		_Data["store"]      = Store;
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