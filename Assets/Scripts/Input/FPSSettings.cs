using UnityEngine;

public class FPSSettings : MonoBehaviour
{
	void Awake()
	{
		Application.targetFrameRate = 60;
	}
}