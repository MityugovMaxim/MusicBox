using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AudioChannel
{
	[Preserve]
	public class Factory : PlaceholderFactory<AudioChannelType, AudioChannelSettings, AudioChannel> { }


	public string TrackID => m_Playlist.GetID(Track);

	public string Title => m_Playlist.GetTitle(Track);

	public string Artist => m_Playlist.GetArtist(Track);

	AudioChannelType     Type     { get; }
	AudioChannelSettings Settings { get; }

	int Track
	{
		get => m_Track;
		set
		{
			if (m_Track == value)
				return;
			
			m_Track = value;
			
			OnTrackChanged?.Invoke();
		}
	}

	public AudioChannelState State
	{
		get => m_State;
		private set
		{
			if (m_State == value)
				return;
			
			m_State = value;
			
			OnStateChanged?.Invoke(m_State);
		}
	}

	public DynamicDelegate<AudioChannelState> OnStateChanged { get; } = new DynamicDelegate<AudioChannelState>();
	public DynamicDelegate                    OnTrackChanged { get; } = new DynamicDelegate();

	readonly AudioPlaylist m_Playlist = new AudioPlaylist();

	[Inject] AudioEntity.Pool  m_Pool;
	[Inject] AudioClipProvider m_Provider;

	AudioChannelState m_State;
	int               m_Track;
	AudioEntity       m_Item;

	public AudioChannel(AudioChannelType _Type, AudioChannelSettings _Settings)
	{
		Type     = _Type;
		Settings = _Settings;
	}

	public void RemoveRange(params AudioTrack[] _Items)
	{
		if (_Items == null || _Items.Length == 0)
			return;
		
		foreach (AudioTrack item in _Items)
		{
			if (item != null)
				m_Playlist.Remove(item);
		}
	}

	public void AddRange(IEnumerable<AudioTrack> _Tracks)
	{
		if (_Tracks == null)
			return;
		
		foreach (AudioTrack item in _Tracks)
		{
			if (item != null)
				m_Playlist.Add(item);
		}
		
		if (Settings.Shuffle)
			m_Playlist.Shuffle();
	}

	public void Play(AudioTrack _Track)
	{
		TokenProvider.CancelToken(this, Type);
		
		m_Playlist.Clear();
		m_Playlist.Add(_Track);
		
		m_Track = 0;
		m_State = AudioChannelState.Stop;
		
		RemoveItem();
		
		Play();
		
		OnTrackChanged?.Invoke();
	}

	public async void Play()
	{
		if (State == AudioChannelState.Play || State == AudioChannelState.Loading)
			return;
		
		TokenProvider.CancelToken(this, Type);
		
		CancellationToken token = TokenProvider.CreateToken(this, Type);
		
		try
		{
			State = AudioChannelState.Loading;
			
			string sound = m_Playlist.GetSound(Track);
			
			AudioClip clip = await m_Provider.LoadAsync(sound, token);
			
			token.ThrowIfCancellationRequested();
			
			await Task.Delay(250, token);
			
			State = AudioChannelState.Play;
			
			if (clip == null)
			{
				Next();
				return;
			}
			
			CreateItem(clip);
			
			await m_Item.PlayAsync(token);
			
			Next();
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		finally
		{
			TokenProvider.RemoveToken(this, Type);
		}
	}

	public async void Pause()
	{
		if (State == AudioChannelState.Pause)
			return;
		
		State = AudioChannelState.Pause;
		
		TokenProvider.CancelToken(this, Type);
		
		if (m_Item == null)
			return;
		
		CancellationToken token = TokenProvider.CreateToken(this, Type);
		
		try
		{
			await m_Item.PauseAsync(token);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		finally
		{
			TokenProvider.RemoveToken(this, Type);
		}
	}

	public void Stop()
	{
		if (State == AudioChannelState.Stop)
			return;
		
		TokenProvider.CancelToken(this, Type);
		
		State = AudioChannelState.Stop;
		
		RemoveItem();
	}

	void CreateItem(AudioClip _Clip)
	{
		if (m_Item != null || _Clip == null)
			return;
		
		m_Item      = m_Pool.Spawn(_Clip);
		m_Item.Loop = Settings.Loop;
	}

	async void RemoveItem()
	{
		if (m_Item == null)
			return;
		
		AudioEntity item = m_Item;
		
		m_Item = null;
		
		await item.StopAsync();
		
		m_Pool.Despawn(item);
	}

	public void Next()
	{
		TokenProvider.CancelToken(this, Type);
		
		Track++;
		
		if (!Settings.Repeat && !Settings.Loop && Track >= m_Playlist.Length)
		{
			Track = m_Playlist.Length - 1;
			State = AudioChannelState.Stop;
			
			RemoveItem();
			
			return;
		}
		
		if (State == AudioChannelState.Pause && m_Item != null)
			RemoveItem();
		
		if (State != AudioChannelState.Play && State != AudioChannelState.Loading)
			return;
		
		m_State = AudioChannelState.Stop;
		
		RemoveItem();
		
		Play();
	}

	public void Previous()
	{
		TokenProvider.CancelToken(this, Type);
		
		Track = State == AudioChannelState.Play && m_Item != null && m_Item.Time > 3000 ? Track : Track - 1;
		
		if (!Settings.Repeat && !Settings.Loop && Track <= 0)
		{
			Track = 0;
			State = AudioChannelState.Stop;
			
			RemoveItem();
			
			return;
		}
		
		if (State == AudioChannelState.Pause && m_Item != null)
			RemoveItem();
		
		if (State != AudioChannelState.Play && State != AudioChannelState.Loading)
			return;
		
		m_State = AudioChannelState.Stop;
		
		RemoveItem();
		
		Play();
	}
}
