using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public abstract class Thumbnail : UIEntity, IPointerClickHandler
{
	public abstract string ID { get; }

	public RectTransform Mount => m_Mount;

	public event Action OnClick;

	RectTransform m_Mount;

	[Inject]
	public void Construct(RectTransform _Mount)
	{
		m_Mount = _Mount;
		
		RectTransform.SetParent(_Mount, false);
	}

	public void OnPointerClick(PointerEventData _EventData)
	{
		OnClick?.Invoke();
	}

	public virtual void OnShow() { }

	public virtual void OnHide() { }

	[UsedImplicitly]
	public class Factory : PlaceholderFactory<string, RectTransform, Thumbnail> { }
}