using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public class LanguageSnapshot : Snapshot
{
	public bool   Active { get; }
	public string Name   { get; }

	public LanguageSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active = _Data.GetBool("active");
		Name   = _Data.GetString("name");
	}

	public override string ToString() => Name;
}
