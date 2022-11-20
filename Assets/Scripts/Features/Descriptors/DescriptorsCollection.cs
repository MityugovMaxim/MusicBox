using UnityEngine.Scripting;
using Zenject;

[Preserve]
public abstract class DescriptorsCollection : DataCollection<Descriptor>
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
