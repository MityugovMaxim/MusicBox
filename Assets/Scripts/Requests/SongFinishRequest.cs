using System.Collections.Generic;

public class SongFinishRequest : FunctionRequest<bool>
{
	protected override string Command => "SongFinish";

	string SongID   { get; }
	long   Score    { get; }
	int    Accuracy { get; }
	bool   Double   { get; }

	public SongFinishRequest(
		string _SongID,
		long   _Score,
		int    _Accuracy,
		bool   _Double
	)
	{
		SongID   = _SongID;
		Score    = _Score;
		Accuracy = _Accuracy;
		Double   = _Double;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["song_id"]  = SongID;
		_Data["score"]    = Score;
		_Data["accuracy"] = Accuracy;
		_Data["double"]   = Double;
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