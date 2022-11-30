using System.Collections.Generic;

public class SeasonCompleteRequest : FunctionRequest<bool>
{
	protected override string Command => "SeasonComplete";

	readonly string m_SeasonID;

	public SeasonCompleteRequest(string _SeasonID)
	{
		m_SeasonID = _SeasonID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["season_id"] = m_SeasonID;
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