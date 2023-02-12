using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class AmbientManager : IDataManager
{
	public AmbientCollection Collection => m_AmbientCollection;

	[Inject] AmbientCollection m_AmbientCollection;
	[Inject] AudioProcessor    m_AudioProcessor;

	public Task<bool> Activate()
	{
		return TaskProvider.ProcessAsync(
			this,
			TaskProvider.Group(Collection.Load),
			TaskProvider.Group(CreateChannel)
		);
	}

	public void SubscribeState(Action _Action) => m_AudioProcessor.SubscribeState(AudioChannelType.Ambient, _Action);

	public void SubscribeState(Action<AudioChannelState> _Action) => m_AudioProcessor.SubscribeState(AudioChannelType.Ambient, _Action);

	public void UnsubscribeState(Action _Action) => m_AudioProcessor.UnsubscribeState(AudioChannelType.Ambient, _Action);

	public void UnsubscribeState(Action<AudioChannelState> _Action) => m_AudioProcessor.UnsubscribeState(AudioChannelType.Ambient, _Action);

	public void SubscribeTrack(Action _Action) => m_AudioProcessor.SubscribeTrack(AudioChannelType.Ambient, _Action);

	public void UnsubscribeTrack(Action _Action) => m_AudioProcessor.UnsubscribeTrack(AudioChannelType.Ambient, _Action);

	public void Play() => m_AudioProcessor.Play(AudioChannelType.Ambient);

	public void Pause() => m_AudioProcessor.Pause(AudioChannelType.Ambient);

	public void Stop() => m_AudioProcessor.Stop(AudioChannelType.Ambient);

	public void Next() => m_AudioProcessor.Next(AudioChannelType.Ambient);

	public void Previous() => m_AudioProcessor.Previous(AudioChannelType.Ambient);

	public string GetTitle() => m_AudioProcessor.GetTitle(AudioChannelType.Ambient);

	public string GetArtist() => m_AudioProcessor.GetArtist(AudioChannelType.Ambient);

	public AudioChannelState GetState() => m_AudioProcessor.GetState(AudioChannelType.Ambient);

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

	List<string> GetAmbientIDs()
	{
		return Collection.GetIDs()
			.Where(IsActive)
			.ToList();
	}

	Task CreateChannel()
	{
		List<string> ambientIDs = GetAmbientIDs();
		
		if (ambientIDs == null)
			return Task.CompletedTask;
		
		List<AudioTrack> clips = new List<AudioTrack>();
		
		foreach (string ambientID in ambientIDs)
		{
			if (string.IsNullOrEmpty(ambientID))
				continue;
			
			AudioTrack clip = new AudioTrack(
				ambientID,
				GetTitle(ambientID),
				GetArtist(ambientID),
				GetSound(ambientID)
			);
			
			clips.Add(clip);
		}
		
		if (clips.Count == 0)
			return Task.CompletedTask;
		
		AudioChannelSettings settings = new AudioChannelSettings();
		settings.Shuffle = true;
		settings.Repeat  = true;
		
		m_AudioProcessor.RegisterChannel(AudioChannelType.Ambient, settings, clips);
		
		return Task.CompletedTask;
	}

	bool IsActive(string _AmbientID)
	{
		AmbientSnapshot snapshot = Collection.GetSnapshot(_AmbientID);
		
		return snapshot?.Active ?? false;
	}

	public string GetSound(string _AmbientID)
	{
		AmbientSnapshot snapshot = Collection.GetSnapshot(_AmbientID);
		
		return snapshot?.Sound ?? string.Empty;
	}

	public float GetVolume(string _AmbientID)
	{
		AmbientSnapshot snapshot = Collection.GetSnapshot(_AmbientID);
		
		return snapshot?.Volume ?? 0;
	}
}
