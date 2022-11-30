using System.Collections.Generic;

public class FrameSelectRequest : FunctionRequest<bool>
{
	protected override string Command => "FrameSelect";

	readonly string m_FrameID;

	public FrameSelectRequest(string _FrameID)
	{
		m_FrameID = _FrameID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["frame_id"] = m_FrameID;
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