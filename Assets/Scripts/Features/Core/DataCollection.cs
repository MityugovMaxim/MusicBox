using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine.Scripting;

[Preserve]
public abstract class DataCollection<TSnapshot> where TSnapshot : Snapshot
{
	public IReadOnlyList<TSnapshot> Snapshots => m_Snapshots;

	protected abstract string Path { get; }

	public bool Loaded { get; private set; }

	DatabaseReference m_Data;

	readonly List<TSnapshot>               m_Snapshots      = new List<TSnapshot>();
	readonly List<string>                  m_IDs            = new List<string>();
	readonly Dictionary<string, TSnapshot> m_Registry       = new Dictionary<string, TSnapshot>();

	readonly DataEventHandler[] m_Handlers =
	{
		new DataEventHandler(DataEventType.Add),
		new DataEventHandler(DataEventType.Remove),
		new DataEventHandler(DataEventType.Change),
		new DataEventHandler(DataEventType.Reorder),
	};

	Task m_Loading;

	public async Task Load()
	{
		if (Loaded)
			return;
		
		if (m_Loading != null)
		{
			await m_Loading;
			return;
		}
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_Loading = completionSource.Task;
		
		if (m_Data == null)
		{
			FirebaseDatabase database = DevelopmentMode.Enabled
				? FirebaseDatabase.GetInstance("https://audiobox-76b0e-dev.firebaseio.com/")
				: FirebaseDatabase.DefaultInstance;
			
			m_Data              =  database.RootReference.Child(Path);
			m_Data.ChildAdded   += OnSnapshotAdd;
			m_Data.ChildRemoved += OnSnapshotRemove;
			m_Data.ChildChanged += OnSnapshotChange;
			m_Data.ChildMoved   += OnSnapshotReorder;
		}
		
		Loaded = await Fetch();
		
		if (Loaded)
			await OnLoad();
		
		completionSource.TrySetResult(true);
		
		m_Loading = null;
	}

	public Task Reload()
	{
		Unload();
		
		return Load();
	}

	public void Unload()
	{
		if (!Loaded)
			return;
		
		if (m_Data != null)
		{
			m_Data.ChildAdded   -= OnSnapshotAdd;
			m_Data.ChildRemoved -= OnSnapshotRemove;
			m_Data.ChildChanged -= OnSnapshotChange;
			m_Data.ChildMoved   -= OnSnapshotReorder;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	public void Subscribe(DataEventType _EventType, Action _Action) => GetHandler(_EventType)?.AddListener(_Action);

	public void Subscribe(DataEventType _EventType, Action<string> _Action) => GetHandler(_EventType)?.AddListener(_Action);

	public void Subscribe(DataEventType _EventType, string _ID, Action _Action) => GetHandler(_EventType)?.AddListener(_ID, _Action);

	public void Subscribe(DataEventType _EventType, string _ID, Action<string> _Action) => GetHandler(_EventType)?.AddListener(_ID, _Action);

	public void Unsubscribe(DataEventType _EventType, Action _Action) => GetHandler(_EventType)?.RemoveListener(_Action);

	public void Unsubscribe(DataEventType _EventType, Action<string> _Action) => GetHandler(_EventType)?.RemoveListener(_Action);

	public void Unsubscribe(DataEventType _EventType, string _ID, Action _Action) => GetHandler(_EventType)?.RemoveListener(_ID, _Action);

	public void Unsubscribe(DataEventType _EventType, string _ID, Action<string> _Action) => GetHandler(_EventType)?.RemoveListener(_ID, _Action);

	void InvokeHandler(DataEventType _EventType, string _ID) => GetHandler(_EventType)?.Invoke(_ID);

	DataEventHandler GetHandler(DataEventType _EventType) => m_Handlers.FirstOrDefault(_Handler => _Handler.Type == _EventType);

	protected virtual void OnSnapshotAdd(string _ID) { }

	protected virtual void OnSnapshotRemove(string _ID) { }

	protected virtual void OnSnapshotChange(string _ID) { }

	protected virtual void OnSnapshotReorder(string _ID) { }

	async Task<bool> Fetch()
	{
		m_Snapshots.Clear();
		m_Registry.Clear();
		
		DataSnapshot dataSnapshot = await Filter(m_Data).GetValueAsync(15000, 2);
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch data failed.");
			return false;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(Create));
		
		foreach (TSnapshot snapshot in m_Snapshots)
		{
			m_IDs.Add(snapshot.ID);
			m_Registry[snapshot.ID] = snapshot;
		}
		
		return true;
	}

	void OnSnapshotAdd(object _Sender, ChildChangedEventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (_Args == null)
		{
			Log.Error(this, "Snapshot add failed. Invalid arguments");
			return;
		}
		
		if (_Args.DatabaseError != null)
		{
			Log.Error(this, "Snapshot add failed. {0}.", _Args.DatabaseError.Message);
			return;
		}
		
		DataSnapshot data = _Args.Snapshot;
		
		if (data == null)
		{
			Log.Error(this, "Snapshot add failed. Invalid data.");
			return;
		}
		
		TSnapshot snapshot = Create(data);
		
		if (snapshot == null)
		{
			Log.Error(this, "Snapshot add failed. Invalid snapshot.");
			return;
		}
		
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == snapshot.ID);
		m_Snapshots.Add(snapshot);
		
		Sort();
		
		m_Registry[snapshot.ID] = snapshot;
		
		OnSnapshotAdd(snapshot.ID);
		
		InvokeHandler(DataEventType.Add, snapshot.ID);
	}

	void OnSnapshotRemove(object _Sender, ChildChangedEventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (_Args == null)
		{
			Log.Error(this, "Snapshot remove failed. Invalid arguments");
			return;
		}
		
		if (_Args.DatabaseError != null)
		{
			Log.Error(this, "Snapshot remove failed. {0}.", _Args.DatabaseError.Message);
			return;
		}
		
		DataSnapshot data = _Args.Snapshot;
		
		if (data == null)
		{
			Log.Error(this, "Snapshot remove failed. Invalid data.");
			return;
		}
		
		TSnapshot snapshot = Create(data);
		
		if (snapshot == null)
		{
			Log.Error(this, "Snapshot remove failed. Invalid snapshot.");
			return;
		}
		
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == snapshot.ID);
		
		Sort();
		
		if (m_Registry.ContainsKey(snapshot.ID))
			m_Registry.Remove(snapshot.ID);
		
		OnSnapshotRemove(snapshot.ID);
		
		InvokeHandler(DataEventType.Remove, snapshot.ID);
	}

	void OnSnapshotChange(object _Sender, ChildChangedEventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (_Args == null)
		{
			Log.Error(this, "Snapshot change failed. Invalid arguments");
			return;
		}
		
		if (_Args.DatabaseError != null)
		{
			Log.Error(this, "Snapshot change failed. {0}.", _Args.DatabaseError.Message);
			return;
		}
		
		DataSnapshot data = _Args.Snapshot;
		
		if (data == null)
		{
			Log.Error(this, "Snapshot change failed. Invalid data.");
			return;
		}
		
		TSnapshot snapshot = Create(data);
		
		if (snapshot == null)
		{
			Log.Error(this, "Snapshot change failed. Invalid snapshot.");
			return;
		}
		
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == snapshot.ID);
		m_Snapshots.Add(snapshot);
		
		Sort();
		
		if (m_Registry.ContainsKey(snapshot.ID))
			m_Registry.Remove(snapshot.ID);
		
		OnSnapshotChange(snapshot.ID);
		
		InvokeHandler(DataEventType.Change, snapshot.ID);
	}

	void OnSnapshotReorder(object _Sender, ChildChangedEventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (_Args == null)
		{
			Log.Error(this, "Snapshot reorder failed. Invalid arguments");
			return;
		}
		
		if (_Args.DatabaseError != null)
		{
			Log.Error(this, "Snapshot reorder failed. {0}.", _Args.DatabaseError.Message);
			return;
		}
		
		DataSnapshot data = _Args.Snapshot;
		
		if (data == null)
		{
			Log.Error(this, "Snapshot reorder failed. Invalid data.");
			return;
		}
		
		TSnapshot snapshot = Create(data);
		
		if (snapshot == null)
		{
			Log.Error(this, "Snapshot reorder failed. Invalid snapshot.");
			return;
		}
		
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == snapshot.ID);
		m_Snapshots.Add(snapshot);
		
		Sort();
		
		m_Registry[snapshot.ID] = snapshot;
		
		OnSnapshotReorder(snapshot.ID);
		
		InvokeHandler(DataEventType.Reorder, snapshot.ID);
	}

	public bool Contains(string _ID)
	{
		return !string.IsNullOrEmpty(_ID) && m_Registry.ContainsKey(_ID);
	}

	public bool Contains(Func<TSnapshot, bool> _Predicate)
	{
		return _Predicate != null && m_Snapshots.Where(_Snapshot => _Snapshot != null).Any(_Predicate);
	}

	protected virtual Task OnLoad() => Task.CompletedTask;

	protected virtual Query Filter(DatabaseReference _Data)
	{
		return _Data.OrderByChild("order");
	}

	protected virtual TSnapshot Create(DataSnapshot _Data)
	{
		return Activator.CreateInstance(typeof(TSnapshot), _Data) as TSnapshot;
	}

	protected virtual void Sort()
	{
		m_Snapshots.Sort((_A, _B) => _A.Order.CompareTo(_B.Order));
		
		m_IDs.Clear();
		
		m_IDs.AddRange(m_Snapshots.Select(_Snapshot => _Snapshot.ID));
	}

	public IReadOnlyList<string> GetIDs() => m_IDs;

	public TSnapshot GetSnapshot(string _ID)
	{
		if (string.IsNullOrEmpty(_ID))
			return null;
		
		if (m_Registry.TryGetValue(_ID, out TSnapshot snapshot) && snapshot != null)
			return snapshot;
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _ID);
	}

	public TSnapshot GetSnapshot(Func<TSnapshot, bool> _Predicate)
	{
		return _Predicate != null ? m_Snapshots.Where(_Snapshot => _Snapshot != null).FirstOrDefault(_Predicate) : null;
	}

	public int GetOrder(string _ID)
	{
		TSnapshot snapshot = GetSnapshot(_ID);
		
		return snapshot?.Order ?? 0;
	}
}
