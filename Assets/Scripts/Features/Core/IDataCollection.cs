using System.Threading.Tasks;

public interface IDataCollection
{
	bool Loaded { get; }

	DataCollectionPriority Priority { get; }

	Task Load();

	Task Reload();

	void Unload();
}
