using UnityEngine;
using System.Collections.Generic;

public class MeshBuilder
{
	readonly List<Vector2> m_Points;
	readonly List<int>     m_Buffer;

	public MeshBuilder(List<Vector2> _Points)
	{
		m_Points = _Points;
		m_Buffer = new List<int>();
	}

	public void Triangulate(List<int> _Indices)
	{
		_Indices.Clear();
		
		if (m_Points.Count < 3)
			return;
		
		m_Buffer.Clear();
		if (CalcArea() > 0)
		{
			for (int i = 0; i < m_Points.Count; i++)
				m_Buffer.Add(i);
		}
		else
		{
			for (int i = 0; i < m_Points.Count; i++)
				m_Buffer.Add(m_Points.Count - i - 1);
		}
		
		int nv    = m_Points.Count;
		int count = 2 * nv;
		for (int v = nv - 1; nv > 2;)
		{
			if (count-- <= 0)
				return;
			
			int u = v;
			if (nv <= u)
				u = 0;
			v = u + 1;
			if (nv <= v)
				v = 0;
			int w = v + 1;
			if (nv <= w)
				w = 0;
			
			if (!Snip(u, v, w, nv))
				continue;
			
			int a = m_Buffer[u];
			int b = m_Buffer[v];
			int c = m_Buffer[w];
			
			_Indices.Add(a);
			_Indices.Add(b);
			_Indices.Add(c);
			
			for (int s = v, t = v + 1; t < nv; s++, t++)
				m_Buffer[s] = m_Buffer[t];
			
			nv--;
			
			count = 2 * nv;
		}
		
		_Indices.Reverse();
	}

	float CalcArea()
	{
		int   n = m_Points.Count;
		float a = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++)
		{
			Vector2 pValue = m_Points[p];
			Vector2 qValue = m_Points[q];
			a += pValue.x * qValue.y - qValue.x * pValue.y;
		}
		return a * 0.5f;
	}

	bool Snip(int _U, int _V, int _W, int _N)
	{
		Vector2 a = m_Points[m_Buffer[_U]];
		Vector2 b = m_Points[m_Buffer[_V]];
		Vector2 c = m_Points[m_Buffer[_W]];
		if (Mathf.Epsilon > (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x))
			return false;
		
		for (int i = 0; i < _N; i++)
		{
			if (i == _U || i == _V || i == _W)
				continue;
			
			Vector2 point = m_Points[m_Buffer[i]];
			
			if (InsideTriangle(a, b, c, point))
				return false;
		}
		return true;
	}

	static bool InsideTriangle(Vector2 _A, Vector2 _B, Vector2 _C, Vector2 _P)
	{
		float ax  = _C.x - _B.x;
		float ay  = _C.y - _B.y;
		float bx  = _A.x - _C.x;
		float by  = _A.y - _C.y;
		float cx  = _B.x - _A.x;
		float cy  = _B.y - _A.y;
		
		float apx = _P.x - _A.x;
		float apy = _P.y - _A.y;
		float bpx = _P.x - _B.x;
		float bpy = _P.y - _B.y;
		float cpx = _P.x - _C.x;
		float cpy = _P.y - _C.y;
		
		float sbp = ax * bpy - ay * bpx;
		float sap = cx * apy - cy * apx;
		float scp = bx * cpy - by * cpx;
		
		return sbp >= 0.0f && scp >= 0.0f && sap >= 0.0f;
	}
}