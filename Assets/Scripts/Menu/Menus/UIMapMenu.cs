using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioBox.ASF;
using AudioBox.Logging;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using Zenject;
using Note = Melanchall.DryWetMidi.Interaction.Note;

public static class ASFMidiParser
{
	public static ASFTapClip[] GetTapClips(this MidiFile _Midi)
	{
		if (_Midi == null)
			return null;
		
		Note[] notes = _Midi.GetNotes("tap");
		
		List<ASFTapClip> clips = new List<ASFTapClip>();
		
		TempoMap tempoMap = _Midi.GetTempoMap();
		
		foreach (Note note in notes)
		{
			double time = note.GetOnTime(tempoMap);
			
			float position = note.GetTapPosition();
			
			clips.Add(new ASFTapClip(time, position));
		}
		
		return clips.ToArray();
	}

	public static ASFDoubleClip[] GetDoubleClips(this MidiFile _Midi)
	{
		if (_Midi == null)
			return null;
		
		Note[] notes = _Midi.GetNotes("double");
		
		List<ASFDoubleClip> clips = new List<ASFDoubleClip>();
		
		TempoMap tempoMap = _Midi.GetTempoMap();
		
		foreach (Note note in notes)
		{
			double time = note.GetOnTime(tempoMap);
			
			clips.Add(new ASFDoubleClip(time));
		}
		
		return clips.ToArray();
	}

	public static ASFHoldClip[] GetHoldClips(this MidiFile _Midi)
	{
		Note[] notes = _Midi.GetNotes("hold");
		
		List<ASFHoldClip> clips = new List<ASFHoldClip>();
		
		TempoMap tempoMap = _Midi.GetTempoMap();
		
		foreach (Note note in notes)
		{
			double minTime = note.GetOnTime(tempoMap);
			double maxTime = note.GetOffTime(tempoMap);
			double length  = maxTime - minTime;
			
			float position = note.GetHoldPosition();
			
			clips.Add(
				new ASFHoldClip(
					minTime,
					maxTime,
					new ASFHoldKey(0, position),
					new ASFHoldKey(length, position)
				)
			);
		}
		
		return clips.ToArray();
	}

	class BendData
	{
		public long  Time     { get; }
		public float Position { get; }

		public BendData(long _Time, float _Position)
		{
			Time     = _Time;
			Position = _Position;
		}
	}

	public static ASFHoldClip[] GetBendClips(this MidiFile _Midi, int _Channel)
	{
		TempoMap tempoMap = _Midi.GetTempoMap();
		
		Note[] notes  = _Midi.GetNotes($"bend_{_Channel}");
		Note[] slides = _Midi.GetNotes($"slide_{_Channel}");
		
		List<ASFHoldClip> clips = new List<ASFHoldClip>();
		
		foreach (Note note in notes)
		{
			long minTime = note.GetOnMilliseconds(tempoMap);
			long maxTime = note.GetOffMilliseconds(tempoMap);
			
			List<BendData> bends = GetBends(note, slides, tempoMap);
			
			List<ASFHoldKey> keys = new List<ASFHoldKey>();
			
			foreach (BendData bend in bends)
			{
				double time = (double)(bend.Time - minTime) / 1000;
				
				keys.Add(new ASFHoldKey(time, bend.Position));
			}
			
			clips.Add(
				new ASFHoldClip(
					(double)minTime / 1000,
					(double)maxTime / 1000,
					keys.ToArray()
				)
			);
		}
		
		return clips.ToArray();
	}

	static List<BendData> GetBends(Note _Note, Note[] _Slides, TempoMap _TempoMap)
	{
		float basePosition = _Note.GetBendPosition();
		long  minTime      = _Note.GetOnMilliseconds(_TempoMap);
		long  maxTime      = _Note.GetOffMilliseconds(_TempoMap);
		
		Stack<BendData> bends = new Stack<BendData>();
		bends.Push(new BendData(minTime, basePosition));
		
		foreach (Note slide in _Slides)
		{
			long minSlide = slide.GetOnMilliseconds(_TempoMap);
			long maxSlide = slide.GetOffMilliseconds(_TempoMap);
			
			if (minSlide > maxTime || maxSlide < minTime)
				continue;
			
			while (bends.Count > 0 && bends.Peek().Time >= minSlide)
				bends.Pop();
			
			float position = bends.Count > 0
				? bends.Peek().Position
				: basePosition;
			
			bends.Push(new BendData(minSlide, position));
			bends.Push(new BendData(maxSlide, slide.GetBendPosition()));
		}
		
		float lastPosition = bends.Count > 0
			? bends.Peek().Position
			: basePosition;
		
		while (bends.Count > 0 && bends.Peek().Time >= maxTime)
			bends.Pop();
		
		bends.Push(new BendData(maxTime, lastPosition));
		
		return bends.ToList();
	}

	static double GetOnTime(this Note _Note, TempoMap _TempoMap)
	{
		if (_Note == null)
			return 0;
		
		TimedEvent noteTime = _Note.GetTimedNoteOnEvent();
		
		if (noteTime == null)
			return 0;
		
		MetricTimeSpan timeSpan = noteTime.TimeAs<MetricTimeSpan>(_TempoMap);
		
		if (timeSpan == null)
			return 0;
		
		return (double)timeSpan.TotalMicroseconds / 1000000;
	}

	static long GetOnMilliseconds(this Note _Note, TempoMap _TempoMap)
	{
		if (_Note == null)
			return 0;
		
		TimedEvent noteTime = _Note.GetTimedNoteOnEvent();
		
		if (noteTime == null)
			return 0;
		
		MetricTimeSpan timeSpan = noteTime.TimeAs<MetricTimeSpan>(_TempoMap);
		
		if (timeSpan == null)
			return 0;
		
		return timeSpan.TotalMicroseconds / 1000;
	}

	static long GetOffMilliseconds(this Note _Note, TempoMap _TempoMap)
	{
		if (_Note == null)
			return 0;
		
		TimedEvent noteTime = _Note.GetTimedNoteOffEvent();
		
		if (noteTime == null)
			return 0;
		
		MetricTimeSpan timeSpan = noteTime.TimeAs<MetricTimeSpan>(_TempoMap);
		
		if (timeSpan == null)
			return 0;
		
		return timeSpan.TotalMicroseconds / 1000;
	}

	static double GetOffTime(this Note _Note, TempoMap _TempoMap)
	{
		if (_Note == null)
			return 0;
		
		TimedEvent noteTime = _Note.GetTimedNoteOffEvent();
		
		if (noteTime == null)
			return 0;
		
		MetricTimeSpan timeSpan = noteTime.TimeAs<MetricTimeSpan>(_TempoMap);
		
		if (timeSpan == null)
			return 0;
		
		return (double)timeSpan.TotalMicroseconds / 1000000;
	}

	public static Note[] GetNotes(this MidiFile _Midi, string _TrackName)
	{
		List<Note> notes = new List<Note>();
		foreach (TrackChunk trackChunk in _Midi.GetTrackChunks(_TrackName))
			notes.AddRange(trackChunk.GetNotes());
		return notes.ToArray();
	}

	public static PitchBendEvent[] GetPitchBends(this MidiFile _Midi, string _TrackName)
	{
		List<PitchBendEvent> pitchBends = new List<PitchBendEvent>();
		foreach (TrackChunk trackChunk in _Midi.GetTrackChunks(_TrackName))
			pitchBends.AddRange(trackChunk.Events.OfType<PitchBendEvent>());
		return pitchBends.ToArray();
	}

	public static TrackChunk[] GetTrackChunks(this MidiFile _Midi, string _TrackName)
	{
		List<TrackChunk> trackChunks = new List<TrackChunk>();
		foreach (TrackChunk trackChunk in _Midi.GetTrackChunks())
		foreach (SequenceTrackNameEvent midiEvent in trackChunk.Events.OfType<SequenceTrackNameEvent>())
		{
			if (midiEvent.Text != _TrackName)
				continue;
			
			trackChunks.Add(trackChunk);
			
			break;
		}
		return trackChunks.ToArray();
	}

	static float GetTapPosition(this Note _Note)
	{
		const float step = 1.0f / 3.0f;
		switch (_Note.NoteName)
		{
			case NoteName.C:
			case NoteName.CSharp:
			case NoteName.D:
				return step * 0;
			case NoteName.DSharp:
			case NoteName.E:
			case NoteName.F:
				return step * 1;
			case NoteName.FSharp:
			case NoteName.G:
			case NoteName.GSharp:
				return step * 2;
			case NoteName.A:
			case NoteName.ASharp:
			case NoteName.B:
				return step * 3;
			default:
				return 0;
		}
	}

	static float GetHoldPosition(this Note _Note)
	{
		const float step = 1.0f / 3.0f;
		switch (_Note.NoteName)
		{
			case NoteName.C:
			case NoteName.CSharp:
			case NoteName.D:
				return step * 0;
			case NoteName.DSharp:
			case NoteName.E:
			case NoteName.F:
				return step * 1;
			case NoteName.FSharp:
			case NoteName.G:
			case NoteName.GSharp:
				return step * 2;
			case NoteName.A:
			case NoteName.ASharp:
			case NoteName.B:
				return step * 3;
			default:
				return 0;
		}
	}

	static float GetBendPosition(this Note _Note)
	{
		const float step = 1.0f / 6.0f;
		switch (_Note.NoteName)
		{
			case NoteName.C:
				return step * 0;
			case NoteName.CSharp:
			case NoteName.D:
				return step * 1;
			case NoteName.DSharp:
			case NoteName.E:
				return step * 2;
			case NoteName.F:
			case NoteName.FSharp:
				return step * 3;
			case NoteName.G:
			case NoteName.GSharp:
				return step * 4;
			case NoteName.A:
			case NoteName.ASharp:
				return step * 5;
			case NoteName.B:
				return step * 6;
			default:
				return 0;
		}
	}
}

[Menu(MenuType.MapMenu)]
public class UIMapMenu : UIMenu
{
	[SerializeField] UIPlayer    m_Player;
	[SerializeField] UIAudioWave m_Background;
	[SerializeField] UIBeat      m_Beat;

	[Inject] AudioClipProvider m_AudioClipProvider;
	[Inject] ASFProvider       m_ASFProvider;
	[Inject] SongsManager      m_SongsManager;
	[Inject] ConfigProcessor   m_ConfigProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;
	[Inject] AudioManager      m_AudioManager;
	[Inject] UIRecordHandle    m_RecordHandle;

	string m_SongID;
	double m_Time;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
	}

	public async Task Load()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		AudioClip music;
		try
		{
			music = await LoadMusic();
		}
		catch (Exception)
		{
			music = null;
		}
		
		ASFFile asf;
		try
		{
			asf = await LoadASF();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			asf = null;
		}
		
		float ratio    = m_ConfigProcessor.SongRatio;
		float speed    = m_SongsManager.GetSpeed(m_SongID);
		float duration = RectTransform.rect.height / speed;
		
		m_Player.Setup(ratio, duration, music, asf);
		m_Player.Time = 0;
		m_Player.Sample();
		
		m_Background.Ratio     = ratio;
		m_Background.Duration  = duration;
		m_Background.AudioClip = music;
		m_Background.Time      = 0;
		
		m_Beat.Setup(m_SongID);
		
		await m_Background.RenderAsync();
		
		await m_MenuProcessor.Show(MenuType.MapMenu);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Back()
	{
		string artist = m_SongsManager.GetArtist(m_SongID);
		string title  = m_SongsManager.GetTitle(m_SongID);
		
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"song_edit",
			$"EXIT '{artist} - {title}' EDIT",
			"Are you sure want to exit map edit?\nAll unsaved changes will be lost."
		);
		
		if (!confirm)
			return;
		
		//m_AmbientManager.Play();
		
		m_Player.Stop();
		
		m_Player.Time = 0;
		
		Hide();
	}

	public async void Test()
	{
		string artist = m_SongsManager.GetArtist(m_SongID);
		string title  = m_SongsManager.GetTitle(m_SongID);
		
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"song_test",
			$"TEST '{artist} - {title}'",
			"Are you sure want to test song?\nAll unsaved changes will be lost."
		);
		
		if (!confirm)
			return;
		
		//m_AmbientManager.Play();
		
		m_Player.Stop();
		m_Player.Music.UnloadAudioData();
		m_Background.AudioClip.UnloadAudioData();
		m_Player.Music         = null;
		m_Player.ASF           = null;
		m_Background.AudioClip = null;
		m_Player.Time          = 0;
		
		await m_MenuProcessor.Hide(MenuType.MapsMenu, true);
		
		Hide();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		UILoadingMenu loadingMenu = m_MenuProcessor.GetMenu<UILoadingMenu>();
		
		loadingMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.LoadingMenu);
		
		loadingMenu.Load();
		
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	public async void Upload()
	{
		string artist = m_SongsManager.GetArtist(m_SongID);
		string title  = m_SongsManager.GetTitle(m_SongID);
		
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"upload",
			$"UPLOAD '{artist} - {title}'",
			"Are you sure want to upload map?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		m_Player.Save();
		
		string path = m_SongsManager.GetASF(m_SongID);
		
		try
		{
			await m_ASFProvider.UploadAsync(path, m_Player.ASF);
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Restore()
	{
		string artist = m_SongsManager.GetArtist(m_SongID);
		string title  = m_SongsManager.GetTitle(m_SongID);
		
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"restore",
			$"RESTORE '{artist} - {title}'",
			"Are you sure want to restore map?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			float ratio = m_ConfigProcessor.SongRatio;
			
			float speed = m_SongsManager.GetSpeed(m_SongID);
			
			AudioClip music = await LoadMusic();
			
			ASFFile asf = await LoadASF();
			
			m_Player.Clear();
			m_Player.Setup(ratio, speed, music, asf);
			m_Player.Sample();
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public void Record()
	{
		float latency = m_AudioManager.GetLatency();
		
		m_Time = m_Player.Time;
		
		m_Player.Play(latency);
		
		m_RecordHandle.gameObject.SetActive(true);
	}

	public async void Trim()
	{
		string artist = m_SongsManager.GetArtist(m_SongID);
		string title  = m_SongsManager.GetTitle(m_SongID);
		
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"trim",
			$"TRIM '{artist} - {title}'",
			"Are you sure want to trim map?"
		);
		
		if (!confirm)
			return;
		
		m_Player.ASF.Trim(m_Player.Time);
		
		m_Player.Load();
		
		m_Player.Sample();
	}

	public void Play()
	{
		float latency = m_AudioManager.GetLatency();
		
		m_Time = m_Player.Time;
		
		m_Player.Play(latency);
		
		m_RecordHandle.gameObject.SetActive(false);
	}

	public void Stop()
	{
		if (m_Player.State != ASFPlayerState.Play)
			m_Player.Time = m_Time;
		
		m_Player.Stop();
		
		m_RecordHandle.gameObject.SetActive(false);
	}

	protected override async void OnShowFinished()
	{
		base.OnShowFinished();
		
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		
		//m_AmbientManager.Pause();
	}

	Task<AudioClip> LoadMusic()
	{
		string path = m_SongsManager.GetMusic(m_SongID);
		
		return m_AudioClipProvider.DownloadAsync(path);
	}

	Task<ASFFile> LoadASF()
	{
		string path = m_SongsManager.GetASF(m_SongID);
		
		return m_ASFProvider.DownloadAsync(path);
	}
}
