using UnityEngine;
using Zenject;

public class UIEntityPool<T> : MonoMemoryPool<RectTransform, T> where T : UIEntity
{
	protected override void Reinitialize(RectTransform _Container, T _Item)
	{
		_Item.RectTransform.SetParent(_Container, false);
	}
}