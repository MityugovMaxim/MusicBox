using System.Collections.Generic;

public class SongCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "SongCollect";

	readonly string m_SongID;

	public SongCollectRequest(string _SongID)
	{
		m_SongID = _SongID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["song_id"] = m_SongID;
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
