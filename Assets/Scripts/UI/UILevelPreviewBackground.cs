using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelPreviewBackground : UIEntity
{
	[SerializeField] Image          m_Background;
	[SerializeField] float          m_Duration = 0.4f;
	[SerializeField] AnimationCurve m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

	LevelProcessor m_LevelProcessor;

	readonly Queue<Image> m_BackgroundPool = new Queue<Image>();

	[Inject]
	public void Construct(LevelProcessor _LevelProcessor)
	{
		m_LevelProcessor = _LevelProcessor;
	}

	public void Setup(string _LevelID, bool _Instant = false)
	{
		Sprite previewBackground = m_LevelProcessor.GetPreviewBackground(_LevelID);
		
		if (!_Instant && gameObject.activeInHierarchy)
			StartCoroutine(BackgroundRoutine(m_Background, previewBackground));
		else
			m_Background.sprite = previewBackground;
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
		
		background = Instantiate(m_Background, m_Background.rectTransform.parent, true);
		
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
		
		Color source = new Color(1, 1, 1, 0);
		Color target = new Color(1, 1, 1, 1);
		
		targetBackground.sprite = _Sprite;
		targetBackground.color  = source;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			targetBackground.color = Color.Lerp(source, target, phase);
		}
		
		targetBackground.color = target;
		
		yield return null;
		
		sourceBackground.sprite = _Sprite;
		
		RemoveBackground(targetBackground);
	}
}