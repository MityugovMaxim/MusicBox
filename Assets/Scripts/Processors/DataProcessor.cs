using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class Snapshot
{
	public string ID    { get; }
	public int    Order { get; set; }

	protected Snapshot(string _ID, int _Order)
	{
		ID    = _ID;
		Order = _Order;
	}

	protected Snapshot(DataSnapshot _Data)
	{
		ID    = _Data.Key;
		Order = _Data.GetInt("order");
	}

	public virtual void Serialize(Dictionary<string, object> _Data)
	{
		_Data["order"] = Order;
	}

	public override string ToString() => ID;
}

[Preserve]
public abstract class DataProcessor<TSnapshot> where TSnapshot : Snapshot
{
	protected abstract string Path { get; }

	protected SignalBus SignalBus => m_SignalBus;

	protected IReadOnlyList<TSnapshot>               Snapshots => m_Snapshots;
	protected IReadOnlyDictionary<string, TSnapshot> Registry  => m_Registry;

	bool Loaded { get; set; }

	[Inject] SignalBus m_SignalBus;

	string            m_DataPath;
	DatabaseReference m_Data;

	readonly List<TSnapshot>               m_Snapshots = new List<TSnapshot>();
	readonly Dictionary<string, TSnapshot> m_Registry  = new Dictionary<string, TSnapshot>();

	public async Task Load()
	{
		if (m_Data == null || m_DataPath != Path)
		{
			m_DataPath          =  Path;
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child(Path);
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		await OnFetch();
		
		Loaded = true;
	}

	protected void Unload()
	{
		if (m_Data != null)
		{
			m_DataPath          =  null;
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	async void OnUpdate(object _Sender, ValueChangedEventArgs _EventArgs)
	{
		if (!Loaded)
			return;
		
		if (_EventArgs.DatabaseError != null)
		{
			Log.Error(this, _EventArgs.DatabaseError.Message);
			Unload();
			return;
		}
		
		m_Snapshots.Clear();
		
		if (_EventArgs.Snapshot == null)
			return;
		
		m_Snapshots.AddRange(_EventArgs.Snapshot.Children.Select(Create));
		
		await OnUpdate();
		
		FireSignal();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		m_Registry.Clear();
		
		DataSnapshot dataSnapshot = await Filter(m_Data).GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch data failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(Create));
		
		foreach (TSnapshot snapshot in m_Snapshots)
			m_Registry[snapshot.ID] = snapshot;
	}

	protected async Task<bool> Download(ICollection<string> _Paths)
	{
		if (_Paths == null || _Paths.Count == 0)
			return false;
		
		List<Task<DataSnapshot>> tasks = new List<Task<DataSnapshot>>();
		
		foreach (string path in _Paths)
		{
			if (string.IsNullOrEmpty(path))
				continue;
			
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
			
			tasks.Add(reference.GetValueAsync());
		}
		
		try
		{
			await Task.WhenAll(tasks);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (FirebaseException) { }
		
		TSnapshot[] snapshots = tasks
			.Where(_Task => _Task.IsCompletedSuccessfully)
			.Select(_Task => _Task.Result)
			.Where(_Data => _Data.Exists)
			.Select(Create)
			.ToArray();
		
		foreach (TSnapshot snapshot in snapshots)
		{
			if (snapshot == null)
				continue;
			
			m_Snapshots.Add(snapshot);
			
			m_Registry[snapshot.ID] = snapshot;
		}
		
		return true;
	}

	protected bool Contains(string _ID)
	{
		return m_Registry.ContainsKey(_ID);
	}

	protected virtual Task OnFetch() => Task.CompletedTask;

	protected virtual Task OnUpdate() => Task.CompletedTask;

	protected virtual Query Filter(DatabaseReference _Data)
	{
		return _Data.OrderByChild("order");
	}

	protected virtual TSnapshot Create(DataSnapshot _Data)
	{
		return Activator.CreateInstance(typeof(TSnapshot), _Data) as TSnapshot;
	}

	protected TSnapshot GetSnapshot(string _ID)
	{
		if (string.IsNullOrEmpty(_ID))
			return null;
		
		if (m_Registry.TryGetValue(_ID, out TSnapshot snapshot) && snapshot != null)
			return snapshot;
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _ID);
	}

	protected abstract void FireSignal();

	public int GetOrder(string _ID)
	{
		TSnapshot snapshot = GetSnapshot(_ID);
		
		return snapshot?.Order ?? 0;
	}
}

public abstract class DataProcessor<TSnapshot, TSignal> : DataProcessor<TSnapshot> where TSnapshot : Snapshot
{
	[Inject] SignalBus m_SignalBus;

	protected override void FireSignal()
	{
		m_SignalBus.Fire<TSignal>();
	}
}