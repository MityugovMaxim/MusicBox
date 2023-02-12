using System.Collections.Generic;

public class SongsUpdateRequest : FunctionRequest<bool>
{
	protected override string Command => "SongsUpdate";

	readonly string[] m_SongIDs;

	public SongsUpdateRequest(string[] _SongIDs)
	{
		m_SongIDs = _SongIDs;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["song_ids"] = m_SongIDs;
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
