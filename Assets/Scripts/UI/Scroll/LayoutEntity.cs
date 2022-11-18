using UnityEngine;

public abstract class LayoutEntity
{
	public abstract string  ID   { get; }
	public abstract Vector2 Size { get; }

	public Rect Rect { get; set; }

	public abstract void Create(RectTransform _Container);

	public abstract void Remove();

	public virtual void Refresh() { }
}
