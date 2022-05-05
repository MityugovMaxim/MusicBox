using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBox.ASF;
using AudioBox.Logging;
using Facebook.MiniJSON;
using Facebook.Unity;
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
					new ASFHoldClip.Key(0, position),
					new ASFHoldClip.Key(length, position)
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
			
			List<ASFHoldClip.Key> keys = new List<ASFHoldClip.Key>();
			
			foreach (BendData bend in bends)
			{
				double time = (double)(bend.Time - minTime) / 1000;
				
				keys.Add(new ASFHoldClip.Key(time, bend.Position));
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

[Menu(MenuType.SongEditMenu)]
public class UISongEditMenu : UIMenu
{
	[SerializeField] UIPlayer    m_Player;
	[SerializeField] UIAudioWave m_Background;
	[SerializeField] UIBeat      m_Beat;

	[Inject] StorageProcessor m_StorageProcessor;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] ConfigProcessor  m_ConfigProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] AudioManager     m_AudioManager;
	[Inject] AmbientProcessor m_AmbientProcessor;
	[Inject] IFileManager     m_FileManager;

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
		
		string asf;
		try
		{
			asf = await LoadASF();
		}
		catch (Exception)
		{
			asf = string.Empty;
		}
		
		float ratio    = m_ConfigProcessor.SongRatio;
		float bpm      = m_SongsProcessor.GetBPM(m_SongID);
		float speed    = m_SongsProcessor.GetSpeed(m_SongID);
		float duration = RectTransform.rect.height / speed;
		
		m_Player.Setup(ratio, duration, music, asf);
		m_Player.Time = 0;
		m_Player.Sample();
		
		m_Background.Ratio     = ratio;
		m_Background.Duration  = duration;
		m_Background.AudioClip = music;
		m_Background.Time      = 0;
		
		m_Beat.Duration = duration;
		m_Beat.Ratio    = ratio;
		m_Beat.BPM      = bpm;
		m_Beat.Time     = 0;
		
		await m_Background.Render();
		
		await m_MenuProcessor.Show(MenuType.SongEditMenu);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Back()
	{
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		songMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.SongMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.SongEditMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		Dictionary<string, object> data = m_Player.Serialize();
		
		string path = $"Songs/{m_SongID}.asf";
		
		string asf = Json.Serialize(data);
		
		try
		{
			await m_StorageProcessor.UploadJson(path, asf, Encoding.UTF8);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload ASF failed.");
			
			await m_MenuProcessor.ExceptionAsync("Upload failed", exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		string asf = await LoadASF();
		
		m_Player.Clear();
		m_Player.Deserialize(asf);
		m_Player.Sample();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public void Play()
	{
		float latency = m_AudioManager.GetLatency();
		
		m_Time = m_Player.Time;
		
		m_Player.Play(latency);
	}

	public void Stop()
	{
		m_Player.Time = m_Time;
		
		m_Player.Stop();
	}

	public async void Midi()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		string path = null;
		
		try
		{
			path = await m_FileManager.SelectFile("mid");
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Select midi file failed.");
			
			await m_MenuProcessor.ExceptionAsync("Select midi failed", exception);
		}
		
		if (!string.IsNullOrEmpty(path))
		{
			MidiFile midi = MidiFile.Read(path);
			
			m_Player.Deserialize(midi);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override async void OnShowFinished()
	{
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		
		m_AmbientProcessor.Pause();
	}

	Task<AudioClip> LoadMusic()
	{
		string path = $"Songs/{m_SongID}.ogg";
		
		return m_StorageProcessor.LoadAudioClipAsync(path);
	}

	Task<string> LoadASF()
	{
		string path = $"Songs/{m_SongID}.asf";
		
		return m_StorageProcessor.LoadJson(path);
	}
}