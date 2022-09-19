using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class FileManagerUtility
{
	public static readonly string[] MidiExtensions =
	{
		"mid",
		"midi",
	};

	public static readonly string[] AudioExtensions =
	{
		"ogg",
		"oga",
		"ogv",
		"opus",
		"mp3",
		"aac",
		"aiff",
		"wav",
		"wave",
	};

	public static readonly string[] ImageExtensions =
	{
		"png",
		"pdf",
		"jpg",
		"jpeg",
		"webp",
	};
}

public interface IFileManager
{
	Task<string> SelectFile(string[] _Extensions, CancellationToken _Token = default);
}