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

	protected IReadOnlyList<TSnapshot>               Snapshots  => m_Snapshots;
	protected IReadOnlyDictionary<string, TSnapshot> Registry   => m_Registry;

	protected abstract bool SupportsDevelopment { get; }

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
			FirebaseDatabase database = SupportsDevelopment && DevelopmentMode.Enabled
				? FirebaseDatabase.GetInstance("https://audiobox-76b0e-dev.firebaseio.com/")
				: FirebaseDatabase.DefaultInstance;
			
			m_DataPath          =  Path;
			m_Data              =  database.RootReference.Child(Path);
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		if (!Loaded)
			await OnLoad();
		
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
		m_Registry.Clear();
		
		if (_EventArgs.Snapshot == null)
			return;
		
		m_Snapshots.AddRange(_EventArgs.Snapshot.Children.Select(Create));
		
		foreach (TSnapshot snapshot in m_Snapshots)
			m_Registry[snapshot.ID] = snapshot;
		
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

	public bool Contains(string _ID)
	{
		return m_Registry.ContainsKey(_ID);
	}

	protected virtual Task OnLoad()  => Task.CompletedTask;

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
