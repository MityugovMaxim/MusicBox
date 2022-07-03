using UnityEngine;

public class UISpectrumShader : UISpectrum
{
	static readonly int m_SpectrumPropertyID = Shader.PropertyToID("_Spectrum");

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		Shader.SetGlobalFloatArray(m_SpectrumPropertyID, _Amplitude);
	}
}