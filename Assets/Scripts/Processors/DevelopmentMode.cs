using UnityEngine;

public class DevelopmentMode : MonoBehaviour
{
	public static bool Enabled { get; private set; }

	void Awake()
	{
		Enabled = true;
	}
}
