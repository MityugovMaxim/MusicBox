using System.Collections.Generic;

public class ChestOpenRequest : FunctionRequest<ChestReward>
{
	protected override string Command => "ChestOpen";

	readonly string m_ChestID;

	public ChestOpenRequest(string _ChestID)
	{
		m_ChestID = _ChestID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["chest_id"] = m_ChestID;
	}

	protected override ChestReward Success(object _Data)
	{
		Dictionary<string, object> data = _Data as Dictionary<string, object>;
		
		return data != null ? new ChestReward(data) : null;
	}

	protected override ChestReward Fail()
	{
		return null;
	}
}
