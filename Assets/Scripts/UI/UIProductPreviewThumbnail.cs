using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIProductPreviewThumbnail : UIEntity
{
	[SerializeField] Image       m_Thumbnail;
	[SerializeField] CanvasGroup m_LoaderGroup;
	[SerializeField] UILoader    m_Loader;

	StorageProcessor m_StorageProcessor;

	[Inject]
	public void Construct(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
	}

	public void Setup(string _ProductID)
	{
		m_LoaderGroup.alpha = 1;
		
		m_Loader.Restore();
		m_Loader.Play();
		
		m_Thumbnail.sprite = null;
		
		int frame = Time.frameCount;
		
		m_StorageProcessor.LoadProductThumbnail(
			_ProductID,
			_Sprite =>
			{
				if (frame == Time.frameCount)
				{
					m_LoaderGroup.alpha = 0;
					m_Loader.Restore();
				}
				else
				{
					StartCoroutine(AlphaRoutine(m_LoaderGroup, 0, 0.3f));
				}
				
				m_Thumbnail.sprite = _Sprite;
			}
		);
	}

	static IEnumerator AlphaRoutine(CanvasGroup _CanvasGroup, float _Alpha, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = Mathf.Clamp01(_Alpha);
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = target;
	}
}