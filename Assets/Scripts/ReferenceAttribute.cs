using System;
using UnityEngine;

public interface IReferenceResolver
{
	Component GetContext();
	T GetReference<T>(string _Reference) where T : Component;
	Component GetReference(Type _Type, string _Reference);
}

[AttributeUsage(AttributeTargets.Field)]
public class ReferenceAttribute : PropertyAttribute
{
	public Type Type { get; }

	public ReferenceAttribute(Type _Type)
	{
		Type = _Type;
	}
}