using System;
using UnityEngine;

public class UIConstraint : UIEntity
{
	public enum ConstraintAlignment
	{
		Left   = 0,
		Right  = 1,
		Top    = 2,
		Bottom = 3,
	}

	[Serializable]
	public class Constraint
	{
		public RectTransform       Transform => m_Transform;
		public ConstraintAlignment Source    => m_Source;
		public ConstraintAlignment Target    => m_Target;

		public bool Valid
		{
			get
			{
				if (Source == Target)
					return true;
				
				if (Source == ConstraintAlignment.Left && Target == ConstraintAlignment.Right)
					return true;
				
				if (Source == ConstraintAlignment.Right && Target == ConstraintAlignment.Left)
					return true;
				
				if (Source == ConstraintAlignment.Top && Target == ConstraintAlignment.Bottom)
					return true;
				
				if (Source == ConstraintAlignment.Bottom && Target == ConstraintAlignment.Bottom)
					return true;
				
				return false;
			}
		}

		[SerializeField] RectTransform       m_Transform;
		[SerializeField] ConstraintAlignment m_Source;
		[SerializeField] ConstraintAlignment m_Target;
	}

	[SerializeField] Constraint[] m_Constraints;

	protected override void Awake()
	{
		base.Awake();
		
		Resolve();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying || !gameObject.scene.isLoaded)
			return;
		
		Resolve();
	}
	#endif

	void Resolve()
	{
		foreach (Constraint constraint in m_Constraints)
			Resolve(constraint);
	}

	void Resolve(Constraint _Constraint)
	{
		if (!_Constraint.Valid)
		{
			Debug.LogError("[UIConstraint] Resolve failed. Invalid constraint.");
			return;
		}
		
		Rect source = GetWorldRect();
		Rect target = _Constraint.Transform.GetWorldRect();
		
		float leftTarget  = source.xMin;
		if (_Constraint.Source == ConstraintAlignment.Left)
		{
			if (_Constraint.Target == ConstraintAlignment.Left)
				leftTarget  = target.xMin;
			else if (_Constraint.Target == ConstraintAlignment.Right)
				leftTarget  = target.xMax;
		}
		
		float rightTarget = source.xMax;
		if (_Constraint.Source == ConstraintAlignment.Right)
		{
			if (_Constraint.Target == ConstraintAlignment.Left)
				rightTarget = target.xMin;
			else if (_Constraint.Target == ConstraintAlignment.Right)
				rightTarget = target.xMax;
		}
		
		float topTarget = source.yMax;
		if (_Constraint.Source == ConstraintAlignment.Top)
		{
			if (_Constraint.Target == ConstraintAlignment.Top)
				topTarget = target.yMax;
			else if (_Constraint.Target == ConstraintAlignment.Bottom)
				topTarget = target.yMin;
		}
		
		float bottomTarget = source.yMin;
		if (_Constraint.Source == ConstraintAlignment.Bottom)
		{
			if (_Constraint.Target == ConstraintAlignment.Top)
				bottomTarget = target.yMax;
			else if (_Constraint.Target == ConstraintAlignment.Bottom)
				bottomTarget = target.yMin;
		}
		
		float left   = leftTarget - source.xMin;
		float right  = rightTarget - source.xMax;
		float top    = topTarget - source.yMax;
		float bottom = bottomTarget - source.yMin;
		
		Vector2 min = new Vector2(left, bottom);
		Vector2 max = new Vector2(right, top);
		
		min = RectTransform.InverseTransformVector(min);
		max = RectTransform.InverseTransformVector(max);
		
		RectTransform.offsetMin += min;
		RectTransform.offsetMax += max;
	}
}