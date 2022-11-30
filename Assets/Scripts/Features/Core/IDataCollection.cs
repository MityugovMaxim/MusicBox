using System.Threading.Tasks;

public interface IDataCollection
{
	bool Loaded { get; }

	DataPriority Priority { get; }

	Task Load();

	Task Reload();

	void Unload();
}
