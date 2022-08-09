using System.Threading.Tasks;
using Zenject;

public interface IAsyncFactory<T> : IFactory
{
	Task<T> Create();
}

public interface IAsyncFactory<T0, T1> : IFactory
{
	Task<T1> Create(T0 _Param);
}