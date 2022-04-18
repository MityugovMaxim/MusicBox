using System.Collections.Generic;

public class SongRatingRequest : FunctionRequest<bool>
{
	protected override string Command => "SongRating";

	string SongID { get; }
	int    Rating { get; }

	public SongRatingRequest(string _SongID, int _Rating)
	{
		SongID = _SongID;
		Rating = _Rating;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["song_id"] = SongID;
		_Data["rating"]  = Rating;
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