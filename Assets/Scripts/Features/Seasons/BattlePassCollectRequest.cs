using System.Collections.Generic;

public class BattlePassCollectRequest : FunctionRequest<bool>
{
	protected override string Command => "BattlePassCollect";

	readonly string m_BattlePassID;
	readonly int    m_Level;
	readonly bool   m_Free;

	public BattlePassCollectRequest(string _BattlePassID, int _Level, bool _Free)
	{
		m_BattlePassID = _BattlePassID;
		m_Level        = _Level;
		m_Free         = _Free;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["id"]    = m_BattlePassID;
		_Data["level"] = m_Level;
		_Data["free"]  = m_Free;
	}

	protected override bool Success(object _Data) => _Data != null && (bool)_Data;

	protected override bool Fail() => false;
}
