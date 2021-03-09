using System;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IReferenceResolver
{
	Component GetContext();
	T GetReference<T>(string _Reference) where T : Component;
	Object GetReference(Type _Type, string _Reference);
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