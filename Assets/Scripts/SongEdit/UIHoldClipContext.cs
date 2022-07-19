using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

public class UIHoldClipContext : ASFClipContext<ASFHoldClip>, IPointerClickHandler, ICanvasRaycastFilter
{
	[Preserve]
	public class Pool : MonoMemoryPool<RectTransform, ASFHoldClip, Rect, Rect, UIHoldClipContext>
	{
		protected override void Reinitialize(RectTransform _Container, ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect, UIHoldClipContext _Item)
		{
			_Item.Setup(_Container, _Clip, _ClipRect, _ViewRect);
			
			_Item.Select(ClipSelection.Contains(_Item.Clip));
		}

		protected override void OnSpawned(UIHoldClipContext _Item)
		{
			base.OnSpawned(_Item);
			
			ClipSelection.Changed += _Item.OnSelectionChanged;
		}

		protected override void OnDespawned(UIHoldClipContext _Item)
		{
			base.OnDespawned(_Item);
			
			ClipSelection.Changed -= _Item.OnSelectionChanged;
		}
	}

	const int COUNT = 4;

	float Padding => GetLocalRect().width / (COUNT * 2);

	[SerializeField] UISpline   m_Spline;
	[SerializeField] float      m_Weight = 0.25f;
	[SerializeField] GameObject m_Selection;

	[Inject] UIHoldKey.Pool     m_ItemPool;
	[Inject] UICreateHoldHandle m_CreateHoldHandle;

	bool m_Drag;

	readonly List<UIHoldKey> m_Items       = new List<UIHoldKey>();
	readonly ClickCounter    m_CreateKey = new ClickCounter(2);

	public bool ContainsPoint(Vector2 _Position)
	{
		Vector2 position = RectTransform.InverseTransformPoint(_Position);
		
		float distance = m_Spline.GetHorizontalDistance(position);
		
		return distance <= 50;
	}

	public override void Setup(RectTransform _Container, ASFHoldClip _Clip, Rect _ClipRect, Rect _ViewRect)
	{
		base.Setup(_Container, _Clip, _ClipRect, _ViewRect);
		
		m_Spline.ClearKeys();
		
		ProcessKeys();
		
		ProcessTangents();
		
		ProcessHandles();
		
		m_Spline.Rebuild();
	}

	public void Process()
	{
		foreach (UIHoldKey item in m_Items)
			item.Process();
	}

	void RemapKeys()
	{
		Clip.Keys.Sort((_A, _B) => _A.Time.CompareTo(_B.Time));
		
		double min = Clip.Keys.FirstOrDefault()?.Time ?? 0;
		double max = Clip.Keys.LastOrDefault()?.Time ?? 0;
		
		double minDelta = min;
		double maxDelta = max - Clip.Length;
		
		Rect clipRect = ClipRect;
		clipRect.yMin = (float)ASFMath.Remap(min, 0, Clip.Length, ClipRect.yMin, ClipRect.yMax);
		clipRect.yMax = (float)ASFMath.Remap(max, 0, Clip.Length, ClipRect.yMin, ClipRect.yMax);
		
		ClipRect = clipRect;
		ViewRect = new Rect(
			clipRect.x,
			clipRect.y - Padding,
			clipRect.width,
			clipRect.height + Padding * 2
		);
		
		Clip.MinTime += minDelta;
		Clip.MaxTime += maxDelta;
		
		foreach (ASFHoldKey key in Clip.Keys)
		{
			key.Time -= minDelta;
		}
	}

	void ProcessKeys()
	{
		Clip.Keys.Sort((_A, _B) => _A.Time.CompareTo(_B.Time));
		
		foreach (ASFHoldKey key in Clip.Keys)
		{
			Vector2 position = GetKeyPosition(key.Time, key.Position);
			UISpline.Key spline = new UISpline.Key();
			spline.Position   = position;
			spline.InTangent  = Vector2.zero;
			spline.OutTangent = Vector2.zero;
			
			m_Spline.AddKey(spline);
		}
	}

	Vector2 GetKeyPosition(double _Time, float _Position)
	{
		Rect rect = ClipRect.Transform(Container, RectTransform)
			.HorizontalPadding(Padding);
		
		return new Vector2(
			ASFMath.PhaseToPosition(_Position, rect.xMin, rect.xMax),
			ASFMath.TimeToPosition(_Time, 0, Clip.Length, rect.yMin, rect.yMax)
		);
	}

	void ProcessTangents()
	{
		int count = m_Spline.GetKeysCount();
		for (int i = 1; i < count; i++)
		{
			UISpline.Key source = m_Spline.GetKey(i - 1);
			UISpline.Key target = m_Spline.GetKey(i);
			
			float tangent = Mathf.Abs(target.Position.y - source.Position.y) * m_Weight;
			
			source.OutTangent = new Vector2(0, tangent);
			target.InTangent  = new Vector2(0, -tangent);
			
			m_Spline.SetKey(i - 1, source);
			m_Spline.SetKey(i, target);
		}
	}

	void ProcessHandles()
	{
		foreach (UIHoldKey item in m_Items)
			m_ItemPool.Despawn(item);
		m_Items.Clear();
		
		for (int i = 0; i < Clip.Keys.Count; i++)
		{
			ASFHoldKey key      = Clip.Keys[i];
			Vector2    position = GetKeyPosition(key.Time, key.Position);
			float      size     = i == 0 || i == Clip.Keys.Count - 1 ? Padding * 2 : Padding * 1.5f;
			
			UIHoldKey item = m_ItemPool.Spawn(RectTransform, position, size);
			item.Setup(key, Clip, ClipRect, Reposition, Rebuild);
			m_Items.Add(item);
		}
	}

	void Reposition()
	{
		m_Spline.ClearKeys();
		
		ProcessKeys();
		
		ProcessTangents();
		
		m_Spline.Rebuild();
	}

	void Rebuild()
	{
		RemapKeys();
		
		Reposition();
		
		ProcessHandles();
	}

	void Select(bool _Value)
	{
		m_Selection.SetActive(_Value);
	}

	void OnSelectionChanged()
	{
		Select(ClipSelection.Contains(Clip));
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		_EventData.Use();
		
		if (!m_CreateKey.Execute(_EventData))
			return;
		
		Vector2 point = RectTransform.InverseTransformPoint(_EventData.position);
		
		Rect rect = ClipRect.Transform(Container, RectTransform)
			.HorizontalPadding(Padding);
		
		double time     = ASFMath.PositionToTime(point.y, rect.yMin, rect.yMax, 0, Clip.Length);
		float  position = ASFMath.PositionToPhase(point.x, rect.xMin, rect.xMax);
		
		Clip.Keys.Add(new ASFHoldKey(time, position));
		
		Rebuild();
	}

	bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 _Position, Camera _Camera)
	{
		return m_CreateHoldHandle != null && m_CreateHoldHandle.gameObject.activeSelf && ContainsPoint(_Position);
	}
}