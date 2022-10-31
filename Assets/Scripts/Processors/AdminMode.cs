using UnityEngine;

public class AdminMode : MonoBehaviour
{
	public static bool Enabled { get; private set; }

	void Awake()
	{
		Enabled = true;
	}
}
