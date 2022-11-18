using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public class Descriptor : Snapshot
{
	public string Title       { get; }
	public string Description { get; }

	public Descriptor(string _ID) : base(_ID, 0)
	{
		Title       = string.Empty;
		Description = string.Empty;
	}

	public Descriptor(DataSnapshot _Data) : base(_Data)
	{
		Title       = _Data.GetString("title");
		Description = _Data.GetString("description");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["title"]       = Title;
		_Data["description"] = Description;
	}
}

[Preserve]
public abstract class DescriptorProcessor : DataCollection<Descriptor>
{
	protected override string Path => $"{Name}/{m_LanguagesManager.Language}";

	protected abstract string Name { get; }

	[Inject] LanguagesManager m_LanguagesManager;

	public string GetTitle(string _ID)
	{
		Descriptor descriptor = GetSnapshot(_ID);
		
		return descriptor?.Title ?? string.Empty;
	}

	public string GetDescription(string _ID)
	{
		Descriptor descriptor = GetSnapshot(_ID);
		
		return descriptor?.Description ?? string.Empty;
	}
}
