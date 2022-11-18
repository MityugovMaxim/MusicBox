using System.Collections.Generic;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class LanguageSnapshot : Snapshot
{
	public bool   Active { get; }
	public string Name   { get; }

	public LanguageSnapshot() : base("language_code", 0)
	{
		Active = false;
		Name   = "language_name";
	}

	public LanguageSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active = _Data.GetBool("active");
		Name   = _Data.GetString("name");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"] = Active;
		_Data["name"]   = Name;
	}

	public override string ToString() => Name;
}