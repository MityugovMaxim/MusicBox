using System.Threading.Tasks;

public interface IDataManager
{
	bool Activated { get; }

	Task<bool> Activate();
}
