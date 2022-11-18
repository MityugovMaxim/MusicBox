using Zenject;

public abstract class ProfileParameter<TValue> : DataObject<TValue>
{
	protected abstract string Name { get; }

	protected override string Path => $"profiles/{m_SocialProcessor.UserID}/{Name}";

	[Inject] SocialProcessor m_SocialProcessor;
}