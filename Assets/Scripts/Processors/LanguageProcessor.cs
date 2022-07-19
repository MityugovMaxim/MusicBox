using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

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

[Preserve]
public class LanguageDataUpdateSignal { }

[Preserve]
public class LanguageSelectSignal { }

[Preserve]
public class LanguageProcessor : DataProcessor<LanguageSnapshot, LanguageDataUpdateSignal>
{
	const string LANGUAGE_KEY = "LANGUAGE";

	public string Language { get; private set; }

	protected override string Path => "languages";

	[Inject] LocalizationProcessor m_LocalizationProcessor;

	protected override Task OnFetch()
	{
		Language = Determine();
		
		return m_LocalizationProcessor.Load(Language);
	}

	protected override Task OnUpdate()
	{
		string language = Determine();
		
		if (Language == language)
			return Task.CompletedTask;
		
		return Select(language);
	}

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

	public async Task<bool> Select(string _Language)
	{
		if (Language == _Language)
			return false;
		
		LanguageSnapshot snapshot = GetSnapshot(_Language);
		
		if (snapshot == null)
			return false;
		
		PlayerPrefs.SetString(LANGUAGE_KEY, snapshot.ID);
		
		Language = snapshot.ID;
		
		await m_LocalizationProcessor.Load(Language);
		
		FireSignal();
		
		return true;
	}

	public async Task Reload()
	{
		await m_LocalizationProcessor.Reload(Language);
		
		FireSignal();
	}

	string Determine()
	{
		string language = PlayerPrefs.HasKey(LANGUAGE_KEY)
			? PlayerPrefs.GetString(LANGUAGE_KEY)
			: Application.systemLanguage.GetCode();
		
		LanguageSnapshot snapshot = GetSnapshot(language);
		if (snapshot != null)
			return snapshot.ID;
		
		LanguageSnapshot fallback = GetSnapshot(SystemLanguage.English.GetCode());
		if (fallback != null)
			return fallback.ID;
		
		LanguageSnapshot supported = Snapshots.FirstOrDefault(_Snapshot => _Snapshot != null);
		if (supported != null)
			return supported.ID;
		
		Log.Error(this, "Determine language failed.");
		
		return null;
	}
}