using System;
using AudioBox.ASF;
using UnityEngine;
using Zenject;

public class UIIndicatorPool<T> : MonoMemoryPool<RectTransform, ASFClip, Action<ASFClip>, T> where T : UIIndicator
{
	protected override void Reinitialize(RectTransform _Container, ASFClip _Clip, Action<ASFClip> _Callback, T _Item)
	{
		Vector2 anchor = _Container.pivot;
		_Item.RectTransform.SetParent(_Container, false);
		_Item.RectTransform.anchorMin = anchor;
		_Item.RectTransform.anchorMax = anchor;
		_Item.RectTransform.pivot     = new Vector2(0.5f, 0.5f);
		_Item.Setup(_Container, _Clip, _Callback);
	}

	protected override void OnSpawned(T _Item)
	{
		base.OnSpawned(_Item);
		
		_Item.Restore();
	}
}