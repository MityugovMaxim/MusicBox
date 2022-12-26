using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class DataUtility
{
	const BindingFlags BINDING = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public;

	public static Dictionary<string, object> GetTemplate<T>() => GetTemplate(typeof(T));

	public static Dictionary<string, object> GetTemplate(Type _Type)
	{
		PropertyInfo[] properties = GetProperties(_Type);
		
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		foreach (PropertyInfo property in properties)
			GetTemplate(property, data);
		
		return data;
	}

	static void GetTemplate(PropertyInfo _Property, Dictionary<string, object> _Data)
	{
		Type type = _Property.PropertyType;
		
		string name = _Property.Name.ToID();
		
		if (IsNumber(type))
			_Data[name] = new decimal();
		else if (IsString(type))
			_Data[name] = string.Empty;
		else if (IsObject(type))
			_Data[name] = GetTemplate(type);
		else if (IsArray(type))
			_Data[name] = new List<object>();
	}

	public static bool IsNumber(Type _Type)
	{
		if (_Type == null)
			return false;
		
		return _Type == typeof(short) ||
			_Type == typeof(int) ||
			_Type == typeof(long) ||
			_Type == typeof(float) ||
			_Type == typeof(double) ||
			_Type == typeof(decimal);
	}

	public static bool IsString(Type _Type)
	{
		if (_Type == null)
			return false;
		
		return _Type == typeof(string);
	}

	public static bool IsObject(Type _Type)
	{
		if (_Type == null)
			return false;
		
		return (_Type.IsClass || _Type.IsValueType) && _Type.IsPublic && !_Type.IsAbstract && !_Type.IsGenericType;
	}

	public static bool IsArray(Type _Type)
	{
		if (_Type == null)
			return false;
		
		return _Type.IsArray ||
			_Type == typeof(IList) ||
			_Type == typeof(ICollection) ||
			_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(List<>) ||
			_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) ||
			_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(IList<>) ||
			_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(ICollection<>);
	}

	public static IDataNodeParser GetParser(Type _Type)
	{
		if (_Type == typeof(int))
			return new DataIntegerParser();
		if (_Type == typeof(long))
			return new DataLongParser();
		if (_Type == typeof(float))
			return new DataFloatParser();
		if (_Type == typeof(double))
			return new DataDoubleParser();
		if (_Type == typeof(decimal))
			return new DataDecimalParser();
		if (_Type == typeof(string))
			return new DataStringParser();
		if (_Type == typeof(bool))
			return new DataBooleanParser();
		if (_Type.IsEnum)
			return new DataEnumParser();
		if (_Type.IsValueType)
			return new DataStructParser();
		if (_Type.IsArray)
			return new DataArrayParser();
		if (_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(List<>))
			return new DataListParser();
		if (_Type.IsGenericType && _Type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			return new DataDictionaryParser();
		if (_Type.IsClass && !_Type.IsGenericType && !_Type.IsAbstract)
			return new DataObjectParser();
		return null;
	}

	public static PropertyInfo GetProperty(object _Object, string _Name)
	{
		if (_Object == null)
			return null;
		
		Type type = _Object.GetType();
		
		return type.GetProperty(_Name, BINDING);
	}

	public static PropertyInfo[] GetProperties(Type _Type) => _Type.GetProperties(BINDING).OrderBy(_Property => _Property.MetadataToken).ToArray();

	public static PropertyInfo[] GetProperties(object _Object) => _Object != null ? GetProperties(_Object.GetType()) : null;

	public static T GetCastValue<T>(PropertyInfo _Property, object _Target)
	{
		object value = GetValue(_Property, _Target);
		
		if (value is T casted)
			return casted;
		
		return (T)Convert.ChangeType(value, typeof(T));
	}

	public static void SetCastValue<T>(PropertyInfo _Property, object _Target, T _Value)
	{
		object value = Convert.ChangeType(_Value, _Property.PropertyType);
		
		SetValue(_Property, _Target, value);
	}

	public static T GetValue<T>(PropertyInfo _Property, object _Target)
	{
		if (_Property == null || _Target == null)
			return default;
		
		object data = _Property.CanRead
			? _Property.GetValue(_Target)
			: GetValue(_Property, _Target);
		
		return data is T value ? value : default;
	}

	public static void SetValue<T>(PropertyInfo _Property, object _Target, T _Value)
	{
		if (_Property == null || _Target == null)
			return;
		
		if (_Property.CanWrite)
			_Property.SetValue(_Target, _Value);
		else
			SetValue(_Property, _Target, (object)_Value);
	}

	public static object GetValue(DataNode _DataNode) => GetValue(_DataNode.Property, _DataNode.Target);

	public static object GetValue(PropertyInfo _Property, object _Target)
	{
		if (_Target == null)
			return default;
		
		FieldInfo field = GetField(_Property);
		
		if (field == null)
			return default;
		
		return field.GetValue(_Target);
	}

	public static void SetValue(PropertyInfo _Property, object _Target, object _Value)
	{
		if (_Target == null)
			return;
		
		FieldInfo field = GetField(_Property);
		
		if (field == null)
			return;
		
		field.SetValue(_Target, _Value);
	}

	public static FieldInfo GetField(PropertyInfo _Property)
	{
		if (_Property == null)
			return null;
		
		Type type = _Property.DeclaringType;
		
		if (type == null)
			return null;
		
		string name = $"<{_Property.Name}>k__BackingField";
		
		return type.GetField(name, BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
	}
}
