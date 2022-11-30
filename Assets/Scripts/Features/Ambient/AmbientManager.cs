using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;
using Random = UnityEngine.Random;

[Preserve]
public class AmbientManager : IDataManager
{
	public bool              Activated     { get; private set; }
	public AmbientCollection Collection => m_AmbientCollection;

	public bool Playing { get; private set; }

	public bool Paused => !Playing;

	[Inject] AmbientSource.Pool m_SourcePool;
	[Inject] AmbientCollection  m_AmbientCollection;
	[Inject] AudioClipProvider  m_AudioClipProvider;

	int m_AmbientIndex;

	readonly DataEventHandler m_PlayHandler  = new DataEventHandler();
	readonly DataEventHandler m_PauseHandler = new DataEventHandler();

	readonly List<string> m_Playlist = new List<string>();

	public async Task<bool> Activate()
	{
		if (Activated)
			return true;
		
		int frame = Time.frameCount;
		
		await Collection.Load();
		
		CreatePlaylist();
		
		Play();
		
		Activated = true;
		
		return frame == Time.frameCount;
	}

	public string GetTitle() => GetTitle(GetAmbientID());

	public string GetArtist() => GetArtist(GetAmbientID());

	public string GetTitle(string _AmbientID)
	{
		AmbientSnapshot snapshot = Collection.GetSnapshot(_AmbientID);
		
		return snapshot?.Title ?? string.Empty;
	}

	public string GetArtist(string _AmbientID)
	{
		AmbientSnapshot snapshot = Collection.GetSnapshot(_AmbientID);
		
		return snapshot?.Artist ?? string.Empty;
	}

	public void SubscribePlay(Action _Action) => m_PlayHandler.AddListener(_Action);

	public void UnsubscribePlay(Action _Action) => m_PlayHandler.RemoveListener(_Action);

	public void SubscribePause(Action _Action) => m_PauseHandler.AddListener(_Action);

	public void UnsubscribePause(Action _Action) => m_PauseHandler.RemoveListener(_Action);

	string GetAmbientID()
	{
		if (m_Playlist == null || m_Playlist.Count == 0)
		{
			Debug.LogError("---> PLAYLIST IS EMPTY");
			return null;
		}
		
		int index = MathUtility.Repeat(m_AmbientIndex, m_Playlist.Count);
		
		return m_Playlist[index];
	}

	void CreatePlaylist()
	{
		m_Playlist.Clear();
		
		List<string> ambientIDs = GetAmbientIDs();
		
		Shuffle(ambientIDs);
		
		m_Playlist.AddRange(ambientIDs);
	}

	List<string> GetAmbientIDs()
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.ToList();
	}

	bool IsActive(string _AmbientID)
	{
		AmbientSnapshot snapshot = Collection.GetSnapshot(_AmbientID);
		
		return snapshot?.Active ?? false;
	}

	string GetSound()
	{
		string ambientID = GetAmbientID();
		
		AmbientSnapshot snapshot = Collection.GetSnapshot(ambientID);
		
		return snapshot?.Sound ?? string.Empty;
	}

	float GetVolume()
	{
		string ambientID = GetAmbientID();
		
		AmbientSnapshot snapshot = Collection.GetSnapshot(ambientID);
		
		return snapshot?.Volume ?? 0;
	}

	public async void Pause()
	{
		Playing = false;
		
		m_PauseHandler.Invoke(GetAmbientID());
		
		List<Task> tasks = new List<Task>();
		foreach (AmbientSource source in m_AmbientSources)
			tasks.Add(source.PauseAsync());
		await Task.WhenAll(tasks);
	}

	readonly List<AmbientSource> m_AmbientSources = new List<AmbientSource>();

	public async void Play()
	{
		Playing = true;
		
		m_PlayHandler.Invoke(GetAmbientID());
		
		if (m_AmbientSources.Count == 0)
		{
			AudioClip clip = await LoadAsync();
			
			AmbientSource source = m_SourcePool.Spawn();
			
			source.Clip   = clip;
			source.Volume = GetVolume();
			
			m_AmbientSources.Add(source);
		}
		
		List<Task> tasks = new List<Task>();
		foreach (AmbientSource source in m_AmbientSources)
			tasks.Add(source.PlayAsync(Next));
		await Task.WhenAll(tasks);
	}

	public void Next()
	{
		Stop();
		
		if (m_Playlist == null || m_Playlist.Count == 0)
			return;
		
		m_AmbientIndex = MathUtility.Repeat(m_AmbientIndex + 1, m_Playlist.Count);
		
		Playing = true;
		
		m_PlayHandler.Invoke(GetAmbientID());
		
		Play();
	}

	public void Previous()
	{
		Stop();
		
		if (m_Playlist == null || m_Playlist.Count == 0)
			return;
		
		m_AmbientIndex = MathUtility.Repeat(m_AmbientIndex - 1, m_Playlist.Count);
		
		Playing = true;
		
		m_PlayHandler.Invoke(GetAmbientID());
		
		Play();
	}

	async void Stop()
	{
		if (m_AmbientSources == null || m_AmbientSources.Count == 0)
			return;
		
		List<AmbientSource> sources = new List<AmbientSource>(m_AmbientSources);
		
		m_AmbientSources.Clear();
		
		List<Task> tasks = new List<Task>();
		foreach (AmbientSource source in sources)
			tasks.Add(source.PauseAsync());
		await Task.WhenAll(tasks);
		
		foreach (AmbientSource source in sources)
			m_SourcePool.Despawn(source);
	}

	Task<AudioClip> LoadAsync(CancellationToken _Token = default)
	{
		string sound = GetSound();
		
		if (string.IsNullOrEmpty(sound))
			return Task.FromResult<AudioClip>(null);
		
		return m_AudioClipProvider.LoadAsync(sound, _Token);
	}

	static void Shuffle(IList<string> _AmbientIDs)
	{
		if (_AmbientIDs == null || _AmbientIDs.Count == 0)
			return;
		
		for (int i = 0; i < _AmbientIDs.Count; i++)
		{
			int j = Random.Range(i, _AmbientIDs.Count);
			
			(_AmbientIDs[i], _AmbientIDs[j]) = (_AmbientIDs[j], _AmbientIDs[i]);
		}
	}
}
