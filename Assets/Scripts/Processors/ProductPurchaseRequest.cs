using System.Collections.Generic;

public class ProductPurchaseRequest : FunctionRequest<bool>
{
	protected override string Command => "ProductPurchase";

	string ProductID { get; }
	string Receipt   { get; }

	public ProductPurchaseRequest(string _ProductID, string _Receipt)
	{
		ProductID = _ProductID;
		Receipt   = _Receipt;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["product_id"] = ProductID;
		_Data["receipt"]    = Receipt;
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