using UnityEngine;
using UnityEngine.Playables;

public class Player : MonoBehaviour
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			GetComponent<PlayableDirector>().Stop();
			GetComponent<PlayableDirector>().Play();
		}
	}
}