using Firebase.Database;
using Zenject;

public abstract class ProfileCollection<TSnapshot> : DataCollection<TSnapshot> where TSnapshot : Snapshot
{
	protected abstract string Name { get; }
	protected override string Path => $"profiles/{m_SocialProcessor.UserID}/{Name}";

	[Inject] SocialProcessor m_SocialProcessor;
}
