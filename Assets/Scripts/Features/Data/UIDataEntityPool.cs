using UnityEngine;
using Zenject;

public class UIDataEntityPool<T> : MonoMemoryPool<RectTransform, DataNode, T> where T : UIDataEntity
{
	protected override void Reinitialize(RectTransform _Container, DataNode _DataNode, T _Item)
	{
		_Item.RectTransform.SetParent(_Container, false);
		_Item.Setup(_DataNode);
	}
}