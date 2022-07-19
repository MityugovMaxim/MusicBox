#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EditorFileManager : IFileManager
{
	public Task<string> SelectFile(string _Extension, CancellationToken _Token = default)
	{
		if (_Token.IsCancellationRequested)
			return Task.FromResult<string>(null);
		
		string path = UnityEditor.EditorUtility.OpenFilePanel("Select file", Application.dataPath, _Extension);
		
		return Task.FromResult(path);
	}
}
#endif