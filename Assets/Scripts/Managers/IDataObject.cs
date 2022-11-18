using System.Threading.Tasks;

public interface IDataObject
{
	Task Load();

	Task Reload();

	void Unload();

	T GetValue<T>();
}