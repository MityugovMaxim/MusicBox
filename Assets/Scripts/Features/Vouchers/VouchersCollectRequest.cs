using System.Collections.Generic;

public class VouchersCollectRequest : FunctionRequest<RequestState>
{
	protected override string Command => "VouchersCollect";

	readonly string[] m_VoucherIDs;

	public VouchersCollectRequest(params string[] _VoucherIDs)
	{
		m_VoucherIDs = _VoucherIDs;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["voucher_ids"] = m_VoucherIDs;
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
