using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIProductPreviewBackground : UIEntity
{
	[SerializeField] Image          m_Background;
	[SerializeField] float          m_Duration = 0.4f;
	[SerializeField] AnimationCurve m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

	StorageProcessor m_StorageProcessor;

	readonly Queue<Image> m_BackgroundPool = new Queue<Image>();

	[Inject]
	public void Construct(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
	}

	public void Setup(string _ProductID, bool _Instant = false)
	{
		m_StorageProcessor.LoadProductBackground(
			_ProductID,
			_Background =>
			{
				if (!_Instant && gameObject.activeInHierarchy)
					StartCoroutine(BackgroundRoutine(m_Background, _Background));
				else
					m_Background.sprite = _Background;
			}
		);
	}

	Image CreateBackground()
	{
		Image background;
		while (m_BackgroundPool.Count > 0)
		{
			background = m_BackgroundPool.Dequeue();
			
			if (background == null)
				continue;
			
			background.gameObject.SetActive(true);
			background.transform.SetAsLastSibling();
			
			return background;
		}
		
		background       = Instantiate(m_Background, m_Background.rectTransform.parent, true);
		background.color = m_Background.color;
		
		return background;
	}

	void RemoveBackground(Image _Background)
	{
		if (_Background == null)
			return;
		
		_Background.gameObject.SetActive(false);
		
		m_BackgroundPool.Enqueue(_Background);
	}

	IEnumerator BackgroundRoutine(Image _Background, Sprite _Sprite)
	{
		if (_Background == null)
			yield break;
		
		Image sourceBackground = _Background;
		Image targetBackground = CreateBackground();
		
		const float source = 0;
		const float target = 1;
		
		Color color = sourceBackground.color;
		
		color.a = source;
		
		targetBackground.sprite = _Sprite;
		targetBackground.color  = color;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			color.a = Mathf.Lerp(source, target, phase);
			
			targetBackground.color = color;
		}
		
		color.a = target;
		
		targetBackground.color = color;
		
		yield return null;
		
		sourceBackground.sprite = _Sprite;
		
		RemoveBackground(targetBackground);
	}
}