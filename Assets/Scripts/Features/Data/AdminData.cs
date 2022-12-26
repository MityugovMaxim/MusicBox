using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing;
using Task = System.Threading.Tasks.Task;

public abstract class AdminData
{
	public AdminNode Root { get; private set; }

	protected IDictionary<string, object> Data { get; set; }

	protected abstract AdminAttribute[] Attributes { get; }

	public abstract AdminNode CreateObject(AdminNode _Node, string _Path);

	public bool HasAttribute<T>(string _Path) where T : AdminAttribute
	{
		foreach (T modificator in Attributes.OfType<T>())
		{
			if (AdminUtility.Match(modificator.Path, _Path))
				return true;
		}
		return false;
	}

	public T GetAttribute<T>(string _Path) where T : AdminAttribute
	{
		foreach (T modificator in Attributes.OfType<T>())
		{
			if (AdminUtility.Match(modificator.Path, _Path))
				return modificator;
		}
		return null;
	}

	public bool Contains(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return Data != null;
		
		string[] path = AdminUtility.GetPath(_Path);
		
		if (path == null || path.Length == 0)
			return false;
		
		if (path.Length == 1)
			return Data.ContainsKey(path[0]);
		
		object data = Data;
		foreach (string node in path)
		{
			if (data is IDictionary<string, object> dataObject)
			{
				if (!dataObject.ContainsKey(node))
					return false;
				data = dataObject[node];
			}
			else if (data is IList<object> dataArray)
			{
				if (!AdminUtility.TryGetIndex(node, out int index) || index < 0 || index >= dataArray.Count)
					return false;
				data = dataArray[index];
			}
		}
		
		return data != null;
	}

	public object GetValue(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return Data;
		
		string[] path = AdminUtility.GetPath(_Path);
		
		if (path == null || path.Length == 0)
			return default;
		
		if (path.Length == 1)
			return Data[path[^1]];
		
		object data = Data;
		foreach (string node in path)
		{
			if (data is IDictionary<string, object> dataObject)
			{
				if (!dataObject.ContainsKey(node))
					return default;
				
				data = dataObject[node];
			}
			else if (data is IList<object> dataArray)
			{
				if (!AdminUtility.TryGetIndex(node, out int index))
					return default;
				
				data = dataArray[index];
			}
			else
			{
				return default;
			}
		}
		
		return data;
	}

	public T GetValue<T>(string _Path) => GetValue(_Path) is T value ? value : default;

	public void SetValue<T>(string _Path, T _Value)
	{
		string[] path = AdminUtility.GetPath(_Path);
		
		if (path == null || path.Length == 0)
			return;
		
		if (path.Length == 1)
			Data[path[^1]] = _Value;
		
		object data = Data;
		foreach (string node in path[..^1])
		{
			if (data == null)
				return;
			
			if (data is IDictionary<string, object> dataObject)
			{
				if (!dataObject.ContainsKey(node))
					return;
				
				data = dataObject[node];
				
				continue;
			}
			
			if (data is IList<object> dataArray)
			{
				if (!AdminUtility.TryGetIndex(node, out int index))
					return;
				
				data = dataArray[index];
				
				continue;
			}
			
			return;
		}
		
		string name = AdminUtility.GetName(_Path);
		
		if (data is IDictionary<string, object> itemObject)
			itemObject[name] = _Value;
		else if (data is IList<object> itemArray && AdminUtility.TryGetIndex(name, out int index))
			itemArray.Insert(index, _Value);
	}

	public void RemoveValue(string _Path)
	{
		string[] path = AdminUtility.GetPath(_Path);
		
		if (path == null || path.Length == 0)
			return;
		
		if (path.Length == 1)
			Data.Remove(path[^1]);
		
		IDictionary<string, object> data = Data;
		foreach (string node in path[..^1])
		{
			if (data == null)
				return;
			
			data = data[node] as IDictionary<string, object>;
		}
		
		if (data == null)
			return;
		
		data.Remove(path[^1]);
	}

	public abstract Task LoadAsync();

	public abstract Task UploadAsync();

	protected void ProcessNodes()
	{
		if (Root == null)
			Root = new AdminObjectNode(this, null, string.Empty);
		
		ProcessNode(Root, Data, string.Empty);
		
		Root.Create();
	}

	void ProcessNode(AdminNode _Node, object _Data, string _Path)
	{
		if (_Data is IDictionary<string, object> dataObject)
		{
			foreach (KeyValuePair<string, object> entry in dataObject)
			{
				string path = $"{_Path}/{entry.Key}";
				
				AdminNode node = _Node.GetNode(path);
				
				if (node == null)
					node = CreateObject(_Node, path);
				
				if (node == null)
					node = AdminNode.Parse(this, _Node, path);
				
				ProcessNode(node, entry.Value, path);
			}
		}
		else if (_Data is IList<object> dataArray)
		{
			for (int i = 0; i < dataArray.Count; i++)
			{
				object entry = dataArray[i];
				
				string path = $"{_Path}/[{i}]";
				
				AdminNode node = _Node.GetNode(path);
				
				if (node == null)
					node = CreateObject(_Node, path);
				
				if (node == null)
					node = AdminNode.Parse(this, _Node, path);
				
				ProcessNode(node, entry, path);
			}
		}
	}

	public override string ToString() => Data != null ? MiniJson.JsonEncode(Data) : "null";
}
