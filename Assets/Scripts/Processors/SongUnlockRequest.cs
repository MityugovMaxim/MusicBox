using System.Collections.Generic;

public class SongUnlockRequest : FunctionRequest<bool>
{
	protected override string Command => "SongUnlock";

	string SongID { get; }

	public SongUnlockRequest(string _SongID)
	{
		SongID = _SongID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["song_id"] = SongID;
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