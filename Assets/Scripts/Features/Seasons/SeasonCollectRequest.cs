using System.Collections.Generic;

public class SeasonCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "SeasonCollect";

	readonly string m_SeasonID;
	readonly string m_ItemID;

	public SeasonCollectRequest(string _SeasonID, string _ItemID)
	{
		m_SeasonID = _SeasonID;
		m_ItemID   = _ItemID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["season_id"] = m_SeasonID;
		_Data["item_id"]   = m_ItemID;
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