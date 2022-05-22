using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using AudioBox.Compression;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine;

namespace AudioBox.ASF
{
	public enum ASFPlayerState
	{
		Stop,
		Play,
		Loading,
	}

	public abstract class ASFPlayer : UIEntity
	{
		const float MUSIC_OFFSET = 1;

		public double Time
		{
			get => m_Time;
			set
			{
				m_Time = value;
				
				Sample();
			}
		}

		public float Duration
		{
			get => m_Duration;
			protected set => m_Duration = value;
		}

		public float Ratio
		{
			get => m_Ratio;
			protected set => m_Ratio = value;
		}

		public AudioClip Music
		{
			get => m_AudioSource.clip;
			set => m_AudioSource.clip = value;
		}

		public ASFPlayerState State { get; private set; } = ASFPlayerState.Stop;

		protected AudioSource AudioSource => m_AudioSource;

		float MinTime => m_Duration * (m_Ratio - 1);

		float MaxTime => m_Duration * m_Ratio;

		[SerializeField] double      m_Time;
		[SerializeField] float       m_Ratio;
		[SerializeField] float       m_Duration;
		[SerializeField] AudioSource m_AudioSource;

		readonly List<ASFTrack> m_Tracks = new List<ASFTrack>();

		CancellationTokenSource m_TokenSource;

		void Update()
		{
			if (State == ASFPlayerState.Play)
				Time += UnityEngine.Time.deltaTime;
		}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			
			Sample();
		}
		#endif

		public async void Play(float _Latency)
		{
			if (State == ASFPlayerState.Play || State == ASFPlayerState.Loading)
				return;
			
			AudioClip audioClip = m_AudioSource.clip;
			
			if (audioClip == null)
				return;
			
			State = ASFPlayerState.Loading;
			
			m_TokenSource?.Cancel();
			m_TokenSource?.Dispose();
			
			m_TokenSource = new CancellationTokenSource();
			
			CancellationToken token = m_TokenSource.Token;
			
			double offset  = -Math.Min(0, Time);
			double samples = ASFMath.RemapClamped(Time, 0, (double)audioClip.samples / audioClip.frequency, 0, audioClip.samples);
			
			m_AudioSource.Stop();
			
			m_AudioSource.PlayScheduled(AudioSettings.dspTime + offset + MUSIC_OFFSET - _Latency);
			
			m_AudioSource.timeSamples = (int)samples;
			
			double error = 0;
			
			try
			{
				error = UnityEngine.Time.realtimeSinceStartup;
				
				await UnityTask.Delay(MUSIC_OFFSET, token);
				
				error = UnityEngine.Time.realtimeSinceStartup - error - MUSIC_OFFSET;
			}
			catch (TaskCanceledException)
			{
				Log.Info(this, "Play cancelled.");
			}
			catch (Exception exception)
			{
				Log.Exception(this, exception);
			}
			
			if (token.IsCancellationRequested)
				return;
			
			State = ASFPlayerState.Play;
			
			Time -= error;
			
			Sample();
			
			m_TokenSource?.Dispose();
			m_TokenSource = null;
		}

		public void Stop()
		{
			if (State == ASFPlayerState.Stop)
				return;
			
			State = ASFPlayerState.Stop;
			
			m_TokenSource?.Cancel();
			m_TokenSource?.Dispose();
			m_TokenSource = null;
			
			m_AudioSource.Stop();
			
			Sample();
		}

		public virtual void Sample()
		{
			double minTime = Time + MinTime;
			double maxTime = Time + MaxTime;
			
			foreach (ASFTrack track in m_Tracks)
				track.Sample(Time, minTime, maxTime);
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			
			foreach (ASFTrack track in m_Tracks)
			{
				if (track == null)
				{
					Log.Error(this, "Serialization failed. Track is null.");
					continue;
				}
				
				List<object> clips = track.Serialize();
				
				if (clips == null || clips.Count == 0)
					continue;
				
				string name = track.GetType().Name;
				
				data[name] = clips;
			}
			
			return data;
		}

		public void Deserialize(string _ASF)
		{
			Dictionary<string, object> asf = Json.Deserialize(_ASF) as Dictionary<string, object>;
			
			Deserialize(asf);
		}

		protected void Deserialize(IDictionary<string, object> _Data)
		{
			if (_Data == null)
				return;
			
			foreach (string key in _Data.GetKeys())
			{
				Type type = Type.GetType($"AudioBox.ASF.{key}");
				
				if (type == null)
					continue;
				
				ASFTrack track = GetTrack(type);
				
				if (track == null)
				{
					Log.Error(this, "Deserialization error. Track '{0}' not found.", type);
					continue;
				}
				
				track.Deserialize(_Data.GetList(key));
			}
		}

		protected void AddTrack(ASFTrack _Track)
		{
			m_Tracks.Add(_Track);
		}

		protected void RemoveTrack(ASFTrack _Track)
		{
			m_Tracks.Remove(_Track);
		}

		protected void ClearTracks()
		{
			m_Tracks.Clear();
		}

		public T GetTrack<T>() where T : ASFTrack
		{
			return m_Tracks.OfType<T>().FirstOrDefault();
		}

		public ASFTrack GetTrack(Type _Type)
		{
			return m_Tracks.FirstOrDefault(_Track => _Track.GetType() == _Type);
		}

		public void SortTrack<TTrack, TClip>() where TTrack : ASFTrack<TClip> where TClip : ASFClip
		{
			TTrack track = GetTrack<TTrack>();
			
			track?.SortClips();
			
			Sample();
		}
	}
}