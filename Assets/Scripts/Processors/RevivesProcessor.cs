using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class ReviveSnapshot
{
	public string ID     { get; }
	public bool   Active { get; }
	public int    Count  { get; }
	public long   Coins  { get; }
	public int    Order  { get; }

	public ReviveSnapshot(DataSnapshot _Data)
	{
		ID     = _Data.Key;
		Active = _Data.GetBool("active");
		Count  = _Data.GetInt("count");
		Coins  = _Data.GetLong("coins");
		Order  = _Data.GetInt("order");
	}
}

[Preserve]
public class RevivesDataUpdateSignal { }

[Preserve]
public class RevivesProcessor
{
	bool Loaded { get; set; }

	[Inject] SignalBus m_SignalBus;

	DatabaseReference m_Data;

	readonly List<ReviveSnapshot> m_Snapshots = new List<ReviveSnapshot>();

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("revives");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public long GetCoins(int _Count)
	{
		if (m_Snapshots.Count == 0)
			return 0;
		
		int minCount = m_Snapshots.Min(_Snapshot => _Snapshot.Count);
		int maxCount = m_Snapshots.Max(_Snapshot => _Snapshot.Count);
		int count    = Mathf.Clamp(_Count, minCount, maxCount);
		
		ReviveSnapshot snapshot = m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Where(_Snapshot => _Snapshot.Count >= count)
			.Aggregate((_A, _B) => _A.Coins <= _B.Coins ? _A : _B);
		
		return snapshot?.Coins ?? 0;
	}

	async void OnUpdate(object _Sender, ValueChangedEventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[RevivesProcessor] Updating revives data...");
		
		await Fetch();
		
		Debug.Log("[RevivesProcessor] Update revives data complete.");
		
		m_SignalBus.Fire<RevivesDataUpdateSignal>();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync(15000, 4);
		
		if (dataSnapshot == null)
		{
			Debug.LogError("[RevivesProcessor] Fetch revives failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new ReviveSnapshot(_Data)));
	}
}