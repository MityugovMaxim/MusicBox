using System.Collections.Generic;

public class ProductCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "ProductCollect";

	string ProductID { get; }

	public ProductCollectRequest(string _ProductID)
	{
		ProductID = _ProductID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["product_id"] = ProductID;
	}

	protected override bool Success(object _Data)
	{
		return _Data is true;
	}

	protected override bool Fail()
	{
		return false;
	}
}
