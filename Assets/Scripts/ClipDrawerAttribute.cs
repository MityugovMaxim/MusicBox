using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ClipDrawerAttribute : Attribute
{
	public Type ClipType { get; }

	public ClipDrawerAttribute(Type _ClipType)
	{
		ClipType = _ClipType;
	}
}