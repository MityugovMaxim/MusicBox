using System.Collections.Generic;

public class SongReviveRequest : FunctionRequest<bool>
{
	protected override string Command => "SongRevive";

	int Count { get; }

	public SongReviveRequest(int _Count)
	{
		Count = _Count;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["count"] = Count;
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