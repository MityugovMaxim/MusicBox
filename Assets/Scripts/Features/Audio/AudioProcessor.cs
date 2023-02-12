using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Logging;
using Zenject;

public enum AudioChannelType
{
	Ambient,
	Preview,
}

public class AudioProcessor
{
	[Inject] AudioChannel.Factory m_Factory;

	readonly Dictionary<AudioChannelType, AudioChannel> m_Channels = new Dictionary<AudioChannelType, AudioChannel>();

	public string GetID(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		return channel?.TrackID ?? string.Empty;
	}

	public string GetTitle(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		return channel?.Title ?? string.Empty;
	}

	public string GetArtist(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		return channel?.Artist ?? string.Empty;
	}

	public AudioChannelState GetState(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		return channel?.State ?? AudioChannelState.Stop;
	}

	public void SubscribeState(AudioChannelType _Type, Action _Action)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.OnStateChanged.AddListener(_Action);
	}

	public void SubscribeState(AudioChannelType _Type, Action<AudioChannelState> _Action)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.OnStateChanged.AddListener(_Action);
	}

	public void UnsubscribeState(AudioChannelType _Type, Action _Action)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.OnStateChanged.RemoveListener(_Action);
	}

	public void UnsubscribeState(AudioChannelType _Type, Action<AudioChannelState> _Action)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.OnStateChanged.RemoveListener(_Action);
	}

	public void SubscribeTrack(AudioChannelType _Type, Action _Action)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.OnTrackChanged.AddListener(_Action);
	}

	public void UnsubscribeTrack(AudioChannelType _Type, Action _Action)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.OnTrackChanged.RemoveListener(_Action);
	}

	public void RegisterChannel(AudioChannelType _Type, AudioChannelSettings _Settings, params AudioTrack[] _Tracks) => RegisterChannel(_Type, _Settings, _Tracks.AsEnumerable());

	public void RegisterChannel(AudioChannelType _Type, AudioChannelSettings _Settings, IEnumerable<AudioTrack> _Tracks)
	{
		if (m_Channels.ContainsKey(_Type))
		{
			Log.Error(this, "Register channel failed. Channel type '{0}' already registered.", _Type);
			return;
		}
		
		AudioChannel channel = m_Factory.Create(_Type, _Settings);
		
		channel.AddRange(_Tracks);
		
		m_Channels[_Type] = channel;
	}

	public void UnregisterChannel(AudioChannelType _Type)
	{
		if (!m_Channels.ContainsKey(_Type))
		{
			Log.Error(this, "Unregister channel failed. Channel type '{0}' not registered.", _Type);
			return;
		}
		
		AudioChannel channel = GetChannel(_Type);
		
		channel?.Stop();
		
		m_Channels.Remove(_Type);
	}

	public void Play(AudioChannelType _Type, AudioTrack _Track)
	{
		AudioChannel channel = GetChannel(_Type);
		
		if (channel == null)
			return;
		
		ProcessFocus(_Type);
		
		channel.Play(_Track);
	}

	public void Play(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		if (channel == null)
			return;
		
		ProcessFocus(_Type);
		
		channel.Play();
	}

	public void Stop(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		if (channel == null)
			return;
		
		RestoreFocus(_Type);
		
		channel.Stop();
	}

	public void Pause(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		if (channel == null)
			return;
		
		RestoreFocus(_Type);
		
		channel.Pause();
	}

	public void Next(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.Next();
	}

	public void Previous(AudioChannelType _Type)
	{
		AudioChannel channel = GetChannel(_Type);
		
		channel?.Previous();
	}

	AudioChannel GetChannel(AudioChannelType _Type)
	{
		if (!m_Channels.TryGetValue(_Type, out AudioChannel channel))
			return null;
		
		if (channel == null)
		{
			Log.Error(this, "Get channel failed. Channel of type '{0}' is null.", _Type);
			return null;
		}
		
		return channel;
	}

	void ProcessFocus(AudioChannelType _Type)
	{
		foreach (AudioChannelType type in Enum.GetValues(typeof(AudioChannelType)))
		{
			if (type == _Type)
				continue;
			
			AudioChannelState state = GetState(type);
			
			if (state != AudioChannelState.Play && state != AudioChannelState.Loading)
				continue;
			
			AudioChannel channel = GetChannel(type);
			
			if (channel == null)
				continue;
			
			channel.Pause();
		}
	}

	void RestoreFocus(AudioChannelType _Type)
	{
		foreach (AudioChannelType type in Enum.GetValues(typeof(AudioChannelType)).OfType<AudioChannelType>().Reverse())
		{
			if (type >= _Type)
				continue;
			
			AudioChannelState state = GetState(type);
			
			if (state != AudioChannelState.Pause)
				continue;
			
			AudioChannel channel = GetChannel(type);
			
			if (channel == null)
				continue;
			
			channel.Play();
			
			break;
		}
	}
}
