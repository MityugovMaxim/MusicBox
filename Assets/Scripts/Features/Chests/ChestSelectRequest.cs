using System.Collections.Generic;

public class ChestSelectRequest : FunctionRequest<bool>
{
	protected override string Command => "ChestSelect";

	readonly string m_ChestID;

	public ChestSelectRequest(string _ChestID)
	{
		m_ChestID = _ChestID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["chest_id"] = m_ChestID;
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
