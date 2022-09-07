using System;

namespace OggVorbisEncoder
{
	internal static class ArrayExtensions
	{
		public static TElement[] ToFixedLength<TElement>(this TElement[] _Input, int _FixedLength)
		{
			if (_Input == null)
				throw new ArgumentNullException(nameof(_Input));

			if (_Input.Length == _FixedLength)
				return _Input;

			if (_Input.Length > _FixedLength)
				throw new IndexOutOfRangeException(
					$"{nameof(_Input)} of size [{_Input.Length}] is greater than {nameof(_FixedLength)} of [{_FixedLength}]"
				);

			var output = new TElement[_FixedLength];
			Array.Copy(_Input, output, _Input.Length);

			return output;
		}
	}
}