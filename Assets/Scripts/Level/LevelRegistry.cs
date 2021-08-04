using UnityEngine;

[CreateAssetMenu(fileName = "Level Registry", menuName = "Registry/Level Registry")]
public class LevelRegistry : Registry<LevelInfo>
{
	public override string Name => "Levels";
}