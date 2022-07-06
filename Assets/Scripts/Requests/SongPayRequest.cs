using System.Collections.Generic;

public class SongPayRequest : FunctionRequest<bool>
{
	protected override string Command => "SongPay";

	string SongID { get; }

	public SongPayRequest(string _SongID)
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