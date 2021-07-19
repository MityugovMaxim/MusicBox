using UnityEngine;

public class FPSProcessor : MonoBehaviour
{
	[SerializeField] int m_FPS = 0;

	void Awake()
	{
		Application.targetFrameRate = m_FPS;
	}
}