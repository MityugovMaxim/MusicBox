using System.Collections.Generic;

public class DailyCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "DailyCollect";

	protected override void Serialize(IDictionary<string, object> _Data) { }

	protected override bool Success(object _Data)
	{
		return _Data != null && (bool)_Data;
	}

	protected override bool Fail()
	{
		return false;
	}
}
