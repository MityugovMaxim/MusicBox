using System;

public class AdminEnumAttribute : AdminAttribute
{
	public Type Type { get; }

	public AdminEnumAttribute(string _Path, Type _Type) : base(_Path)
	{
		Type = _Type;
	}
}

public class AdminEnumAttribute<T> : AdminEnumAttribute where T : Enum
{
	public AdminEnumAttribute(string _Path) : base(_Path, typeof(T)) { }
}
