using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LanguagesCollection : DataCollection<LanguageSnapshot>
{
	protected override string Path => "languages";

	public List<string> GetLanguages(bool _IncludeInactive = false)
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _IncludeInactive || _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetName(string _Language)
	{
		LanguageSnapshot snapshot = GetSnapshot(_Language);
		
		return snapshot?.Name ?? _Language;
	}
}
