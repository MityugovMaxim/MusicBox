using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class TutorialPlayer : ASFPlayer
{
	[Preserve]
	public class Factory : PlaceholderFactory<TutorialPlayer, TutorialPlayer> { }

	public override double Length => 28;

	[SerializeField] UITapTrack      m_TapTrack;
	[SerializeField] UIDoubleTrack   m_DoubleTrack;
	[SerializeField] UIHoldTrack     m_HoldTrack;
	[SerializeField] UIColorTrack    m_ColorTrack;
	[SerializeField] RectTransform   m_InputArea;
	[SerializeField] UIInputReceiver m_InputReceiver;

	[SerializeField] UIGroup           m_ComboGroup;
	[SerializeField] UIGroup           m_CompleteGroup;
	[SerializeField] UITutorialOverlay m_InputGroup;
	[SerializeField] UITutorialOverlay m_TapGroup;
	[SerializeField] UITutorialOverlay m_DoubleGroup;
	[SerializeField] UITutorialOverlay m_HoldGroup;
	[SerializeField] UITutorialOverlay m_BendGroup;

	[SerializeField, Sound] string m_OverlaySound;
	[SerializeField, Sound] string m_TapSuccessSound;
	[SerializeField, Sound] string m_DoubleSuccessSound;
	[SerializeField, Sound] string m_HoldHitSound;
	[SerializeField, Sound] string m_HoldSuccessSound;

	[Inject] SignalBus       m_SignalBus;
	[Inject] HealthManager   m_HealthManager;
	[Inject] SoundProcessor  m_SoundProcessor;
	[Inject] ConfigProcessor m_ConfigProcessor;

	Action m_Finished;

	CancellationTokenSource m_TokenSource;

	readonly Queue<Func<CancellationToken, Task>> m_Actions = new Queue<Func<CancellationToken, Task>>();

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = null;
	}

	public void Setup(float _Ratio, float _Duration, Action _Finished)
	{
		Ratio    = _Ratio;
		Duration = _Duration;
		
		float position = 1.0f - Ratio;
		
		m_Finished = _Finished;
		
		m_InputArea.anchorMin = new Vector2(0, position);
		m_InputArea.anchorMax = new Vector2(1, position);
		
		m_InputReceiver.gameObject.SetActive(false);
		
		m_InputGroup.Setup(Ratio);
		m_TapGroup.Setup(Ratio);
		m_DoubleGroup.Setup(Ratio);
		m_HoldGroup.Setup(Ratio);
		m_BendGroup.Setup(Ratio);
		
		CreateTapTrack();
		CreateDoubleTrack();
		CreateHoldSimpleTrack();
		CreateHoldAdvancedTrack();
		CreateColorTrack();
		
		m_Actions.Enqueue(InputAction);
		m_Actions.Enqueue(ComboAction);
		m_Actions.Enqueue(TapAutoAction);
		m_Actions.Enqueue(TapManualAction);
		m_Actions.Enqueue(DoubleAutoAction);
		m_Actions.Enqueue(DoubleManualAction);
		m_Actions.Enqueue(HoldSimpleAutoAction);
		m_Actions.Enqueue(HoldSimpleManualAction);
		m_Actions.Enqueue(HoldAdvancedAutoAction);
		m_Actions.Enqueue(HoldAdvancedManualAction);
		m_Actions.Enqueue(CompleteAction);
	}

	public async void Process()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		token.Register(() => m_Actions.Clear());
		
		void TapSuccess() => PlaySound(m_TapSuccessSound);
		void DoubleSuccess() => PlaySound(m_DoubleSuccessSound);
		void HoldHit() => PlaySound(m_HoldHitSound);
		void HoldSuccess() => PlaySound(m_HoldSuccessSound);
		
		m_SignalBus.Subscribe<TapSuccessSignal>(TapSuccess);
		m_SignalBus.Subscribe<DoubleSuccessSignal>(DoubleSuccess);
		m_SignalBus.Subscribe<HoldHitSignal>(HoldHit);
		m_SignalBus.Subscribe<HoldSuccessSignal>(HoldSuccess);
		
		while (m_Actions.Count > 0)
		{
			if (token.IsCancellationRequested)
				break;
			
			try
			{
				await m_Actions.Dequeue().Invoke(token);
			}
			catch (TaskCanceledException) { }
			catch (Exception exception)
			{
				Log.Exception(this, exception);
			}
		}
		
		m_SignalBus.Unsubscribe<TapSuccessSignal>(TapSuccess);
		m_SignalBus.Unsubscribe<DoubleSuccessSignal>(DoubleSuccess);
		m_SignalBus.Unsubscribe<HoldHitSignal>(HoldHit);
		m_SignalBus.Unsubscribe<HoldSuccessSignal>(HoldSuccess);
		
		if (token.IsCancellationRequested)
			return;
		
		m_Finished?.Invoke();
		
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
	}

	async Task InputAction(CancellationToken _Token = default)
	{
		await Task.Delay(1000, _Token);
		
		if (_Token.IsCancellationRequested)
			return;
		
		m_SoundProcessor.Play(m_OverlaySound);
		
		await m_InputGroup.ShowAsync();
		
		await Task.Delay(2000, _Token);
		
		TapFailSignal signal = new TapFailSignal(0);
		
		float iframes = m_ConfigProcessor.SongIFrames;
		
		for (int i = 0; i < 4; i++)
		{
			m_SignalBus.Fire(signal);
			
			await UnityTask.Delay(iframes, _Token);
			
			await Task.Delay(150, _Token);
			
			if (_Token.IsCancellationRequested)
				break;
		}
		
		await Task.Delay(4000, _Token);
		
		if (_Token.IsCancellationRequested)
			return;
		
		m_HealthManager.Restore();
		
		m_InputGroup.Hide();
	}

	async Task ComboAction(CancellationToken _Token = default)
	{
		await Task.Delay(1000, _Token);
		
		if (_Token.IsCancellationRequested)
			return;
		
		m_SoundProcessor.Play(m_OverlaySound);
		
		await m_ComboGroup.ShowAsync();
		
		await Task.Delay(1500, _Token);
		
		int combo = m_ConfigProcessor.ComboX2;
		for (int i = 1; i <= combo; i++)
		{
			ScoreSignal signal = new ScoreSignal(
				i * 100,
				i,
				i < combo ? 1 : 2,
				(float)(i % combo) / combo,
				ScoreGrade.None
			);
			
			await Task.Delay(250, _Token);
			
			m_SignalBus.Fire(signal);
			
			if (_Token.IsCancellationRequested)
				break;
		}
		
		await Task.Delay(4000, _Token);
		
		if (_Token.IsCancellationRequested)
			return;
		
		await m_ComboGroup.HideAsync();
		
		await Task.Delay(1000, _Token);
		
		m_SignalBus.Fire(new TapFailSignal(0));
		
		await Task.Delay(1500, _Token);
	}

	Task TapAutoAction(CancellationToken _Token = default) => AutoAction<TapSuccessSignal>(Time, 0, m_TapGroup, _Token);

	Task TapManualAction(CancellationToken _Token = default) => ManualAction(0, 4, _Token);

	Task DoubleAutoAction(CancellationToken _Token = default) => AutoAction<DoubleSuccessSignal>(4, 5, m_DoubleGroup, _Token);

	Task DoubleManualAction(CancellationToken _Token = default) => ManualAction(5, 11, _Token);

	Task HoldSimpleAutoAction(CancellationToken _Token = default) => AutoAction<HoldHitSignal>(11, 12, m_HoldGroup, _Token);

	Task HoldSimpleManualAction(CancellationToken _Token = default) => ManualAction(12, 20, _Token);

	Task HoldAdvancedAutoAction(CancellationToken _Token = default) => AutoAction<HoldHitSignal>(20, 21, m_BendGroup, _Token);

	Task HoldAdvancedManualAction(CancellationToken _Token = default) => ManualAction(21, 30, _Token);

	async Task CompleteAction(CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return;
		
		m_SoundProcessor.Play(m_OverlaySound);
		
		_Token.Register(() => m_CompleteGroup.Hide(true));
		
		await m_CompleteGroup.ShowAsync();
		
		await Task.Delay(2500, _Token);
	}

	async Task AutoAction<TSignal>(double _Source, double _Target, UIGroup _Group, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return;
		
		m_InputReceiver.gameObject.SetActive(false);
		
		await UnityTask.Phase(
			_Phase => Time = MathUtility.Lerp(_Source, _Target, _Phase),
			(float)(_Target - _Source),
			_Token
		);
		
		if (_Token.IsCancellationRequested)
			return;
		
		m_SoundProcessor.Play(m_OverlaySound);
		
		_Token.Register(() => _Group.Hide(true));
		
		await _Group.ShowAsync();
		
		await Task.Delay(500, _Token);
		
		bool success = false;
		
		void Success() => success = true;
		
		m_HealthManager.Restore();
		
		m_SignalBus.Subscribe<TSignal>(Success);
		
		m_InputReceiver.gameObject.SetActive(true);
		
		m_InputReceiver.Process();
		
		await UnityTask.Until(() => success, _Token);
		
		m_SignalBus.Unsubscribe<TSignal>(Success);
		
		m_InputReceiver.gameObject.SetActive(false);
		
		_Group.Hide();
	}

	async Task ManualAction(double _Source, double _Target, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return;
		
		m_InputReceiver.gameObject.SetActive(true);
		
		await UnityTask.Phase(
			_Phase =>
			{
				Time = MathUtility.Lerp(_Source, _Target, _Phase);
				m_InputReceiver.Process();
			},
			(float)(_Target - _Source), _Token
		);
		
		m_InputReceiver.gameObject.SetActive(false);
	}

	void PlaySound(string _Sound)
	{
		AudioClip sound = m_SoundProcessor.GetSound(_Sound);
		float     pitch = m_SoundProcessor.GetPitch(_Sound);
		
		if (sound != null)
		{
			AudioSource.pitch = pitch;
			AudioSource.PlayOneShot(sound);
		}
	}

	void CreateTapTrack()
	{
		const float step = 1.0f / 3.0f;
		
		ASFTapTrack track = new ASFTapTrack(m_TapTrack);
		
		ASFTapClip clip0 = new ASFTapClip(0, step * 3);
		ASFTapClip clip1 = new ASFTapClip(1, step * 2);
		ASFTapClip clip2 = new ASFTapClip(2, step * 1);
		ASFTapClip clip3 = new ASFTapClip(3, step * 0);
		
		track.AddClip(clip0);
		track.AddClip(clip1);
		track.AddClip(clip2);
		track.AddClip(clip3);
		
		AddTrack(track);
	}

	void CreateDoubleTrack()
	{
		ASFDoubleTrack track = new ASFDoubleTrack(m_DoubleTrack);
		
		ASFDoubleClip clip0 = new ASFDoubleClip(5);
		ASFDoubleClip clip1 = new ASFDoubleClip(6.5);
		ASFDoubleClip clip2 = new ASFDoubleClip(8);
		ASFDoubleClip clip3 = new ASFDoubleClip(9.5);
		
		track.AddClip(clip0);
		track.AddClip(clip1);
		track.AddClip(clip2);
		track.AddClip(clip3);
		
		AddTrack(track);
	}

	void CreateHoldSimpleTrack()
	{
		const float step = 1.0f / 3.0f;
		
		ASFHoldTrack track = new ASFHoldTrack(m_HoldTrack);
		
		ASFHoldClip clip0 = new ASFHoldClip(12, 13, new ASFHoldClip.Key(0, step * 3), new ASFHoldClip.Key(1, step * 3));
		ASFHoldClip clip1 = new ASFHoldClip(14, 15, new ASFHoldClip.Key(0, step * 2), new ASFHoldClip.Key(1, step * 2));
		ASFHoldClip clip2 = new ASFHoldClip(16, 17, new ASFHoldClip.Key(0, step * 1), new ASFHoldClip.Key(1, step * 1));
		ASFHoldClip clip3 = new ASFHoldClip(18, 19, new ASFHoldClip.Key(0, step * 0), new ASFHoldClip.Key(1, step * 0));
		
		track.AddClip(clip0);
		track.AddClip(clip1);
		track.AddClip(clip2);
		track.AddClip(clip3);
		
		AddTrack(track);
	}

	void CreateHoldAdvancedTrack()
	{
		const float step = 1.0f / 3.0f;
		
		ASFHoldTrack track = new ASFHoldTrack(m_HoldTrack);
		
		ASFHoldClip clip0 = new ASFHoldClip(21, 22, new ASFHoldClip.Key(0, step * 3), new ASFHoldClip.Key(1, step * 2));
		ASFHoldClip clip1 = new ASFHoldClip(23, 24, new ASFHoldClip.Key(0, step * 2), new ASFHoldClip.Key(1, step * 1));
		ASFHoldClip clip2 = new ASFHoldClip(25, 26, new ASFHoldClip.Key(0, step * 1), new ASFHoldClip.Key(1, step * 2));
		ASFHoldClip clip3 = new ASFHoldClip(27, 28, new ASFHoldClip.Key(0, step * 0), new ASFHoldClip.Key(1, step * 1));
		
		track.AddClip(clip0);
		track.AddClip(clip1);
		track.AddClip(clip2);
		track.AddClip(clip3);
		
		AddTrack(track);
	}

	void CreateColorTrack()
	{
		ASFColorTrack track = new ASFColorTrack(m_ColorTrack, m_ColorTrack);
		
		ASFColorClip clip0 = new ASFColorClip(
			0,
			new Color(0, 1, 0.87f),
			new Color(1, 0.25f, 0.5f),
			new Color(1, 1, 1, 0.75f),
			new Color(1, 0.25f, 0.5f)
		);
		
		ASFColorClip clip1 = new ASFColorClip(
			5,
			new Color(0, 1, 0.87f),
			new Color(1, 0.25f, 0.5f),
			new Color(1, 1, 1, 0.75f),
			new Color(1, 0.25f, 0.5f)
		);
		
		ASFColorClip clip2 = new ASFColorClip(
			5.1,
			new Color(1f, 0.61f, 0f),
			new Color(1f, 0f, 0.25f),
			new Color(1, 1, 1, 0.75f),
			new Color(1f, 0f, 0.25f)
		);
		
		ASFColorClip clip3 = new ASFColorClip(
			12,
			new Color(1f, 0.61f, 0f),
			new Color(1f, 0f, 0.25f),
			new Color(1, 1, 1, 0.75f),
			new Color(1f, 0f, 0.25f)
		);
		
		ASFColorClip clip4 = new ASFColorClip(
			12.1,
			new Color(0.73f, 0f, 1f),
			new Color(0f, 0.63f, 1f),
			new Color(1, 1, 1, 0.75f),
			new Color(0f, 0.63f, 1f)
		);
		
		ASFColorClip clip5 = new ASFColorClip(
			21,
			new Color(0.73f, 0f, 1f),
			new Color(0f, 0.63f, 1f),
			new Color(1, 1, 1, 0.75f),
			new Color(0f, 0.63f, 1f)
		);
		
		ASFColorClip clip6 = new ASFColorClip(
			21.1,
			new Color(0.33f, 1f, 0f),
			new Color(0.75f, 0f, 0.04f),
			new Color(1, 1, 1, 0.75f),
			new Color(0.75f, 0f, 0.04f)
		);
		
		track.AddClip(clip0);
		track.AddClip(clip1);
		track.AddClip(clip2);
		track.AddClip(clip3);
		track.AddClip(clip4);
		track.AddClip(clip5);
		track.AddClip(clip6);
		
		AddTrack(track);
	}

	public void Clear()
	{
		m_TapTrack.Clear();
		m_DoubleTrack.Clear();
		m_HoldTrack.Clear();
		m_ColorTrack.Clear();
	}
}