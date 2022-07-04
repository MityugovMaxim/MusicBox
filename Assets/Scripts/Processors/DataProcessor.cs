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
	public int    Order { get; }

	protected Snapshot(DataSnapshot _Data)
	{
		ID    = _Data.Key;
		Order = _Data.GetInt("order");
	}
}

[Preserve]
public abstract class DataProcessor<TSnapshot, TSignal> where TSnapshot : Snapshot
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
		
		m_SignalBus.Fire<TSignal>();
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

	protected TSnapshot GetSnapshot(string _ID)
	{
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _ID);
	}

	protected int GetOrder(string _ID)
	{
		TSnapshot snapshot = GetSnapshot(_ID);
		
		return snapshot?.Order ?? 0;
	}
}