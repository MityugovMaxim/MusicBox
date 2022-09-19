#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EditorFileManager : IFileManager
{
	public Task<string> SelectFile(string[] _Extensions, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(
			"Select file",
			Application.dataPath,
			new string[] { "AllFiles", string.Join(',', _Extensions) }
		);
		
		return Task.FromResult(path);
	}
}
#endif