using Firebase.Database;

public class ProfileChest
{
	public int Count    { get; }
	public int Progress { get; }

	public ProfileChest(DataSnapshot _Data)
	{
		Count    = _Data.GetInt("count");
		Progress = _Data.GetInt("progress");
	}
}

public abstract class ProfileChests : ProfileParameter<ProfileChest>, IDataObject
{
	public int Count => Value?.Count ?? 0;

	public int Progress => Value?.Progress ?? 0;

	protected abstract RankType Rank { get; }

	protected override string Name => $"chests/{Rank.ToString().ToLowerInvariant()}";

	protected override ProfileChest Create(DataSnapshot _Data) => new ProfileChest(_Data);
}

public class ProfileBronzeChests : ProfileChests
{
	protected override RankType Rank => RankType.Bronze;
}

public class ProfileSilverChests : ProfileChests
{
	protected override RankType Rank => RankType.Silver;
}

public class ProfileGoldChests : ProfileChests
{
	protected override RankType Rank => RankType.Gold;
}

public class ProfilePlatinumChests : ProfileChests
{
	protected override RankType Rank => RankType.Platinum;
}
