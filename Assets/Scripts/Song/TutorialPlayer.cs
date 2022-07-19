using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Video;
using Zenject;

public class TutorialPlayer : ASFPlayer
{
	[Preserve]
	public class Factory : PlaceholderFactory<TutorialPlayer, TutorialPlayer> { }

	public override double Length => 28;

	[SerializeField] UIInputReceiver m_InputReceiver;
	[SerializeField] UIColorTrack    m_ColorTrack;
	[SerializeField] UITapTrack      m_TapTrack;
	[SerializeField] UIDoubleTrack   m_DoubleTrack;
	[SerializeField] UIHoldTrack     m_HoldTrack;

	[SerializeField] UITutorialFingers m_Fingers;
	[SerializeField] UITutorialOverlay m_InputOverlay;
	[SerializeField] UITutorialOverlay m_ComboOverlay;
	[SerializeField] UITutorialLabel   m_HealthLabel;
	[SerializeField] UITutorialLabel   m_ComboLabel;
	[SerializeField] UITutorialLabel   m_TapLabel;
	[SerializeField] UITutorialLabel   m_DoubleLabel;
	[SerializeField] UITutorialLabel   m_HoldLabel;
	[SerializeField] UITutorialLabel   m_BendLabel;
	[SerializeField] UITutorialLabel   m_CompleteLabel;

	[SerializeField, Path(typeof(VideoClip))] string m_TapVideoClip;
	[SerializeField, Path(typeof(VideoClip))] string m_DoubleVideoClip;
	[SerializeField, Path(typeof(VideoClip))] string m_HoldVideoClip;
	[SerializeField, Path(typeof(VideoClip))] string m_BendVideoClip;

	[SerializeField, Sound] string m_TapSound;
	[SerializeField, Sound] string m_DoubleSound;
	[SerializeField, Sound] string m_HoldSound;
	[SerializeField, Sound] string m_BendSound;

	[Inject] ScoreManager    m_ScoreManager;
	[Inject] ConfigProcessor m_ConfigProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;

	Action m_Finished;

	CancellationTokenSource m_TokenSource;

	bool m_Input;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = null;
	}

	void LateUpdate()
	{
		if (m_Input)
			m_InputReceiver.Sample();
	}

	protected override void ProcessSamplers() { }

	public void Setup(float _Ratio, float _Speed, Action _Finished)
	{
		Rect rect = GetLocalRect();
		
		Ratio    = _Ratio;
		Duration = rect.height / _Speed;
		
		m_Finished = _Finished;
		
		m_InputReceiver.Setup(Ratio);
		m_InputOverlay.Setup(Ratio);
		m_Fingers.Setup(Ratio);
		
		AddTrack(new ASFTapTrack(m_TapTrack));
		AddTrack(new ASFDoubleTrack(m_DoubleTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		AddTrack(new ASFHoldTrack(m_HoldTrack));
		AddTrack(new ASFColorTrack(m_ColorTrack, m_ColorTrack));
		
		ASFColorClip color = new ASFColorClip(
			0,
			new Color(0, 1, 0.87f),
			new Color(1, 0.25f, 0.5f),
			new Color(1, 1, 1, 0.75f),
			new Color(1, 0.25f, 0.5f)
		);
		
		AddClips(color);
	}

	public async void Process()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		token.ThrowIfCancellationRequested();
		
		try
		{
			await HealthTutorialAsync(token);
			
			await ComboTutorialAsync(token);
			
			bool tapSuccess = await TapTutorialAsync(token);
			if (!tapSuccess)
			{
				await PlayVideoAsync(m_TapVideoClip, token);
				await TapTutorialAsync(token);
			}
			
			bool doubleSuccess = await DoubleTutorialAsync(token);
			if (!doubleSuccess)
			{
				await PlayVideoAsync(m_DoubleVideoClip, token);
				await DoubleTutorialAsync(token);
			}
			
			bool holdSuccess = await HoldTutorialAsync(token);
			if (!holdSuccess)
			{
				await PlayVideoAsync(m_HoldVideoClip, token);
				await HoldTutorialAsync(token);
			}
			
			bool bendSuccess = await BendTutorialAsync(token);
			if (!bendSuccess)
			{
				await PlayVideoAsync(m_BendVideoClip, token);
				await BendTutorialAsync(token);
			}
			
			await CompleteTutorialAsync(token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		finally
		{
			m_Finished?.Invoke();
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public void Clear()
	{
		m_TapTrack.Clear();
		m_DoubleTrack.Clear();
		m_HoldTrack.Clear();
		m_ColorTrack.Clear();
	}

	async Task HealthTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Heath tutorial canceled.");
		
		await m_InputOverlay.ShowAsync(_Token);
		
		m_HealthLabel.Show();
		
		await Task.Delay(3000, _Token);
		
		m_HealthLabel.Hide();
		
		await m_InputOverlay.HideAsync(_Token);
	}

	async Task ComboTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Combo tutorial canceled.");
		
		await m_ComboOverlay.ShowAsync(_Token);
		
		m_ComboLabel.Show();
		
		await Task.Delay(3000, _Token);
		
		m_ComboLabel.Hide();
		
		await m_ComboOverlay.HideAsync(_Token);
	}

	async Task CompleteTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Complete tutorial canceled.");
		
		m_CompleteLabel.Show();
		
		await Task.Delay(4000, _Token);
	}

	Task<bool> TapTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Tap tutorial canceled.");
		
		ASFTapClip[] clips =
		{
			new ASFTapClip(0, GetPosition(3)),   // 1
			new ASFTapClip(1.5, GetPosition(4)), // 2
			new ASFTapClip(3, GetPosition(3)),   // 3
			new ASFTapClip(4.5, GetPosition(2)), // 4
			new ASFTapClip(6, GetPosition(1)),   // 5
			new ASFTapClip(7.5, GetPosition(2)), // 6
			new ASFTapClip(9, GetPosition(3)),   // 7
			new ASFTapClip(10.5, GetPosition(4)) // 8
		};
		
		ASFColorClip color = new ASFColorClip(
			0,
			new Color(0, 1, 0.87f),
			new Color(1, 0.25f, 0.5f),
			new Color(1, 1, 1, 0.75f),
			new Color(1, 0.25f, 0.5f)
		);
		
		return TutorialAsync(
			clips,
			color,
			m_TapLabel,
			UITutorialFingers.Gesture.Tap,
			m_ConfigProcessor.TutorialTapThreshold,
			m_TapSound,
			_Token
		);
	}

	Task<bool> DoubleTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Double tutorial canceled.");
		
		ASFDoubleClip[] clips =
		{
			new ASFDoubleClip(0),  // 1
			new ASFDoubleClip(2),  // 2
			new ASFDoubleClip(4),  // 3
			new ASFDoubleClip(6),  // 4
			new ASFDoubleClip(8),  // 5
			new ASFDoubleClip(10), // 6
			new ASFDoubleClip(12), // 7
			new ASFDoubleClip(14)  // 8
		};
		
		ASFColorClip color = new ASFColorClip(
			0,
			new Color(1, 0.5f, 0),
			new Color(1, 0, 0.15f),
			new Color(1, 1, 1, 0.75f),
			new Color(1, 0, 0.15f)
		);
		
		return TutorialAsync(
			clips,
			color,
			m_DoubleLabel,
			UITutorialFingers.Gesture.Double,
			m_ConfigProcessor.TutorialDoubleThreshold,
			m_DoubleSound,
			_Token
		);
	}

	Task<bool> HoldTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Hold tutorial canceled.");
		
		ASFHoldClip[] clips =
		{
			new ASFHoldClip(0,   2, new ASFHoldKey(0, GetPosition(3)), new ASFHoldKey(2, GetPosition(3))), // 1
			new ASFHoldClip(3,   5, new ASFHoldKey(0, GetPosition(4)), new ASFHoldKey(2, GetPosition(4))), // 2
			new ASFHoldClip(6,   8, new ASFHoldKey(0, GetPosition(3)), new ASFHoldKey(2, GetPosition(3))), // 3
			new ASFHoldClip(9,  11, new ASFHoldKey(0, GetPosition(2)), new ASFHoldKey(2, GetPosition(2))), // 4
			new ASFHoldClip(12, 14, new ASFHoldKey(0, GetPosition(1)), new ASFHoldKey(2, GetPosition(1)))  // 5
		};
		
		ASFColorClip color = new ASFColorClip(
			0,
			new Color(0, 1, 0.3f),
			new Color(0, 0.6f, 1),
			new Color(1, 1, 1, 0.75f),
			new Color(0, 0.6f, 1)
		);
		
		return TutorialAsync(
			clips,
			color,
			m_HoldLabel,
			UITutorialFingers.Gesture.Hold,
			m_ConfigProcessor.TutorialHoldThreshold,
			m_HoldSound,
			_Token
		);
	}

	Task<bool> BendTutorialAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			throw new TaskCanceledException("Bend tutorial canceled.");
		
		ASFHoldClip[] clips =
		{
			new ASFHoldClip(0,   2, new ASFHoldKey(0, GetPosition(3)), new ASFHoldKey(2, GetPosition(2))), // 1
			new ASFHoldClip(3,   5, new ASFHoldKey(0, GetPosition(4)), new ASFHoldKey(2, GetPosition(3))), // 2
			new ASFHoldClip(6,   8, new ASFHoldKey(0, GetPosition(3)), new ASFHoldKey(2, GetPosition(2))), // 3
			new ASFHoldClip(9,  11, new ASFHoldKey(0, GetPosition(2)), new ASFHoldKey(2, GetPosition(1))), // 4
			new ASFHoldClip(12, 14, new ASFHoldKey(0, GetPosition(1)), new ASFHoldKey(2, GetPosition(2))), // 5
			new ASFHoldClip(15, 17, new ASFHoldKey(0, GetPosition(2)), new ASFHoldKey(2, GetPosition(3))), // 6
			new ASFHoldClip(18, 20, new ASFHoldKey(0, GetPosition(3)), new ASFHoldKey(2, GetPosition(4)))  // 7
		};
		
		ASFColorClip color = new ASFColorClip(
			0,
			new Color(0.65f, 0, 1),
			new Color(0, 0.45f, 1),
			new Color(1, 1, 1, 0.75f),
			new Color(0, 0.45f, 1)
		);
		
		return TutorialAsync(
			clips,
			color,
			m_BendLabel,
			UITutorialFingers.Gesture.Bend,
			m_ConfigProcessor.TutorialBendThreshold,
			m_BendSound,
			_Token
		);
	}

	async Task<bool> TutorialAsync<TClip>(
		TClip[]                   _Clips,
		ASFColorClip              _Color,
		UITutorialLabel           _Label,
		UITutorialFingers.Gesture _Gesture,
		float                     _Threshold,
		string                    _Sound,
		CancellationToken         _Token = default
	) where TClip : ASFClip
	{
		if (_Clips == null || _Clips.Length == 0)
			return true;
		
		float minTime = (float)_Clips[0].MinTime;
		float maxTime = (float)_Clips[^1].MaxTime;
		
		AddClips(_Clips);
		
		ASFColorTrack track = GetTrack<ASFColorTrack>();
		ASFColorClip  color = track?.Clips.LastOrDefault();
		
		if (color != null && color.Clone() is ASFColorClip startColor)
		{
			startColor.Time = minTime;
			AddClips(startColor);
		}
		
		if (_Color != null)
		{
			if (_Color.Clone() is ASFColorClip sourceColor)
			{
				sourceColor.Time = minTime + 0.05f;
				AddClips(sourceColor);
			}
			
			if (_Color.Clone() is ASFColorClip targetColor)
			{
				targetColor.Time = maxTime;
				AddClips(targetColor);
			}
		}
		
		int progress  = 0;
		int threshold = (int)(_Clips.Length * _Threshold);
		
		AudioClip sound = m_SoundProcessor.GetSound(_Sound);
		
		void ComboChanged(int _Combo, ScoreGrade _Grade)
		{
			if (_Grade != ScoreGrade.Fail && _Grade != ScoreGrade.Miss)
			{
				AudioSource.PlayOneShot(sound);
				progress++;
			}
			
			ProcessSamplers(progress, threshold);
		}
		
		m_Input = false;
		
		m_ScoreManager.OnComboChanged += ComboChanged;
		
		await SampleAsync(Duration, _Token);
		
		await m_InputOverlay.ShowAsync(_Token);
		
		_Label.Show();
		
		m_Fingers.Show(_Gesture);
		
		await InputAsync(_Token);
		
		_Label.Hide();
		
		m_Fingers.Hide();
		
		await Task.WhenAll(
			m_InputOverlay.HideAsync(_Token),
			SampleAsync(maxTime + Duration, _Token)
		);
		
		m_ScoreManager.OnComboChanged -= ComboChanged;
		
		m_Input = false;
		
		ProcessSamplers(0, threshold);
		
		return progress >= threshold;
	}

	async Task PlayVideoAsync(string _VideoClip, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_VideoClip))
			return;
		
		VideoClip clip = await ResourceManager.LoadAsync<VideoClip>(_VideoClip, _Token);
		
		if (clip == null)
			return;
		
		UIVideoMenu videoMenu = m_MenuProcessor.GetMenu<UIVideoMenu>();
		
		if (videoMenu == null)
			return;
		
		videoMenu.Setup(clip);
		
		await m_MenuProcessor.Show(MenuType.VideoMenu);
		
		await videoMenu.ProcessAsync();
	}

	static float GetPosition(int _Position)
	{
		const int min = 1;
		const int max = 4;
		return Mathf.InverseLerp(min, max, _Position);
	}

	void AddClips<TClip>(params TClip[] _Clips) where TClip : ASFClip
	{
		ASFTrack<TClip> track = GetTrack<ASFTrack<TClip>>();
		
		double time = Time + Duration;
		foreach (TClip clip in _Clips)
		{
			clip.MinTime += time;
			clip.MaxTime += time;
			track.AddClip(clip);
		}
		
		Sample();
	}

	Task SampleAsync(float _Duration, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.FromCanceled(_Token);
		
		double source = Time;
		double target = Time + _Duration;
		
		return UnityTask.Phase(
			_Phase => Time = MathUtility.Lerp(source, target, _Phase),
			_Duration,
			_Token
		);
	}

	Task InputAsync(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.FromCanceled(_Token);
		
		bool selected = false;
		
		void Select(UIHandle _Handle) => selected = _Handle != null;
		
		m_InputReceiver.OnSelect += Select;
		
		m_Input = true;
		
		return UnityTask.Until(
			() =>
			{
				if (!selected)
					return false;
				
				m_InputReceiver.OnSelect -= Select;
				
				return true;
			},
			_Token
		);
	}

	void ProcessSamplers(int _Progress, int _Threshold)
	{
		foreach (IASFSampler sampler in Samplers)
			sampler.Sample(_Progress, _Threshold);
	}
}