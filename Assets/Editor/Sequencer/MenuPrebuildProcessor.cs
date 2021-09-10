using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class MenuPrebuildProcessor : IPreprocessBuildWithReport
{
	public int callbackOrder => 0;

	public void OnPreprocessBuild(BuildReport _Report)
	{
		MenuPrebuild.Generate();
	}
}