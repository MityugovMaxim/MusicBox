using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class Snapshot
{
	public string ID    { get; }
	#if ADMIN
	public int    Order { get; set; }
	#else
	public int    Order { get; }
	#endif

	protected Snapshot(DataSnapshot _Data)
	{
		ID    = _Data.Key;
		Order = _Data.GetInt("order");
	}

	public virtual void Serialize(Dictionary<string, object> _Data)
	{
		_Data["order"] = Order;
	}
}

[Preserve]
public abstract class DataProcessor<TSnapshot> where TSnapshot : Snapshot
{
	protected abstract string Path { get; }

	protected SignalBus SignalBus => m_SignalBus;

	protected IReadOnlyList<TSnapshot> Snapshots => m_Snapshots;

	bool Loaded { get; set; }

	[Inject] SignalBus m_SignalBus;

	string            m_DataPath;
	DatabaseReference m_Data;

	readonly List<TSnapshot> m_Snapshots = new List<TSnapshot>();

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

	void OnUpdate(object _Sender, ValueChangedEventArgs _EventArgs)
	{
		if (!Loaded)
			return;
		
		if (_EventArgs.DatabaseError != null)
		{
			Log.Error(this, _EventArgs.DatabaseError.Message);
			Log.Error(this, _EventArgs.DatabaseError.Details);
			Unload();
			return;
		}
		
		m_Snapshots.Clear();
		
		if (_EventArgs.Snapshot == null)
			return;
		
		m_Snapshots.AddRange(_EventArgs.Snapshot.Children.Select(Create));

		FireSignal();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await Filter(m_Data).GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch data failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(Create));
	}

	protected virtual Task OnFetch() => Task.CompletedTask;

	protected virtual Query Filter(DatabaseReference _Data)
	{
		return _Data.OrderByChild("order");
	}

	protected virtual TSnapshot Create(DataSnapshot _Data)
	{
		return Activator.CreateInstance(typeof(TSnapshot), _Data) as TSnapshot;
	}

	#if ADMIN
	public Task DeleteAsync(string _ID)
	{
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == _ID);
		
		return m_Data.Child(_ID).SetValueAsync(null);
	}

	public Task UploadAsync(string _ID)
	{
		TSnapshot snapshot = GetSnapshot(_ID);
		
		if (snapshot == null)
			return Task.CompletedTask;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		snapshot.Serialize(data);
		
		return m_Data.Child(_ID).SetValueAsync(data);
	}

	public Task UploadAsync()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (TSnapshot snapshot in Snapshots)
		{
			if (snapshot == null)
				continue;
			
			Dictionary<string, object> child = new Dictionary<string, object>();
			
			snapshot.Serialize(child);
			
			data[snapshot.ID] = child;
		}
		
		return m_Data.SetValueAsync(data);
	}

	public void MoveUp(string _ID)
	{
		int source = m_Snapshots.FindIndex(_Snapshot => _Snapshot.ID == _ID);
		
		if (source < 0)
			return;
		
		int target = UnityEngine.Mathf.Clamp(source - 1, 0, m_Snapshots.Count - 1);
		
		if (source == target)
			return;
		
		(m_Snapshots[source], m_Snapshots[target]) = (m_Snapshots[target], m_Snapshots[source]);
		
		Reorder();
	}

	public void MoveDown(string _ID)
	{
		int source = m_Snapshots.FindIndex(_Snapshot => _Snapshot.ID == _ID);
		
		if (source < 0)
			return;
		
		int target = UnityEngine.Mathf.Clamp(source + 1, 0, m_Snapshots.Count - 1);
		
		if (source == target)
			return;
		
		(m_Snapshots[source], m_Snapshots[target]) = (m_Snapshots[target], m_Snapshots[source]);
		
		Reorder();
	}

	public void Reorder()
	{
		int order = 0;
		foreach (TSnapshot snapshot in Snapshots)
			snapshot.Order = order++;
	}
	#else
	public TSnapshot GetSnapshot(string _ID)
	{
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _ID);
	}
	#endif

	protected abstract void FireSignal();

	protected int GetOrder(string _ID)
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