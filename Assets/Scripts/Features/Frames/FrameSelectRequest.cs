using System.Collections.Generic;

public class FrameSelectRequest : FunctionRequest<bool>
{
	protected override string Command => "FrameSelect";

	string FrameID { get; }

	public FrameSelectRequest(string _FrameID)
	{
		FrameID = _FrameID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["frame_id"] = FrameID;
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
