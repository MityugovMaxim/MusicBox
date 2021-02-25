using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ReferenceAttribute : PropertyAttribute
{
	public Type ReferenceType { get; }

	public ReferenceAttribute(Type _ReferenceType)
	{
		ReferenceType = _ReferenceType;
	}
}