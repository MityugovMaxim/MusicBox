using System.Collections.Generic;

public class UsernameRequest : FunctionRequest<string>
{
	protected override string Command => "GetUsername";

	string UserID { get; }

	public UsernameRequest(string _UserID)
	{
		UserID = _UserID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["user_id"] = UserID;
	}

	protected override string Success(object _Data)
	{
		return _Data != null ? (string)_Data : string.Empty;
	}

	protected override string Fail()
	{
		return string.Empty;
	}
}