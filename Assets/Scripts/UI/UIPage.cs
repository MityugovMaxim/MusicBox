using System;

public abstract class UIPage<T> : UIGroup where T : Enum
{
	public abstract T Type { get; }
}