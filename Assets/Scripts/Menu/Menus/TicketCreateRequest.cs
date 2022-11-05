using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TicketCreateRequest : FunctionRequest<bool>
{
	public enum GroupType
	{
		All        = 0,
		User       = 1,
		Region     = 2,
		F2P        = 3,
		Purchasers = 4,
	}

	protected override string Command => "TicketCreate";

	readonly ProfileTicketType m_Type;
	readonly GroupType         m_Group;
	readonly string            m_UserID;
	readonly string            m_Region;
	readonly long              m_Amount;
	readonly long              m_ExpirationTimestamp;

	public TicketCreateRequest(
		ProfileTicketType _Type,
		GroupType         _Group,
		string            _UserID,
		string            _Region,
		long              _Amount,
		long              _ExpirationTimestamp
	)
	{
		m_Type                = _Type;
		m_Group               = _Group;
		m_UserID              = _UserID;
		m_Region              = _Region;
		m_Amount              = _Amount;
		m_ExpirationTimestamp = _ExpirationTimestamp;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["type"]                 = (int)m_Type;
		_Data["amount"]               = m_Amount;
		_Data["expiration_timestamp"] = m_ExpirationTimestamp;
		_Data["group"]                = (int)m_Group;
		_Data["user_id"]              = m_UserID;
		_Data["region"]               = m_Region;
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
