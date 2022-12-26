using System.Collections.Generic;

public class VoucherCollectRequest : FunctionRequest<RequestState>
{
	protected override string Command => "VoucherCollect";

	readonly string m_VoucherID;

	public VoucherCollectRequest(string _VoucherID)
	{
		m_VoucherID = _VoucherID;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["voucher_id"] = m_VoucherID;
	}

	protected override RequestState Success(object _Data)
	{
		return _Data != null && (bool)_Data ? RequestState.Success : RequestState.Fail;
	}

	protected override RequestState Fail()
	{
		return RequestState.Fail;
	}
}
