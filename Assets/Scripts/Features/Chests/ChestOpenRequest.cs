using System.Collections.Generic;

public class ChestOpenRequest : FunctionRequest<ChestReward>
{
	protected override string Command => "ChestOpen";

	readonly int m_Slot;

	public ChestOpenRequest(int _Slot)
	{
		m_Slot = _Slot;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["slot"] = m_Slot;
	}

	protected override ChestReward Success(object _Data)
	{
		return _Data is Dictionary<string, object> data ? new ChestReward(data) : null;
	}

	protected override ChestReward Fail()
	{
		return null;
	}
}
