using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongCreatePreviewPage : UISongCreateMenuPage
{
	const float DURATION = 20;
	const float FADE     = 1.5f;

	public override bool Valid => m_Preview != null && m_Preview.length >= 30;

	[SerializeField] RectTransform m_Container;
	[SerializeField] RectTransform m_Content;
	[SerializeField] Button        m_SelectButton;
	[SerializeField] Button        m_PlayButton;
	[SerializeField] Button        m_StopButton;
	[SerializeField] AudioSource   m_AudioSource;
	[SerializeField] UIGroup       m_ControlGroup;
	[SerializeField] UIAudioWave   m_Wave;

	[Inject] IFileManager     m_FileManager;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] AmbientManager m_AmbientManager;

	AudioClip m_Preview;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		m_SelectButton.Subscribe(Select);
		m_PlayButton.Subscribe(Play);
		m_StopButton.Subscribe(Stop);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SelectButton.Unsubscribe(Select);
		m_PlayButton.Unsubscribe(Play);
		m_StopButton.Unsubscribe(Stop);
	}

	public Task<string> CreatePreview()
	{
		if (m_Preview == null)
			return Task.FromResult<string>(null);
		
		(float min, float max) = GetTime();
		
		float duration = Mathf.Abs(max - min);
		
		if (duration < float.Epsilon)
			return Task.FromResult<string>(null);
		
		float fade = Mathf.Min(FADE, duration * 0.5f);
		
		AudioClip preview = m_Preview.Trim(min, max);
		
		preview.FadeInOut(fade, EaseFunction.Linear);
		
		return preview.CacheOGG(0.85f);
	}

	protected override void OnShowStarted()
	{
		Stop();
		
		if (m_Preview != null)
			m_ControlGroup.Show(true);
		else
			m_ControlGroup.Hide(true);
	}

	protected override void OnHideStarted()
	{
		Stop();
	}

	async void Select()
	{
		Stop();
		
		string path = null;
		
		try
		{
			path = await m_FileManager.SelectFile(FileManagerUtility.AudioExtensions);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		if (string.IsNullOrEmpty(path))
			return;
		
		await m_ControlGroup.HideAsync();
		
		m_Preview = await WebRequest.LoadAudioClipFile(path, AudioType.UNKNOWN);
		
		if (m_Preview == null)
			return;
		
		m_Preview.LoadAudioData();
		
		ProcessContent();
		
		await m_Wave.RenderAsync(m_Preview, m_Preview.length);
		
		m_ControlGroup.Show();
	}

	async void Play()
	{
		Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		m_PlayButton.gameObject.SetActive(false);
		
		m_StopButton.gameObject.SetActive(true);
		
		m_AmbientManager.Pause();
		
		try
		{
			(float min, float max) = GetTime();
			
			float duration = Mathf.Abs(max - min);
			
			await m_AudioSource.PlayAsync(min, duration, m_TokenSource.Token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		catch (OperationCanceledException)
		{
			return;
		}
		finally
		{
			m_PlayButton.gameObject.SetActive(true);
			
			m_StopButton.gameObject.SetActive(false);
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Stop()
	{
		Cancel();
		
		m_AudioSource.Stop();
		
		m_StopButton.gameObject.SetActive(false);
		
		m_PlayButton.gameObject.SetActive(true);
		
		m_AmbientManager.Play();
	}

	void Cancel()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	(float min, float max) GetTime()
	{
		Rect source = m_Container.GetWorldRect();
		Rect target = m_Content.GetWorldRect();
		
		float min = MathUtility.Remap(source.xMin, target.xMin, target.xMax, 0, m_Preview.length);
		float max = MathUtility.Remap(source.xMax, target.xMin, target.xMax, 0, m_Preview.length);
		
		return (min, max);
	}

	void ProcessContent()
	{
		Rect rect = m_Container.rect;
		
		float duration = Mathf.Clamp(DURATION, 0, m_Preview.length);
		
		float pps = rect.width / duration;
		
		Vector2 size = m_Content.sizeDelta;
		
		size.x = m_Preview.length * pps;
		
		m_Content.sizeDelta = size;
		
		m_Content.ForceUpdateRectTransforms();
	}
}
