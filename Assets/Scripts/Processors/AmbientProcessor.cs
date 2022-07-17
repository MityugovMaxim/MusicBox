using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class AmbientSnapshot : Snapshot
{
	public bool   Active { get; }
	public string Sound  { get; }
	public float  Volume { get; }

	public AmbientSnapshot() : base("new_ambient", 0)
	{
		Active = false;
		Sound  = "Ambient/new_ambient.ogg";
		Volume = 0.5f;
	}

	public AmbientSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active = _Data.GetBool("active");
		Sound  = _Data.GetString("sound", $"Ambient/{ID}.ogg");
		Volume = _Data.GetFloat("volume");
	}
}

public class AmbientProcessor : MonoBehaviour
{
	const float PLAY_FADE_DURATION   = 1.0f;
	const float PAUSE_FADE_DURATION  = 1.0f;
	const float RESUME_FADE_DURATION = 1.0f;

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
		
		await FetchAmbient();
		
		Loaded = true;
		
		ProcessAmbient();
	}

	public List<string> GetAmbientIDs()
	{
		return m_Snapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public float GetVolume(string _AmbientID)
	{
		AmbientSnapshot snapshot = GetSnapshot(_AmbientID);
		
		return snapshot?.Volume ?? 0;
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

	async Task PauseAsync()
	{
		if (m_Locked)
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Paused = true;
		
		try
		{
			await m_AudioSource.SetVolumeAsync(0, PAUSE_FADE_DURATION, token);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		if (token.IsCancellationRequested)
			return;
		
		m_AudioSource.Pause();
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	async Task ResumeAsync()
	{
		if (m_Locked)
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Paused = false;
		m_AudioSource.UnPause();
		
		try
		{
			await m_AudioSource.SetVolumeAsync(GetVolume(m_AmbientID), RESUME_FADE_DURATION, token);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
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
		
		try
		{
			AudioClip audioClip = await m_StorageProcessor.LoadAudioClipAsync($"Ambient/{_AmbientID}.ogg", null, token);
			
			if (token.IsCancellationRequested)
				return;
			
			m_AudioSource.Stop();
			m_AudioSource.volume = 0;
			m_AudioSource.clip   = audioClip;
			
			if (audioClip != null)
			{
				m_AudioSource.Play();
				
				float volume = GetVolume(_AmbientID);
				
				await m_AudioSource.SetVolumeAsync(volume, PLAY_FADE_DURATION, token);
			}
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
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
			
			AmbientSnapshot snapshot = GetSnapshot(ambientID);
			
			if (snapshot == null)
			{
				await Task.Delay(250);
				continue;
			}
			
			await PlayAsync(snapshot.ID);
			
			await UnityTask.While(() => m_AudioSource.isPlaying || m_Paused);
		}
	}

	void Unload()
	{
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnUpdate;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	async void OnUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		if (FirebaseAuth.DefaultInstance.CurrentUser == null)
		{
			Unload();
			return;
		}
		
		Log.Info(this, "Updating ambient data...");
		
		await FetchAmbient();
		
		Log.Info(this, "Update ambient data complete.");
		
		ProcessAmbient();
	}

	async Task FetchAmbient()
	{
		m_Snapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_Data.GetValueAsync();
		
		foreach (DataSnapshot ambientSnapshot in dataSnapshot.Children)
		{
			AmbientSnapshot ambient = new AmbientSnapshot(ambientSnapshot);
			if (ambient.Active)
				m_Snapshots.Add(ambient);
		}
		
		for (int i = 0; i < m_Snapshots.Count; i++)
		{
			int j = Random.Range(i, m_Snapshots.Count);
			
			(m_Snapshots[i], m_Snapshots[j]) = (m_Snapshots[j], m_Snapshots[i]);
		}
	}

	AmbientSnapshot GetSnapshot(string _AmbientID)
	{
		if (string.IsNullOrEmpty(_AmbientID))
		{
			Log.Error(this, "Get ambient snapshot failed. Ambient ID is null or empty.");
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