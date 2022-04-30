using System.Threading;
using System.Threading.Tasks;

public interface IFileManager
{
	Task<string> SelectFile(string _Extension, CancellationToken _Token = default);
}