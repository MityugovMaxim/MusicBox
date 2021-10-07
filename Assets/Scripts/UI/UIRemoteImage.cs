using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIRemoteImage : UIEntity
{
	[SerializeField] Image    m_Image;
	[SerializeField] UIGroup  m_LoaderGroup;
	[SerializeField] UILoader m_Loader;

	CancellationTokenSource m_TokenSource;

	public async void Load(Task<Sprite> _Task)
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Loader.Restore();
		m_Loader.Play();
		
		m_Image.sprite = null;
		m_LoaderGroup.Show(true);
		
		int frame = Time.frameCount;
		
		Sprite sprite = await _Task;
		
		if (token.IsCancellationRequested)
			return;
		
		m_Image.sprite = sprite;
		
		m_LoaderGroup.Hide(frame == Time.frameCount || !gameObject.activeInHierarchy);
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}
