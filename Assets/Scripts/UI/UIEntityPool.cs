using UnityEngine;
using Zenject;

public class UIEntityPool<T> : MonoMemoryPool<RectTransform, T> where T : UIEntity
{
	public Vector2 Size
	{
		get
		{
			if (m_Size.HasValue)
				return m_Size.Value;
			
			T item = GetInternal();
			
			m_Size = item.GetLocalRect().size;
			
			Despawn(item);
			
			return m_Size ?? Vector2.zero;
		}
	}

	Vector2? m_Size;

	protected override void Reinitialize(RectTransform _Container, T _Item)
	{
		_Item.RectTransform.SetParent(_Container, false);
	}
}
