using System.IO;
using UnityEditor;
using UnityEngine;

public class LineGenerator : EditorWindow
{
	[MenuItem("Tools/Line generator...")]
	public static void Open()
	{
		LineGenerator window = GetWindow<LineGenerator>(true);
		window.titleContent = new GUIContent("Line generator");
		window.minSize      = new Vector2(400, 680);
		window.maxSize      = new Vector2(400, 680);
		window.Show();
	}

	AnimationCurve m_Curve    = AnimationCurve.Linear(0, 1, 1, 1);
	Gradient       m_Gradient = new Gradient();
	Texture2D      m_Line;
	Texture2D      m_Cap;

	void OnEnable()
	{
		GenerateLine();
		GenerateCap();
	}

	void OnGUI()
	{
		EditorGUI.BeginChangeCheck();
		
		m_Curve = EditorGUILayout.CurveField(m_Curve);
		
		m_Gradient = EditorGUILayout.GradientField(m_Gradient);
		
		if (EditorGUI.EndChangeCheck())
		{
			GenerateLine();
			GenerateCap();
		}
		
		Rect capRect  = GUILayoutUtility.GetAspectRect(2);
		
		Rect lineRect = GUILayoutUtility.GetAspectRect(1);
		
		if (m_Cap != null)
			EditorGUI.DrawTextureTransparent(capRect, m_Cap, ScaleMode.ScaleToFit);
		
		if (m_Line != null)
			EditorGUI.DrawTextureTransparent(lineRect, m_Line, ScaleMode.ScaleToFit);
		
		if (GUILayout.Button("Save"))
			Save();
	}

	void GenerateLine()
	{
		m_Line = new Texture2D(256, 512, TextureFormat.ARGB32, false);
		
		for (int y = 0; y < m_Line.height; y++)
		for (int x = 0; x < m_Line.width; x++)
		{
			float phase = Mathf.PingPong((float)x / (m_Line.width - 1) * 2, 1);
			
			float alpha = m_Curve.Evaluate(phase);
			
			Color color = m_Gradient.Evaluate(phase);
			
			color.a *= alpha;
			
			m_Line.SetPixel(x, y, color);
		}
		
		m_Line.Apply();
	}

	void GenerateCap()
	{
		m_Cap = new Texture2D(256, 256, TextureFormat.ARGB32, false);
		
		Vector2 pivot = new Vector2(m_Cap.width * 0.5f, 0);
		
		for (int y = 0; y < m_Cap.height; y++)
		for (int x = 0; x < m_Cap.width; x++)
		{
			Vector2 position = new Vector2(x, y);
			
			float phase = 1 - (pivot - position).magnitude / (m_Cap.width * 0.5f);
			
			float alpha = m_Curve.Evaluate(phase);
			
			Color color = m_Gradient.Evaluate(phase);
			
			color.a *= alpha;
			
			m_Cap.SetPixel(x, y, color);
		}
		
		m_Cap.Apply();
	}

	void Save()
	{
		if (m_Line == null)
			return;
		
		string directory = EditorUtility.OpenFolderPanel("Select folder to save line and cap", Application.dataPath + "Assets", string.Empty);
		
		if (string.IsNullOrEmpty(directory))
			return;
		
		string linePath = Path.Combine(directory, "line.png");
		
		string capPath  = Path.Combine(directory, "cap.png");
		
		byte[] lineData = m_Line.EncodeToPNG();
		
		File.WriteAllBytes(linePath, lineData);
		
		byte[] capData = m_Cap.EncodeToPNG();
		
		File.WriteAllBytes(capPath, capData);
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
}
