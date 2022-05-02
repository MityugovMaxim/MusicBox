using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class AmbientSnapshot
{
	public string ID     { get; set; }
	public bool   Active { get; set; }
	public float  Volume { get; set; }
	[HideProperty]
	public int    Order  { get; set; }

	public AmbientSnapshot(string _ID)
	{
		ID     = _ID;
		Volume = 0.5f;
	}

	public AmbientSnapshot(DataSnapshot _Data)
	{
		ID     = _Data.Key;
		Active = _Data.GetBool("active");
		Volume = _Data.GetFloat("volume");
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		data["active"] = Active;
		data["volume"] = Volume;
		data["order"]  = Order;
		
		return data;
	}
}

public class AmbientProcessor : MonoBehaviour
{
	const float PLAY_FADE_DURATION   = 0.5f;
	const float PAUSE_FADE_DURATION  = 0.5f;
	const float RESUME_FADE_DURATION = 0.5f;

	bool Loaded { get; set; }

	readonly List<AmbientSnapshot> m_Snapshots = new List<AmbientSnapshot>();

	SignalBus        m_SignalBus;
	StorageProcessor m_StorageProcessor;

	DatabaseReference m_Data;

	string                  m_AmbientID;
	bool                    m_Processing;
	bool                    m_Paused;
	bool                    m_Locked;
	AudioSource             m_AudioSource;
	CancellationTokenSource m_TokenSource;

	[Inject]
	public void Construct(SignalBus _SignalBus, StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
		
		m_AudioSource             = gameObject.AddComponent<AudioSource>();
		m_AudioSource.loop        = false;
		m_AudioSource.playOnAwake = false;
		m_AudioSource.volume      = 0;
	}

	public async Task Load()
	{
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child("ambient");
			m_Data.ValueChanged += OnUpdate;
		}
		
		await Fetch();
		
		Loaded = true;
		
		ProcessAmbient();
	}

	public List<string> GetAmbientIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public float GetVolume(string _AmbientID)
	{
		AmbientSnapshot ambientSnapshot = GetSnapshot(_AmbientID);
		
		if (ambientSnapshot == null)
		{
			Debug.LogErrorFormat("[AmbientProcessor] Get volume failed. Ambient with ID '{0}' is null.", _AmbientID);
			return 0;
		}
		
		return ambientSnapshot.Volume;
	}

	public async void Pause()
	{
		await PauseAsync();
	}

	public async void Resume()
	{
		await ResumeAsync();
	}

	public void Lock()
	{
		m_Locked = true;
	}

	public void Unlock()
	{
		m_Locked = false;
	}

	public async Task PauseAsync()
	{
		if (m_Locked)
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Paused = true;
		
		await m_AudioSource.SetVolumeAsync(0, PAUSE_FADE_DURATION, token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_AudioSource.Pause();
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public async Task ResumeAsync()
	{
		if (m_Locked)
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Paused = false;
		m_AudioSource.UnPause();
		
		await m_AudioSource.SetVolumeAsync(GetVolume(m_AmbientID), RESUME_FADE_DURATION, token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	async Task PlayAsync(string _AmbientID)
	{
		m_AmbientID = _AmbientID;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		AudioClip audioClip = await m_StorageProcessor.LoadAudioClipAsync($"Ambient/{_AmbientID}.ogg", token);
		
		if (audioClip == null || token.IsCancellationRequested)
			return;
		
		m_AudioSource.Stop();
		m_AudioSource.volume = 0;
		m_AudioSource.clip   = audioClip;
		m_AudioSource.Play();
		
		await m_AudioSource.SetVolumeAsync(GetVolume(_AmbientID), PLAY_FADE_DURATION, token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	async void ProcessAmbient()
	{
		if (m_AudioSource == null || m_AudioSource.isPlaying || m_Paused)
			return;
		
		while (m_Snapshots != null && m_Snapshots.Count > 0)
		{
			string ambientID = GetAmbientID(m_AmbientID, 1);
			
			AmbientSnapshot ambientSnapshot = GetSnapshot(ambientID);
			
			if (ambientSnapshot == null)
			{
				await Task.Delay(250);
				continue;
			}
			
			await PlayAsync(ambientSnapshot.ID);
			
			await UnityTask.While(() => m_AudioSource.isPlaying || m_Paused);
		}
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[MusicProcessor] Updating ambient data...");
		
		await Fetch();
		
		Debug.Log("[MusicProcessor] Update ambient data complete.");
		
		ProcessAmbient();
	}

	async Task Fetch()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.OrderByChild("order").GetValueAsync();
		
		if (dataSnapshot == null)
		{
			Log.Error(this, "Fetch ambient failed.");
			return;
		}
		
		m_Snapshots.AddRange(dataSnapshot.Children.Select(_Data => new AmbientSnapshot(_Data)));
		
		for (int i = 0; i < m_Snapshots.Count; i++)
		{
			int j = Random.Range(i, m_Snapshots.Count);
			
			(m_Snapshots[i], m_Snapshots[j]) = (m_Snapshots[j], m_Snapshots[i]);
		}
	}

	public async Task Upload()
	{
		Loaded = false;
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (AmbientSnapshot snapshot in m_Snapshots)
		{
			if (snapshot != null)
				data[snapshot.ID] = snapshot.Serialize();
		}
		
		await m_Data.SetValueAsync(data);
		
		await Fetch();
		
		Loaded = true;
	}

	public async Task Upload(params string[] _AmbientIDs)
	{
		if (_AmbientIDs == null || _AmbientIDs.Length == 0)
			return;
		
		Loaded = false;
		
		foreach (string ambientID in _AmbientIDs)
		{
			AmbientSnapshot snapshot = GetSnapshot(ambientID);
			
			Dictionary<string, object> data = snapshot?.Serialize();
			
			await m_Data.Child(ambientID).SetValueAsync(data);
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public void MoveSnapshot(string _AmbientID, int _Offset)
	{
		int sourceIndex = m_Snapshots.FindIndex(_Snapshot => _Snapshot.ID == _AmbientID);
		int targetIndex = sourceIndex + _Offset;
		
		if (sourceIndex < 0 || sourceIndex >= m_Snapshots.Count || targetIndex < 0 || targetIndex >= m_Snapshots.Count)
			return;
		
		(m_Snapshots[sourceIndex], m_Snapshots[targetIndex]) = (m_Snapshots[targetIndex], m_Snapshots[sourceIndex]);
		
		for (int i = 0; i < m_Snapshots.Count; i++)
			m_Snapshots[i].Order = i;
	}

	public AmbientSnapshot CreateSnapshot(string _AmbientID)
	{
		string ambientID = _AmbientID.ToUnique('_', GetAmbientIDs());
		
		AmbientSnapshot snapshot = new AmbientSnapshot(ambientID);
		
		m_Snapshots.Insert(0, snapshot);
		
		return snapshot;
	}

	public void RemoveSnapshot(string _AmbientID)
	{
		m_Snapshots.RemoveAll(_Snapshot => _Snapshot.ID == _AmbientID);
	}

	public AmbientSnapshot GetSnapshot(string _AmbientID)
	{
		if (string.IsNullOrEmpty(_AmbientID))
		{
			Debug.LogError("[AmbientProcessor] Get ambient snapshot failed. Ambient ID is null or empty.");
			return null;
		}
		
		return m_Snapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _AmbientID);
	}

	string GetAmbientID(string _AmbientID, int _Offset)
	{
		List<string> ambientIDs = GetAmbientIDs();
		
		if (string.IsNullOrEmpty(_AmbientID))
			return ambientIDs.Count > 0 ? ambientIDs[0] : string.Empty;
		
		if (ambientIDs == null || ambientIDs.Count == 0)
			return _AmbientID;
		
		int index = ambientIDs.IndexOf(_AmbientID);
		
		index = MathUtility.Repeat(index + _Offset, ambientIDs.Count);
		
		return ambientIDs[index];
	}
}