public class LevelStartSignal
{
	public string LevelID { get; }

	public LevelStartSignal(string _LevelID)
	{
		LevelID = _LevelID;
	}
}

public class LevelRestartSignal
{
	public string LevelID { get; }

	public LevelRestartSignal(string _LevelID)
	{
		LevelID = _LevelID;
	}
}

public class LevelExitSignal
{
	public string LevelID { get; }

	public LevelExitSignal(string _LevelID)
	{
		LevelID = _LevelID;
	}
}