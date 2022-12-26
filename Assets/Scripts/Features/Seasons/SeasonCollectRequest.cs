using System.Collections.Generic;

public class SeasonCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "SeasonCollect";

	readonly string         m_SeasonID;
	readonly int            m_Level;
	readonly SeasonItemMode m_Mode;

	public SeasonCollectRequest(string _SeasonID, int _Level, SeasonItemMode _Mode)
	{
		m_SeasonID = _SeasonID;
		m_Level    = _Level;
		m_Mode     = _Mode;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["season_id"] = m_SeasonID;
		_Data["level"]     = m_Level;
		_Data["mode"]      = (int)m_Mode;
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
