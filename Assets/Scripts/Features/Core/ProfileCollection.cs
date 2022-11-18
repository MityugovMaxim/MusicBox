using Firebase.Database;
using Zenject;

public abstract class ProfileCollection<TValue> : DataCollection<DataSnapshot<TValue>>
{
	protected abstract string Name { get; }
	protected override string Path => $"profiles/{m_SocialProcessor.UserID}/{Name}";

	[Inject] SocialProcessor m_SocialProcessor;

	protected override DataSnapshot<TValue> Create(DataSnapshot _Data) => new DataSnapshot<TValue>(_Data);

	public new TValue GetSnapshot(string _ID)
	{
		DataSnapshot<TValue> snapshot = base.GetSnapshot(_ID);
		
		return snapshot != null ? snapshot.Value : default;
	}
}
