using System;
using System.Collections.Generic;

public static class AdminUtility
{
	public static IDictionary<string, string> GetParameters(string _Source, string _Target)
	{
		IDictionary<string, string> parameters = new Dictionary<string, string>();
		
		if (string.IsNullOrEmpty(_Source) || string.IsNullOrEmpty(_Target))
			return parameters;
		
		string[] source = _Source.Split('/');
		string[] target = _Target.Split('/');
		
		if (source.Length != target.Length)
			return parameters;
		
		for (int i = 0; i < source.Length; i++)
		{
			string a = source[i];
			string b = target[i];
			
			if (IsVariable(a))
				parameters[a[1..^1]] = b;
			else if (a != b)
				break;
		}
		
		return parameters;
	}

	public static bool Match(string _Source, string _Target)
	{
		if (_Source == _Target)
			return true;
		
		if (string.IsNullOrEmpty(_Source) || string.IsNullOrEmpty(_Target))
			return false;
		
		string[] source = GetPath(_Source);
		string[] target = GetPath(_Target);
		
		if (source.Length != target.Length)
			return false;
		
		for (int i = 0; i < source.Length; i++)
		{
			string a = source[i];
			string b = target[i];
			
			if (a == b || IsVariable(a) || IsVariable(b))
				continue;
			
			return false;
		}
		
		return true;
	}

	public static string[] GetPath(string _Path)
	{
		return !string.IsNullOrEmpty(_Path) ? _Path.Split('/', StringSplitOptions.RemoveEmptyEntries) : null;
	}

	public static string GetName(string _Path)
	{
		return !string.IsNullOrEmpty(_Path) ? System.IO.Path.GetFileName(_Path) : "null";
	}

	public static AdminNodeType GetType(object _Object)
	{
		if (IsNull(_Object))
			return AdminNodeType.Null;
		if (IsObject(_Object))
			return AdminNodeType.Object;
		if (IsNumber(_Object))
			return AdminNodeType.Number;
		if (IsString(_Object))
			return AdminNodeType.String;
		if (IsBoolean(_Object))
			return AdminNodeType.Boolean;
		if (IsArray(_Object))
			return AdminNodeType.Array;
		return AdminNodeType.Null;
	}

	public static bool IsIndex(string _Value)
	{
		return !string.IsNullOrEmpty(_Value) && _Value.Length >= 3 && _Value.StartsWith('[') && _Value.EndsWith(']');
	}

	public static int GetIndex(string _Value)
	{
		return IsIndex(_Value) && int.TryParse(_Value[1..^1], out int index) ? index : -1;
	}

	public static bool TryGetIndex(string _Value, out int _Index)
	{
		_Index = GetIndex(_Value);
		
		return _Index >= 0;
	}

	static bool IsVariable(string _Value)
	{
		return !string.IsNullOrEmpty(_Value) && _Value.StartsWith('{') && _Value.EndsWith('}');
	}

	static bool IsNull(object _Object) => _Object is null;

	static bool IsObject(object _Object) => _Object is IDictionary<string, object>;

	static bool IsString(object _Object) => _Object is string;

	static bool IsNumber(object _Object) => _Object is long or double;

	static bool IsBoolean(object _Object) => _Object is bool;

	static bool IsArray(object _Object) => _Object is IList<object>;
}
