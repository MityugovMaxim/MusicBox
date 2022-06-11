using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class SoundProcessor
{
	readonly SoundSource.Pool                m_SourcePool;
	readonly Dictionary<string, SoundInfo>   m_SoundInfos;
	readonly Dictionary<string, AudioClip>   m_SoundCache;
	readonly Dictionary<string, SoundSource> m_SoundSources;
	readonly Dictionary<string, SoundSource> m_PersistentSources;

	readonly Dictionary<string, CancellationTokenSource> m_TokenSources = new Dictionary<string, CancellationTokenSource>();

	[Inject]
	public SoundProcessor(SoundSource.Pool _SourcePool, SoundInfo[] _SoundInfos)
	{
		m_SourcePool    = _SourcePool;
		m_SoundInfos    = _SoundInfos.ToDictionary(_SoundInfo => _SoundInfo.ID, _SoundInfo => _SoundInfo);
		m_SoundCache    = new Dictionary<string, AudioClip>();
		m_SoundSources  = new Dictionary<string, SoundSource>();
		m_PersistentSources = new Dictionary<string, SoundSource>();
	}

	public void Start(string _SoundID)
	{
		Stop(_SoundID);
		
		AudioClip sound = GetSound(_SoundID);
		
		if (sound == null)
		{
			Log.Error(this, "Start failed. Sound is null.");
			return;
		}
		
		SoundSource source = m_SourcePool.Spawn();
		
		if (source == null)
		{
			Log.Error(this, "Start failed. Source is null.");
			return;
		}
		
		m_SoundSources[_SoundID] = source;
		
		source.StartSound(sound);
	}

	public void Stop(string _SoundID)
	{
		if (!m_SoundSources.ContainsKey(_SoundID))
			return;
		
		SoundSource source = m_SoundSources[_SoundID];
		
		m_SoundSources.Remove(_SoundID);
		
		if (source == null)
		{
			Log.Error(this, "Stop failed. Source is null.");
			return;
		}
		
		source.StopSound();
	}

	public async void Play(string _SoundID)
	{
		await PlayAsync(_SoundID);
	}

	public async Task PlayAsync(string _SoundID)
	{
		AudioClip sound = GetSound(_SoundID);
		
		if (sound == null)
			return;
		
		float pitch      = GetPitch(_SoundID);
		float volume     = GetVolume(_SoundID);
		bool  persistent = IsPersistent(_SoundID);
		
		if (persistent)
		{
			if (m_TokenSources.TryGetValue(_SoundID, out CancellationTokenSource tokenSource))
			{
				tokenSource?.Cancel();
				tokenSource?.Dispose();
				m_TokenSources.Remove(_SoundID);
			}
			
			if (!m_PersistentSources.TryGetValue(_SoundID, out SoundSource source) || source == null)
			{
				source = m_SourcePool.Spawn();
				
				m_PersistentSources[_SoundID] = source;
			}
			
			tokenSource = new CancellationTokenSource();
			
			CancellationToken token = tokenSource.Token;
			
			m_TokenSources[_SoundID] = tokenSource;
			
			await source.Play(sound, pitch, volume);
			
			if (token.IsCancellationRequested)
				return;
			
			m_PersistentSources.Remove(_SoundID);
			
			m_SourcePool.Despawn(source);
			
			m_TokenSources.Remove(_SoundID);
			
			tokenSource.Dispose();
		}
		else
		{
			SoundSource source = m_SourcePool.Spawn();
			
			await source.Play(sound, pitch, volume);
			
			m_SourcePool.Despawn(source);
		}
	}

	public AudioClip GetSound(string _SoundID)
	{
		if (string.IsNullOrEmpty(_SoundID))
		{
			Debug.LogError("[SoundProcessor] Get sound failed. Sound ID is null or empty.");
			return null;
		}
		
		if (m_SoundCache.ContainsKey(_SoundID) && m_SoundCache[_SoundID] != null)
			return m_SoundCache[_SoundID];
		
		SoundInfo soundInfo = GetSoundInfo(_SoundID);
		
		if (soundInfo == null)
		{
			Debug.LogErrorFormat("[SoundProcessor] Get sound failed. Sound with ID '{0}' is null.", _SoundID);
			return null;
		}
		
		AudioClip sound = Resources.Load<AudioClip>(soundInfo.Path);
		
		if (sound == null)
			return null;
		
		m_SoundCache[_SoundID] = sound;
		
		return sound;
	}

	public float GetPitch(string _SoundID)
	{
		SoundInfo soundInfo = GetSoundInfo(_SoundID);
		
		return soundInfo != null ? soundInfo.Pitch : 1;
	}

	float GetVolume(string _SoundID)
	{
		SoundInfo soundInfo = GetSoundInfo(_SoundID);
		
		return soundInfo != null ? soundInfo.Volume : 1;
	}

	bool IsPersistent(string _SoundID)
	{
		SoundInfo soundInfo = GetSoundInfo(_SoundID);
		
		return soundInfo != null && soundInfo.Persistent;
	}

	SoundInfo GetSoundInfo(string _SoundID)
	{
		if (string.IsNullOrEmpty(_SoundID))
		{
			Debug.LogError("[SoundProcessor] Get sound failed. Sound ID is null or empty.");
			return null;
		}
		
		if (!m_SoundInfos.ContainsKey(_SoundID) || m_SoundInfos[_SoundID] == null)
		{
			Debug.LogErrorFormat("[SoundProcessor] Get sound failed. Sound '{0}' not found.", _SoundID);
			return null;
		}
		
		return m_SoundInfos[_SoundID];
	}
}