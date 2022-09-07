using System;

namespace OggVorbisEncoder
{
	public class OffsetMemory<T>
	{
		private readonly Memory<T> m_Memory;

		public OffsetMemory(in Memory<T> _Memory, int _Offset)
		{
			m_Memory = _Memory;
			Offset   = _Offset;
		}

		public int Offset { get; }

		public T this[int _Index]
		{
			get { return m_Memory.Span[_Index]; }
		}
	}
}