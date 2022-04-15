using UnityEngine;

public abstract class UIHandle : UIEntity
{
	protected abstract bool Processed { get; }

	public bool Select(Rect _Rect)
	{
		if (Processed)
			return false;
		
		Rect rect = RectTransform.InverseTransformRect(_Rect);
		
		return RectTransform.rect.Overlaps(rect);
	}

	public abstract void EnterZone();

	public abstract void ExitZone();

	public abstract void Reverse();

	public abstract void Restore();

	public abstract void TouchDown(int _ID, Rect _Area);

	public abstract void TouchUp(int   _ID, Rect _Area);

	public abstract void TouchMove(int _ID, Rect _Area);
}