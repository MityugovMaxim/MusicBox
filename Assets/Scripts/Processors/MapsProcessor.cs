using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class MapsProcessor
{
	bool Loaded { get; set; }

	readonly Dictionary<string, SongSnapshot>       m_Snapshots = new Dictionary<string, SongSnapshot>();
	readonly Dictionary<string, Task<SongSnapshot>> m_Fetch     = new Dictionary<string, Task<SongSnapshot>>();

	[Inject] ProfileProcessor m_ProfileProcessor;

	public async Task Load()
	{
		if (Loaded)
			return;
		
		Loaded = true;
		
		IReadOnlyList<string> playlist = m_ProfileProcessor.GetPlaylist();
		
		List<Task> tasks = new List<Task>();
		foreach (string mapID in playlist)
			tasks.Add(FetchSnapshot(mapID));
		
		await Task.WhenAll(tasks);
	}

	public SongSnapshot GetSnapshot(string _SongID)
	{
		return m_Snapshots.TryGetValue(_SongID, out SongSnapshot snapshot) ? snapshot : null;
	}

	async Task<SongSnapshot> FetchSnapshot(string _SongID)
	{
		if (m_Snapshots.TryGetValue(_SongID, out SongSnapshot snapshot) && snapshot != null)
			return snapshot;
		
		if (m_Fetch.TryGetValue(_SongID, out Task<SongSnapshot> task) && task != null)
			return await task;
		
		TaskCompletionSource<SongSnapshot> completionSource = new TaskCompletionSource<SongSnapshot>();
		
		m_Fetch[_SongID] = completionSource.Task;
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference
			.Child("maps")
			.Child(_SongID);
		
		DataSnapshot data = await reference.GetValueAsync();
		
		snapshot = new SongSnapshot(data);
		
		m_Snapshots[_SongID] = snapshot;
		
		completionSource.TrySetResult(snapshot);
		
		m_Fetch.Remove(_SongID);
		
		return snapshot;
	}
}