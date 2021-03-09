using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SequencerDrawerAttribute : Attribute
{
	public Type Type { get; }

	public SequencerDrawerAttribute(Type _Type)
	{
		Type = _Type;
	}
}