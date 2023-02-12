using System.Collections.Generic;

public class ChestSelectRequest : FunctionRequest<bool>
{
	protected override string Command => "ChestSelect";

	readonly RankType m_Rank;

	public ChestSelectRequest(RankType _Rank)
	{
		m_Rank = _Rank;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["rank"] = (int)m_Rank;
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
