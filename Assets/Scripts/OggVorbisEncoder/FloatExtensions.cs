using System.Runtime.InteropServices;

namespace OggVorbisEncoder
{
	public static class FloatExtensions
	{
		public static float ToDecibel(this float _Value)
		{
			var i = Converter.PackFloat(_Value);
			i &= 0x7fffffff;
			return i * 7.17711438e-7f - 764.6161886f;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct Converter
	{
		[FieldOffset(0)]          float m_FloatValue;
		[FieldOffset(0)] readonly uint  m_UIntValue;

		public static uint PackFloat(float _Value)
		{
			var converter = new Converter();
			converter.m_FloatValue = _Value;
			return converter.m_UIntValue;
		}
	}
}