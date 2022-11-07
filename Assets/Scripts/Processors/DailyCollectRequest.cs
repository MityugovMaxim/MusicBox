using System.Collections.Generic;

public class DailyCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "DailyCollect";

	string DailyID { get; }

	public DailyCollectRequest(string _DailyID)
	{
		DailyID = _DailyID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["daily_id"] = DailyID;
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