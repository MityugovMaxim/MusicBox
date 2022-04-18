using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public class Descriptor
{
	public string ID          { get; }
	public string Title       { get; }
	public string Description { get; }

	public Descriptor(DataSnapshot _Data)
	{
		ID          = _Data.Key;
		Title       = _Data.GetString("title");
		Description = _Data.GetString("description");
	}
}

[Preserve]
public abstract class DescriptorProcessor<TSignal> : IInitializable, IDisposable
{
	protected abstract string Path { get; }

	bool Loaded { get; set; }

	[Inject] SignalBus         m_SignalBus;
	[Inject] LanguageProcessor m_LanguageProcessor;

	readonly Dictionary<string, Descriptor> m_Descriptors = new Dictionary<string, Descriptor>();

	DatabaseReference m_Data;

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	public async Task Load()
	{
		if (m_Data == null)
		{
			string path = $"{Path}/{m_LanguageProcessor.Language}";
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child(path);
			m_Data.ValueChanged += OnUpdate;
		}

		await Fetch();

		Loaded = true;
	}

	public string GetTitle(string _ID)
	{
		Descriptor descriptor = GetDescriptor(_ID);

		return descriptor?.Title ?? string.Empty;
	}

	public string GetDescription(string _ID)
	{
		Descriptor descriptor = GetDescriptor(_ID);

		return descriptor?.Description ?? string.Empty;
	}

	async void OnLanguageSelect()
	{
		if (m_Data == null)
			return;

		m_Data.ValueChanged -= OnUpdate;
		m_Data              =  null;
		Loaded              =  false;

		await Load();

		m_SignalBus.Fire<TSignal>();
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;

		Log.Info(this, "Updating descriptors...");

		await Fetch();

		Log.Info(this, "Update descriptors complete.");

		m_SignalBus.Fire<TSignal>();
	}

	async Task Fetch()
	{
		m_Descriptors.Clear();

		DataSnapshot dataSnapshot = await m_Data.GetValueAsync();

		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch descriptors failed.");
			return;
		}

		IEnumerable<Descriptor> descriptors = dataSnapshot.Children.Select(_Data => new Descriptor(_Data));
		foreach (Descriptor descriptor in descriptors)
			m_Descriptors[descriptor.ID] = descriptor;
	}

	Descriptor GetDescriptor(string _ID)
	{
		if (string.IsNullOrEmpty(_ID))
		{
			Log.Error(this, "Get descriptor failed. ID is null or empty.");
			return null;
		}

		if (m_Descriptors.TryGetValue(_ID, out Descriptor descriptor))
			return descriptor;

		Log.Error(this, "Get descriptor failed. Descriptor with ID '{0}' is null.", _ID);

		return null;
	}
}