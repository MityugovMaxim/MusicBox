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
	public string ID     { get; }
	public bool   Active { get; }
	public float  Volume { get; }

	public AmbientSnapshot(DataSnapshot _Data)
	{
		ID     = _Data.Key;
		Active = _Data.GetBool("active");
		Volume = _Data.GetFloat("volume");
	}
}

public class AmbientProcessor : MonoBehaviour
{
	const float PLAY_FADE_DURATION   = 1.0f;
	const float PAUSE_FADE_DURATION  = 1.0f;
	const float RESUME_FADE_DURATION = 1.0f;

	bool Loaded { get; set; }

	readonly List<AmbientSnapshot> m_AmbientSnapshots = new List<AmbientSnapshot>();

	SignalBus        m_SignalBus;
	StorageProcessor m_StorageProcessor;

	DatabaseReference m_AmbientData;

	string                  m_AmbientID;
	bool                    m_Processing;
	bool                    m_Paused;
	bool                    m_Locked;
	AudioSource             m_AudioSource;
	CancellationTokenSource m_TokenSource;

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			Pause();
		}
		if (Input.GetKeyDown(KeyCode.H))
		{
			Resume();
		}
	}

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
		if (m_AmbientData == null)
		{
			m_AmbientData              =  FirebaseDatabase.DefaultInstance.RootReference.Child("ambient");
			m_AmbientData.ValueChanged += OnAmbientUpdate;
		}
		
		await FetchAmbient();
		
		Loaded = true;
		
		ProcessAmbient();
	}

	public List<string> GetAmbientIDs()
	{
		return m_AmbientSnapshots
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public float GetVolume(string _AmbientID)
	{
		AmbientSnapshot ambientSnapshot = GetAmbientSnapshot(_AmbientID);
		
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
		
		AudioClip audioClip = await m_StorageProcessor.LoadAudioClipAsync($"Ambient/{_AmbientID}.ogg", token);
		
		if (audioClip == null || token.IsCancellationRequested)
			return;
		
		m_AudioSource.Stop();
		m_AudioSource.volume = 0;
		m_AudioSource.clip   = audioClip;
		m_AudioSource.Play();
		
		try
		{
			await m_AudioSource.SetVolumeAsync(GetVolume(_AmbientID), PLAY_FADE_DURATION, token);
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
		
		while (m_AmbientSnapshots != null && m_AmbientSnapshots.Count > 0)
		{
			string ambientID = GetAmbientID(m_AmbientID, 1);
			
			AmbientSnapshot ambientSnapshot = GetAmbientSnapshot(ambientID);
			
			if (ambientSnapshot == null)
			{
				await Task.Delay(250);
				continue;
			}
			
			await PlayAsync(ambientSnapshot.ID);
			
			await UnityTask.While(() => m_AudioSource.isPlaying || m_Paused);
		}
	}

	async void OnAmbientUpdate(object _Sender, EventArgs _Args)
	{
		if (!Loaded)
			return;
		
		Debug.Log("[MusicProcessor] Updating ambient data...");
		
		await FetchAmbient();
		
		Debug.Log("[MusicProcessor] Update ambient data complete.");
		
		ProcessAmbient();
	}

	async Task FetchAmbient()
	{
		m_AmbientSnapshots.Clear();
		
		DataSnapshot dataSnapshot = await m_AmbientData.GetValueAsync();
		
		foreach (DataSnapshot ambientSnapshot in dataSnapshot.Children)
		{
			AmbientSnapshot ambient = new AmbientSnapshot(ambientSnapshot);
			if (ambient.Active)
				m_AmbientSnapshots.Add(ambient);
		}
		
		for (int i = 0; i < m_AmbientSnapshots.Count; i++)
		{
			int j = Random.Range(i, m_AmbientSnapshots.Count);
			
			(m_AmbientSnapshots[i], m_AmbientSnapshots[j]) = (m_AmbientSnapshots[j], m_AmbientSnapshots[i]);
		}
	}

	AmbientSnapshot GetAmbientSnapshot(string _AmbientID)
	{
		if (string.IsNullOrEmpty(_AmbientID))
		{
			Debug.LogError("[AmbientProcessor] Get ambient snapshot failed. Ambient ID is null or empty.");
			return null;
		}
		
		return m_AmbientSnapshots.FirstOrDefault(_Snapshot => _Snapshot.ID == _AmbientID);
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