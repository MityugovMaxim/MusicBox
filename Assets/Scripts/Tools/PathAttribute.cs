using System;
using UnityEngine;

public class PathAttribute : PropertyAttribute
{
	public Type Type { get; }

	public PathAttribute(Type _Type)
	{
		Type = _Type;
	}
}