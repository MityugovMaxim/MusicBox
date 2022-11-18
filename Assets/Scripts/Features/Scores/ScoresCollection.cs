using UnityEngine.Scripting;

[Preserve]
public class ScoresCollection : ProfileCollection<ScoreSnapshot>
{
	protected override string Name => "scores";
}
