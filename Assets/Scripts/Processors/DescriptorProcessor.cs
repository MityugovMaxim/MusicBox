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
	public string ID          => m_ID;
	public string Title       { get; set; }
	[TextProperty]
	public string Description { get; set; }

	string m_ID;

	public Descriptor(string _ID)
	{
		m_ID        = _ID;
		Title       = $"{ID} title";
		Description = $"{ID} description";
	}

	public Descriptor(DataSnapshot _Data)
	{
		m_ID        = _Data.Key;
		Title       = _Data.GetString("title");
		Description = _Data.GetString("description");
	}

	public void Setup(string _ID)
	{
		m_ID = _ID;
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["title"]       = Title;
		data["description"] = Description;
		
		return data;
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

	public async Task Upload()
	{
		Loaded = false;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (Descriptor descriptor in m_Descriptors.Values)
		{
			if (descriptor != null)
				data[descriptor.ID] = descriptor.Serialize();
		}
		
		await m_Data.SetValueAsync(data);
		
		await Fetch();
		
		Loaded = true;
	}

	public async Task Upload(params string[] _IDs)
	{
		if (_IDs == null || _IDs.Length == 0)
			return;
		
		Loaded = false;
		
		foreach (string id in _IDs)
		{
			Descriptor descriptor = GetDescriptor(id);
			
			Dictionary<string, object> data = descriptor?.Serialize();
			
			await m_Data.Child(id).SetValueAsync(data);
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public Descriptor CreateDescriptor(string _ID)
	{
		if (string.IsNullOrEmpty(_ID))
		{
			Log.Error(this, "Create descriptor failed. ID is null or empty.");
			return null;
		}
		
		if (m_Descriptors.ContainsKey(_ID))
		{
			Log.Error(this, "Create descriptor failed. Descriptor with ID '{0}' already exists.", _ID);
			return null;
		}
		
		m_Descriptors[_ID] = new Descriptor(_ID);
		
		return m_Descriptors[_ID];
	}

	public void RemoveDescriptor(string _ID)
	{
		if (m_Descriptors.ContainsKey(_ID))
			m_Descriptors.Remove(_ID);
	}

	public Descriptor GetDescriptor(string _ID)
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