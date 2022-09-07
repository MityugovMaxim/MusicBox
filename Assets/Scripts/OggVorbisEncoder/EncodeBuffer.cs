using System;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder
{
	public class EncodeBuffer
	{
		const int BufferIncrement = 256;

		static readonly uint[] m_Mask =
		{
			0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000f,
			0x0000001f, 0x0000003f, 0x0000007f, 0x000000ff, 0x000001ff,
			0x000003ff, 0x000007ff, 0x00000fff, 0x00001fff, 0x00003fff,
			0x00007fff, 0x0000ffff, 0x0001ffff, 0x0003ffff, 0x0007ffff,
			0x000fffff, 0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
			0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff, 0x1fffffff,
			0x3fffffff, 0x7fffffff, 0xffffffff
		};

		byte[] m_Buffer;
		int    m_EndBit;
		int    m_EndByte;

		public EncodeBuffer()
			: this(BufferIncrement) { }

		public EncodeBuffer(int _InitialBufferSize)
		{
			m_Buffer = new byte[_InitialBufferSize];
		}

		int Bytes => m_EndByte + (m_EndBit + 7) / 8;

		public void WriteBook(CodeBook _Book, int _A)
		{
			if (_A < 0 || _A >= _Book.Entries)
				return;

			Write(_Book.CodeList[_A], _Book.StaticBook.LengthList[_A]);
		}

		public void WriteString(string _Str)
		{
			foreach (var c in _Str)
				Write(c, 8);
		}

		public void Write(uint _Value, int _Bits)
		{
			if (_Bits < 0 || _Bits > 32)
				throw new ArgumentException($"{nameof(_Bits)} must be between 0 and 32");

			if (m_EndByte >= m_Buffer.Length - 4)
			{
				if (m_Buffer.Length > int.MaxValue - BufferIncrement)
					throw new InvalidOperationException("Maximum buffer size exceeded");

				Array.Resize(ref m_Buffer, m_Buffer.Length + BufferIncrement);
			}

			_Value &= m_Mask[_Bits];
			_Bits  += m_EndBit;

			m_Buffer[m_EndByte] = (byte)(m_Buffer[m_EndByte] | (_Value << m_EndBit));

			if (_Bits >= 8)
			{
				m_Buffer[m_EndByte + 1] = (byte)(_Value >> (8 - m_EndBit));
				if (_Bits >= 16)
				{
					m_Buffer[m_EndByte + 2] = (byte)(_Value >> (16 - m_EndBit));
					if (_Bits >= 24)
					{
						m_Buffer[m_EndByte + 3] = (byte)(_Value >> (24 - m_EndBit));
						if (_Bits >= 32)
							if (m_EndBit != 0)
								m_Buffer[m_EndByte + 4] = (byte)(_Value >> (32 - m_EndBit));
							else
								m_Buffer[m_EndByte + 4] = 0;
					}
				}
			}

			m_EndByte += _Bits / 8;
			m_EndBit  =  _Bits & 7;
		}

		public byte[] GetBytes()
		{
			Array.Resize(ref m_Buffer, Bytes);
			return m_Buffer;
		}
	}
}