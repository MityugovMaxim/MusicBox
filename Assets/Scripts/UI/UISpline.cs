using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UISpline : UIEntity, IEnumerable<UISpline.Point>
{
	#region nested types

	[Serializable]
	public struct Key
	{
		#region properties

		public Vector2 Position
		{
			get => m_Position;
			set => m_Position = value;
		}

		public Vector2 Anchor
		{
			get => m_Anchor;
			set => m_Anchor = value;
		}

		public Vector2 InTangent
		{
			get => m_InTangent;
			set => m_InTangent = value;
		}

		public Vector2 OutTangent
		{
			get => m_OutTangent;
			set => m_OutTangent = value;
		}

		#endregion

		#region attributes

		[SerializeField] Vector2 m_Position;
		[SerializeField] Vector2 m_Anchor;
		[SerializeField] Vector2 m_InTangent;
		[SerializeField] Vector2 m_OutTangent;

		public Vector2 GetPosition(Rect _Rect)
		{
			return Vector2.Scale(_Rect.size, Anchor) + Position;
		}

		#endregion
	}

	public struct Point
	{
		#region properties

		public Vector2 Position { get; }

		public Vector2 Normal { get; }

		public float Phase { get; }

		#endregion

		#region attributes

		#endregion

		#region constructor

		public Point(Vector2 _Position, Vector2 _Normal, float _Phase)
		{
			Position = _Position;
			Normal   = _Normal;
			Phase    = _Phase;
		}

		#endregion
	}

	#endregion

	#region properties

	public Point this[int _Index] => m_Points[_Index];

	public int Length => m_Points != null ? m_Points.Count : 0;

	public int Samples
	{
		get => m_Samples;
		set
		{
			if (m_Samples == value)
				return;
			
			m_Samples = value;
			
			SetSplineDirty();
		}
	}

	public bool Loop
	{
		get => m_Loop;
		set
		{
			if (m_Loop == value)
				return;
			
			m_Loop = value;
			
			SetSplineDirty();
		}
	}

	public bool Uniform
	{
		get => m_Uniform;
		set
		{
			if (m_Uniform == value)
				return;
			
			m_Uniform = value;
			
			SetSplineDirty();
		}
	}

	public bool Optimize
	{
		get => m_Optimize;
		set
		{
			if (m_Optimize == value)
				return;
			
			m_Optimize = value;
			
			SetSplineDirty();
		}
	}

	public float Threshold
	{
		get => m_Threshold;
		set
		{
			if (Mathf.Approximately(m_Threshold, value))
				return;
			
			m_Threshold = value;
			
			SetSplineDirty();
		}
	}

	#endregion

	#region attributes

	public event Action OnRebuild;

	[SerializeField]                  bool      m_AutoRebuild = true;
	[SerializeField, HideInInspector] List<Key> m_Keys        = new List<Key>();
	[SerializeField, HideInInspector] int       m_Samples     = 1;
	[SerializeField, HideInInspector] bool      m_Loop        = false;
	[SerializeField, HideInInspector] bool      m_Uniform     = false;
	[SerializeField, HideInInspector] bool      m_Optimize    = false;
	[SerializeField, HideInInspector] float     m_Threshold   = 10;

	bool m_SplineDirty;

	readonly List<Point>   m_Points          = new List<Point>();
	readonly List<Vector2> m_PositionsBuffer = new List<Vector2>();
	readonly List<Vector2> m_NormalsBuffer   = new List<Vector2>();
	readonly List<float>   m_LUT             = new List<float>();

	#endregion

	#region engine methods

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		SetSplineDirty();
	}
	#endif

	protected override void OnEnable()
	{
		base.OnEnable();
		
		if (m_AutoRebuild)
			Rebuild();
	}

	void LateUpdate()
	{
		if (m_SplineDirty)
		{
			m_SplineDirty = false;
			
			Rebuild();
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		SetSplineDirty();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		
		SetSplineDirty();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		SetSplineDirty();
	}

	#endregion

	#region public methods

	public Key GetKey(int _Index)
	{
		return m_Keys[_Index];
	}

	public void SetKey(int _Index, Key _Key)
	{
		m_Keys[_Index] = _Key;

		SetSplineDirty();
	}

	public void AddKeys(ICollection<Key> _Keys)
	{
		m_Keys.AddRange(_Keys);
		
		SetSplineDirty();
	}

	public void ClearKeys()
	{
		m_Keys.Clear();
		
		SetSplineDirty();
	}

	public void AddKey(Key _Key)
	{
		m_Keys.Add(_Key);
		
		SetSplineDirty();
	}

	public void InsertKey(int _Index, Key _Key)
	{
		m_Keys.Insert(_Index, _Key);
		
		SetSplineDirty();
	}

	public void RemoveKey(Key _Key)
	{
		m_Keys.Remove(_Key);
		
		SetSplineDirty();
	}

	public void RemoveKeyAt(int _Index)
	{
		m_Keys.RemoveAt(_Index);
		
		SetSplineDirty();
	}

	public int GetKeysCount()
	{
		return m_Keys != null ? m_Keys.Count : 0;
	}

	public Key[] GetKeys()
	{
		return m_Keys != null ? m_Keys.ToArray() : null;
	}

	public int GetKeysNonAlloc(Key[] _Keys)
	{
		if (m_Keys == null || _Keys == null)
			return 0;
		
		int count = Mathf.Min(m_Keys.Count, _Keys.Length);
		
		m_Keys.CopyTo(0, _Keys, 0, count);
		
		return count;
	}

	public float CalcLength(int _Samples)
	{
		int samples = Samples;
		
		m_Samples = _Samples;
		
		GenerateLUT();
		
		if (Loop)
			GenerateLoopPoints();
		else
			GenerateStraightPoints();
		
		m_Samples = samples;
		
		return GetLength(1);
	}

	public float GetLength(float _Phase)
	{
		if (m_LUT.Count == 0)
			return 0;
		
		return _Phase * m_LUT[m_LUT.Count - 1];
	}

	public Vector2 Evaluate(float _Phase)
	{
		return Uniform ? GetUniformPosition(_Phase) : GetPosition(_Phase);
	}

	public Point GetPoint(float _Phase)
	{
		if (m_Points == null || m_Points.Count == 0)
			return default;
		
		int i = 0;
		int j = m_Points.Count - 1;
		if (m_Points.Count > 2)
		{
			while (i < j)
			{
				int k = i + (j - i) / 2;
				
				Point value = m_Points[k];
				
				if (Mathf.Approximately(value.Phase, _Phase))
				{
					i = j = k;
					break;
				}
				
				if (value.Phase > _Phase)
					j = k;
				else
					i = k;
				
				if (j - i <= 1)
					break;
			}
		}
		
		Point source = m_Points[i];
		Point target = m_Points[j];
		
		float phase = Mathf.InverseLerp(source.Phase, target.Phase, _Phase);
		
		return new Point(
			Vector2.Lerp(source.Position, target.Position, phase),
			Vector2.Lerp(source.Normal, target.Normal, phase).normalized,
			Mathf.Lerp(source.Phase, target.Phase, phase)
		);
	}

	public void Fill(IEnumerable<Vector2> _Positions)
	{
		ClearKeys();
		
		foreach (Vector2 position in _Positions)
		{
			Key key = new Key();
			key.Position   = position;
			key.InTangent  = Vector2.zero;
			key.OutTangent = Vector2.zero;
			
			AddKey(key);
		}
		
		m_SplineDirty = true;
	}

	public void Smooth(float _Weight)
	{
		for (int i = 1; i < m_Keys.Count; i++)
		{
			Key source = m_Keys[i - 1];
			Key target = m_Keys[i];
			
			float tangent = Mathf.Abs(target.Position.y - source.Position.y) * _Weight;
			
			source.OutTangent = new Vector2(0, tangent);
			target.InTangent  = new Vector2(0, -tangent);
			
			m_Keys[i - 1] = source;
			m_Keys[i]     = target;
		}
		
		m_SplineDirty = true;
	}

	public void Resample(float _SamplesPerUnit)
	{
		float length = CalcLength(25);
		
		Samples = Mathf.Max(4, Mathf.CeilToInt(length * _SamplesPerUnit));
		
		Rebuild();
	}

	void SetSplineDirty()
	{
		m_SplineDirty = m_AutoRebuild;
	}

	[ContextMenu("Rebuild")]
	public void Rebuild()
	{
		m_SplineDirty = false;
		
		GenerateLUT();
		
		if (Loop)
			GenerateLoopPoints();
		else
			GenerateStraightPoints();
		
		if (Optimize)
		{
			float threshold = Threshold * Threshold;
			for (int i = m_Points.Count - 2; i > 0; i--)
			{
				Point l = m_Points[i - 1];
				Point c = m_Points[i];
				Point r = m_Points[i + 1];
				
				Vector2 cl = l.Position - c.Position;
				Vector2 cr = r.Position - c.Position;
				
				Vector2 direction = c.Position - r.Position;
				
				float value    = Vector2.Dot(cl.normalized, cr.normalized);
				float distance = direction.sqrMagnitude;
				
				if (Mathf.Approximately(value, -1) && distance < threshold)
					m_Points.RemoveAt(i);
			}
		}
		
		OnRebuild?.Invoke();
	}

	#endregion

	#region service methods

	void GenerateLUT()
	{
		m_LUT.Clear();
		
		if (m_Keys == null || m_Keys.Count < 2)
			return;
		
		int samples = Mathf.Max(Loop ? 2 : 1, Samples);
		
		float step   = 1.0f / samples;
		float length = 0;
		for (int i = 0; i <= samples; i++)
		{
			float sourcePhase = step * Mathf.Max(0, i - 1);
			float targetPhase = step * i;
			
			Vector2 sourcePosition = GetPosition(sourcePhase);
			Vector2 targetPosition = GetPosition(targetPhase);
			
			length += Vector2.Distance(targetPosition, sourcePosition);
			
			m_LUT.Add(length);
		}
	}

	void GenerateLoopPoints()
	{
		m_Points.Clear();
		m_PositionsBuffer.Clear();
		m_NormalsBuffer.Clear();
		
		if (m_Keys == null || m_Keys.Count < 2)
			return;
		
		int   samples = Mathf.Max(2, Samples);
		float step    = 1.0f / samples;
		for (int i = 0; i < samples; i++)
		{
			Vector2 position = Uniform
				? GetUniformPosition(step * i)
				: GetPosition(step * i);
			
			m_PositionsBuffer.Add(position);
		}
		
		// process first point
		Vector2 firstSource    = m_PositionsBuffer[m_PositionsBuffer.Count - 1];
		Vector2 firstTarget    = m_PositionsBuffer[1];
		Vector2 firstDirection = firstTarget - firstSource;
		Vector2 firstNormal    = new Vector2(firstDirection.y, -firstDirection.x).normalized;
		m_NormalsBuffer.Add(firstNormal);
		
		for (int i = 1; i < m_PositionsBuffer.Count; i++)
		{
			Vector2 source = m_PositionsBuffer[i - 1];
			Vector2 target = m_PositionsBuffer[(i + 1) % m_PositionsBuffer.Count];
			
			Vector2 direction = target - source;
			Vector2 normal    = new Vector2(direction.y, -direction.x).normalized;
			
			m_NormalsBuffer.Insert(i, normal);
		}
		
		int count = Mathf.Min(m_PositionsBuffer.Count, m_NormalsBuffer.Count);
		
		for (int i = 0; i < count; i++)
		{
			Vector2 position = m_PositionsBuffer[i];
			Vector2 normal   = m_NormalsBuffer[i];
			float   phase    = 1.0f / count * i;
			
			m_Points.Add(new Point(position, normal, phase));
		}
		
		m_Points.Add(new Point(m_PositionsBuffer[0], m_NormalsBuffer[0], 1));
	}

	void GenerateStraightPoints()
	{
		m_Points.Clear();
		m_PositionsBuffer.Clear();
		m_NormalsBuffer.Clear();
		
		if (m_Keys == null || m_Keys.Count < 2)
			return;
		
		int   samples = Mathf.Max(1, Samples);
		float step    = 1.0f / samples;
		for (int i = 0; i <= samples; i++)
		{
			Vector2 position = Uniform
				? GetUniformPosition(step * i)
				: GetPosition(step * i);
			
			m_PositionsBuffer.Add(position);
		}
		
		Vector2 firstPosition  = m_PositionsBuffer[0];
		Vector2 firstTarget    = m_PositionsBuffer[1];
		Vector2 firstDirection = firstTarget - firstPosition;
		Vector2 firstNormal    = new Vector2(firstDirection.y, -firstDirection.x).normalized;
		m_NormalsBuffer.Add(firstNormal);
		
		for (int i = 1; i < m_PositionsBuffer.Count - 1; i++)
		{
			Vector2 source = m_PositionsBuffer[i - 1];
			Vector2 target = m_PositionsBuffer[(i + 1) % m_PositionsBuffer.Count];
			
			Vector2 direction = target - source;
			
			Vector2 normal = new Vector2(direction.y, -direction.x).normalized;
			
			m_NormalsBuffer.Add(normal);
		}
		
		Vector2 lastPosition  = m_PositionsBuffer[m_PositionsBuffer.Count - 1];
		Vector2 lastTarget    = m_PositionsBuffer[m_PositionsBuffer.Count - 2];
		Vector2 lastDirection = lastPosition - lastTarget;
		Vector2 lastNormal    = new Vector2(lastDirection.y, -lastDirection.x).normalized;
		m_NormalsBuffer.Add(lastNormal);
		
		int count = Mathf.Min(m_PositionsBuffer.Count, m_NormalsBuffer.Count);
		
		for (int i = 0; i < count; i++)
		{
			Vector2 position = m_PositionsBuffer[i];
			Vector2 normal   = m_NormalsBuffer[i];
			float   phase    = 1.0f / (count - 1) * i;
			
			m_Points.Add(new Point(position, normal, phase));
		}
	}

	Vector2 GetPosition(float _Phase)
	{
		if (m_Keys == null || m_Keys.Count == 0)
			return Vector2.zero;
		
		float step;
		int   index;
		
		if (Loop)
		{
			step  = 1.0f / m_Keys.Count;
			index = Mathf.Min(Mathf.FloorToInt(_Phase * m_Keys.Count), m_Keys.Count - 1);
		}
		else
		{
			step  = 1.0f / (m_Keys.Count - 1);
			index = Mathf.FloorToInt(_Phase * (m_Keys.Count - 1));
		}
		
		if (index < 0 || index >= m_Keys.Count)
			return Vector2.zero;
		
		Key source = m_Keys[index];
		Key target = m_Keys[(index + 1) % m_Keys.Count];
		
		float phase = step * index;
		
		Rect    rect           = RectTransform.rect;
		Vector2 sourcePosition = source.GetPosition(rect);
		Vector2 targetPosition = target.GetPosition(rect);
		
		return Lerp(
			sourcePosition,
			sourcePosition + source.OutTangent,
			targetPosition + target.InTangent,
			targetPosition,
			Mathf.InverseLerp(phase, phase + step, _Phase)
		);
	}

	Vector2 GetUniformPosition(float _Phase)
	{
		if (m_LUT == null || m_LUT.Count == 0)
			return GetPosition(_Phase);
		
		float length = _Phase * m_LUT[^1];
		
		int i = 0;
		int j = m_LUT.Count - 1;
		if (m_LUT.Count > 2)
		{
			while (i < j)
			{
				int k = i + (j - i) / 2;
				
				float value = m_LUT[k];
				
				if (Mathf.Approximately(value, length))
				{
					i = j = k;
					break;
				}
				
				if (value > length)
					j = k;
				else
					i = k;
				
				if (j - i <= 1)
					break;
			}
		}
		
		float step        = 1.0f / (m_LUT.Count - 1);
		float sourcePhase = step * i;
		float targetPhase = step * j;
		
		float sourceLength = m_LUT[i];
		float targetLength = m_LUT[j];
		
		float phase = Mathf.Lerp(
			sourcePhase,
			targetPhase,
			Mathf.InverseLerp(sourceLength, targetLength, length)
		);
		
		return GetPosition(phase);
	}

	static Vector2 Lerp(
		Vector2 _SourcePosition,
		Vector2 _SourceTangent,
		Vector2 _TargetPosition,
		Vector2 _TargetTangent,
		float   _Phase
	)
	{
		float p1 = Mathf.Clamp01(_Phase);
		float p2 = p1 * p1;
		float p3 = p2 * p1;
		
		float v1 = 1 - p1;
		float v2 = v1 * v1;
		float v3 = v2 * v1;
		
		return v3 * _SourcePosition + _SourceTangent * (v2 * p1 * 3) + _TargetPosition * (v1 * p2 * 3) + p3 * _TargetTangent;
	}

	#endregion

	#region IEnumerable implementation

	public IEnumerator<Point> GetEnumerator()
	{
		return m_Points.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion
}
