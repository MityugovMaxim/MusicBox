using System;

namespace OggVorbisEncoder
{
	public static class Encoding
	{
		const int Man     = 21;
		const int ExpBias = 768; // bias toward values smaller than 1.

		public static int Log(int _Value)
		{
			int ret;

			for (ret = 0; _Value != 0; ret++)
				_Value >>= 1;

			return ret;
		}

		public static float UnpackFloat(int _Value)
		{
			double mant = _Value & 0x1fffff;
			var    sign = _Value & 0x80000000;
			var    exp  = (int)((_Value & 0x7fe00000L) >> Man);

			if (sign != 0)
				mant = -mant;

			return (float)(mant * Math.Pow(2, exp - (Man - 1) - ExpBias));
		}

		//given a list of word lengths, generate a list of codewords.  Works
		//  for length ordered or unordered, always assigns the lowest valued
		//  codewords first.  Extended to handle unused entries (length 0) 
		public static uint[] MakeWords(byte[] _L, int _Sparsecount)
		{
			var count  = 0;
			var n      = _L.Length;
			var marker = new uint[33];

			var r = new uint[_Sparsecount != 0 ? _Sparsecount : n];

			for (var i = 0; i < n; i++)
			{
				int length = _L[i];
				if (length > 0)
				{
					var entry = marker[length];

					// when we claim a node for an entry, we also claim the nodes
					// below it (pruning off the imagined tree that may have dangled
					// from it) as well as blocking the use of any nodes directly
					// above for leaves 
					if (length < 32 && entry >> length != 0)
						return null;

					r[count++] = entry;

					// Look to see if the next shorter marker points to the node
					// above. if so, update it and repeat.  
					{
						for (var j = length; j > 0; j--)
						{
							if ((marker[j] & 1) != 0)
							{
								// have to jump branches 
								if (j == 1)
									marker[1]++;
								else
									marker[j] = marker[j - 1] << 1;
								break;
								// invariant says next upper marker would already have been moved if it was on the same path
							}
							marker[j]++;
						}
					}

					// prune the tree; the implicit invariant says all the longer
					// markers were dangling from our just-taken node.  Dangle them
					// from our *new* node. 
					for (var j = length + 1; j < 33; j++)
						if (marker[j] >> 1 == entry)
						{
							entry     = marker[j];
							marker[j] = marker[j - 1] << 1;
						}
						else
						{
							break;
						}
				}
				else if (_Sparsecount == 0)
				{
					count++;
				}
			}

			// any underpopulated tree must be rejected. 
			// Single-entry codebooks are a retconned extension to the spec.
			// They have a single codeword '0' of length 1 that results in an
			// underpopulated tree.  Shield that case from the underformed tree check. 
			if (!(count == 1 && marker[2] == 2))
				for (var i = 1; i < 33; i++)
					if ((marker[i] & (0xffffffffUL >> (32 - i))) != 0)
						return null;

			// bitreverse the words because our bitwise packer/unpacker is LSb endian 
			count = 0;
			for (var i = 0; i < n; i++)
			{
				uint temp = 0;
				for (var j = 0; j < _L[i]; j++)
				{
					temp <<= 1;
					temp |=  (r[count] >> j) & 1;
				}

				if (_Sparsecount != 0)
				{
					if (_L[i] != 0)
						r[count++] = temp;
				}
				else
				{
					r[count++] = temp;
				}
			}

			return r;
		}
	}
}