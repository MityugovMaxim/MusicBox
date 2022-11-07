using System.Collections.Generic;
using System.Linq;

public class VoucherCreateRequest : FunctionRequest<bool>
{
	protected override string Command => "VoucherCreate";

	readonly VoucherType  m_Type;
	readonly VoucherGroup m_Group;
	readonly long         m_Amount;
	readonly List<string> m_IDs;
	readonly long         m_Expiration;

	public VoucherCreateRequest(
		VoucherType  _Type,
		VoucherGroup _Group,
		long         _Amount,
		List<string> _IDs,
		long         _Expiration
	)
	{
		m_Type       = _Type;
		m_Group      = _Group;
		m_Amount     = _Amount;
		m_IDs        = _IDs;
		m_Expiration = _Expiration;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["type"]       = (int)m_Type;
		_Data["group"]      = (int)m_Group;
		_Data["amount"]     = m_Amount;
		_Data["ids"]        = m_IDs.ToDictionary(_ID => _ID, _ => true);
		_Data["expiration"] = m_Expiration;
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
