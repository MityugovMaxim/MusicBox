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

	[SerializeField] ScrollRect    m_Scroll;
	[SerializeField] RectTransform m_Container;
	[SerializeField] RectTransform m_Content;
	[SerializeField] AudioSource   m_AudioSource;
	[SerializeField] UIGroup       m_ControlGroup;
	[SerializeField] UIAudioWave   m_Wave;
	[SerializeField] Button        m_PlayButton;
	[SerializeField] Button        m_StopButton;

	[Inject] IFileManager  m_FileManager;
	[Inject] MenuProcessor m_MenuProcessor;

	AudioClip m_Preview;

	CancellationTokenSource m_TokenSource;

	public byte[] CreatePreview()
	{
		if (m_Preview == null)
			return null;
		
		float length   = m_Preview.length;
		float duration = Mathf.Clamp(DURATION, 0, length);
		float phase    = m_Scroll.horizontalNormalizedPosition;
		
		float minTime = Mathf.Lerp(0, length - duration, phase);
		float maxTime = Mathf.Lerp(duration, length, phase);
		
		float fade = Mathf.Min(FADE, duration * 0.5f);
		
		AudioClip preview = m_Preview.Trim(minTime, maxTime);
		
		preview.FadeInOut(fade, EaseFunction.Linear);
		
		return preview.EncodeToOGG(0.85f);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		if (m_Preview != null)
			m_ControlGroup.Show(true);
		else
			m_ControlGroup.Hide(true);
	}

	async void Select()
	{
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
		
		m_Preview = await WebRequest.LoadAudioClipFile(path);
		
		if (m_Preview == null)
			return;
		
		ProcessContent();
		
		m_Scroll.horizontalNormalizedPosition = 0.5f;
		
		await m_Wave.RenderAsync(m_Preview, m_Preview.length);
		
		m_ControlGroup.Show();
	}

	async void Play()
	{
		Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		m_PlayButton.gameObject.SetActive(false);
		
		m_StopButton.gameObject.SetActive(true);
		
		try
		{
			await m_AudioSource.PlayAsync(m_TokenSource.Token);
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

	void ProcessContent()
	{
		Rect rect = m_Container.GetWorldRect();
		
		float duration = Mathf.Clamp(DURATION, 0, m_Preview.length);
		
		float unitsPerSecond = rect.width / duration;
		
		Vector2 size = m_Content.sizeDelta;
		
		size.x = m_Preview.length * unitsPerSecond;
		
		m_Content.sizeDelta = size;
	}

	void Stop()
	{
		Cancel();
	}

	void Cancel()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}