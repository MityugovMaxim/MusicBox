using System.Collections;
using Coffee.UIExtensions;
using UnityEngine;

public class ParticlePrewarm : MonoBehaviour
{
	[SerializeField] UIParticle[] m_Particles;

	void Awake()
	{
		StartCoroutine(PrewarmRoutine());
	}

	IEnumerator PrewarmRoutine()
	{
		UIParticle[] particles = new UIParticle[m_Particles.Length];
		
		for (int i = 0; i < m_Particles.Length; i++)
			particles[i] = Instantiate(m_Particles[i]);
		
		yield return null;
		
		foreach (UIParticle particle in particles)
			Destroy(particle.gameObject);
	}
}