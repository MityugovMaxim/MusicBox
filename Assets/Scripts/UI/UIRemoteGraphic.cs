using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UIRemoteGraphic : UIEntity
{
	[SerializeField] UIImage  m_Image;
	[SerializeField] Sprite   m_Default;
	[SerializeField] UIGroup  m_LoaderGroup;
	[SerializeField] UILoader m_Loader;

	CancellationTokenSource m_TokenSource;

	public void Load(Uri _Uri)
	{
		Load(WebRequest.LoadSprite(_Uri?.ToString()));
	}

	public void Load(string _URL)
	{
		Load(WebRequest.LoadSprite(_URL));
	}

	public async void Load(Task<Sprite> _Task)
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Loader.Restore();
		m_Loader.Play();
		
		m_Image.Sprite = null;
		m_Image.gameObject.SetActive(false);
		m_LoaderGroup.Show(true);
		
		int frame = Time.frameCount;
		
		Sprite sprite = await _Task;
		
		if (token.IsCancellationRequested)
			return;
		
		if (sprite != null || m_Default != null)
		{
			m_Image.Sprite = sprite != null ? sprite : m_Default;
			m_Image.gameObject.SetActive(true);
			
			m_LoaderGroup.Hide(frame == Time.frameCount || !gameObject.activeInHierarchy);
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}