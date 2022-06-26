using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class UIIndicatorFX : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIIndicatorFX> { }

	[Serializable]
	public class FX
	{
		public ScoreGrade Grade => m_Grade;

		[SerializeField] ScoreGrade m_Grade;
		[SerializeField] GameObject m_FX;
		[SerializeField] float      m_Duration;

		public async Task PlayAsync()
		{
			m_FX.SetActive(true);
			
			await UnityTask.Delay(m_Duration);
			
			m_FX.SetActive(false);
		}
	}

	public ScoreType Type => m_Type;

	[SerializeField, NonReorderable] FX[]      m_Grades;
	[SerializeField]                 ScoreType m_Type;

	[Inject] ScoreManager m_ScoreManager;

	public Task PlayAsync(float _Progress)
	{
		ScoreGrade grade = m_ScoreManager.GetGrade(m_Type, _Progress);
		
		FX fx = m_Grades.FirstOrDefault(_FX => _FX.Grade == grade);
		
		return fx != null ? fx.PlayAsync() : Task.CompletedTask;
	}
}