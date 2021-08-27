using UnityEngine;

public abstract class UIHandle : UIEntity
{
	protected abstract bool Processed { get; }

	public bool Select(Rect _Rect)
	{
		if (Processed)
			return false;
		
		Rect area = RectTransform.InverseTransformRect(_Rect);
		Rect rect = RectTransform.rect;
		
		return rect.Overlaps(area);
	}

	public abstract void StartReceiveInput();

	public abstract void StopReceiveInput();

	public abstract void TouchDown(int _ID, Rect _Area);

	public abstract void TouchUp(int   _ID, Rect _Area);

	public abstract void TouchMove(int _ID, Rect _Area);
}