using System;
using System.Collections.Generic;
using System.Linq;
using OggVorbisEncoder.Setup;

namespace OggVorbisEncoder.Lookups
{
	public class FloorLookup
	{
		const    int   Posit = 63;
		readonly Floor m_Floor;
		readonly int[] m_ForwardIndex = new int[Posit + 2];
		readonly int[] m_HighNeighbor = new int[Posit];

		readonly int[] m_LowNeighbor = new int[Posit];
		readonly int   m_N;
		readonly int   m_Posts;
		readonly int   m_QuantQ;
		readonly int[] m_ReverseIndex = new int[Posit + 2];
		readonly int[] m_SortedIndex  = new int[Posit + 2];

		public FloorLookup(Floor _Floor)
		{
			m_Floor = _Floor;
			m_N     = _Floor.PostList[1];

			// we drop each position value in-between already decoded values,
			// and use linear interpolation to predict each new value past the
			// edges.  The positions are read in the order of the position
			// list... we precompute the bounding positions in the lookup.  Of
			// course, the neighbors can change (if a position is declined), but
			// this is an initial mapping 
			var n = 0;
			foreach (var partition in _Floor.PartitionClass)
				n += _Floor.ClassDimensions[partition];

			n       += 2;
			m_Posts =  n;

			// also store a sorted position index 
			var sorted = _Floor
				.PostList
				.Select((_V, _I) => new { Value = _V, Index = _I })
				.OrderBy(_O => _O.Value)
				.ToArray();

			// points from sort order back to range number 
			for (var i = 0; i < n; i++)
				m_ForwardIndex[i] = sorted[i].Index;

			// points from range order to sorted position 
			for (var i = 0; i < n; i++)
				m_ReverseIndex[m_ForwardIndex[i]] = i;

			// we actually need the post values too 
			for (var i = 0; i < n; i++)
				m_SortedIndex[i] = _Floor.PostList[m_ForwardIndex[i]];

			// quantize values to multiplier spec 
			switch (_Floor.Mult)
			{
				case 1: // 1024 . 256 
					m_QuantQ = 256;
					break;
				case 2: // 1024 . 128 
					m_QuantQ = 128;
					break;
				case 3: // 1024 . 86 
					m_QuantQ = 86;
					break;
				case 4: // 1024 . 64 
					m_QuantQ = 64;
					break;
			}

			// discover our neighbors for decode where we don't use fit flags
			// (that would push the neighbors outward) 
			for (var i = 0; i < n - 2; i++)
			{
				var lo       = 0;
				var hi       = 1;
				var lx       = 0;
				var hx       = m_N;
				var currentX = _Floor.PostList[i + 2];
				for (var j = 0; j < i + 2; j++)
				{
					var x = _Floor.PostList[j];
					if (x > lx && x < currentX)
					{
						lo = j;
						lx = x;
					}
					if (x < hx && x > currentX)
					{
						hi = j;
						hx = x;
					}
				}
				m_LowNeighbor[i]  = lo;
				m_HighNeighbor[i] = hi;
			}
		}

		public int[] Fit(in Span<float> _Logmdct, float[] _Logmask)
		{
			var n = m_N;

			var                   nonzero   = 0;
			Span<FitAccumulation> fits      = stackalloc FitAccumulation[Posit + 1];
			Span<int>             fitValueA = stackalloc int[Posit + 2]; // index by range list position 
			Span<int>             fitValueB = stackalloc int[Posit + 2]; // index by range list position 

			Span<int> loneighbor = stackalloc int[Posit + 2]; // sorted index of range list position (+2) 
			Span<int> hineighbor = stackalloc int[Posit + 2];
			Span<int> memo       = stackalloc int[Posit + 2];

			for (var i = 0; i < m_Posts; i++)
				fitValueA[i] = -200; // mark all unused 

			for (var i = 0; i < m_Posts; i++)
				fitValueB[i] = -200; // mark all unused 

			for (var i = 0; i < m_Posts; i++)
				loneighbor[i] = 0; // 0 for the implicit 0 post 

			for (var i = 0; i < m_Posts; i++)
				hineighbor[i] = 1; // 1 for the implicit post at n 

			for (var i = 0; i < m_Posts; i++)
				memo[i] = -1; // no neighbor yet 

			// quantize the relevant floor points and collect them into line fit
			// structures (one per minimal division) at the same time 
			if (m_Posts == 0)
				nonzero = AccumulateFit(_Logmask, _Logmdct, 0, n, ref fits[0], n);
			else
				for (var i = 0; i < m_Posts - 1; i++)
					nonzero += AccumulateFit(_Logmask, _Logmdct, m_SortedIndex[i], m_SortedIndex[i + 1], ref fits[i], n);

			if (nonzero != 0)
			{
				// start by fitting the implicit base case.... 
				FitLine(fits, 0, m_Posts - 1, out var y0, out var y1);

				fitValueA[0] = y0;
				fitValueB[0] = y0;
				fitValueB[1] = y1;
				fitValueA[1] = y1;

				// Non degenerate case 
				// start progressive splitting.  This is a greedy, non-optimal
				// algorithm, but simple and close enough to the best
				// answer. 
				for (var i = 2; i < m_Posts; i++)
				{
					var sortPosition = m_ReverseIndex[i];
					var ln           = loneighbor[sortPosition];
					var hn           = hineighbor[sortPosition];

					// eliminate repeat searches of a particular range with a memo 
					if (memo[ln] != hn)
					{
						// haven't performed this error search yet 
						var lowSortPosition  = m_ReverseIndex[ln];
						var highSortPosition = m_ReverseIndex[hn];
						memo[ln] = hn;

						{
							// A note: we want to bound/minimize *local*, not global, error 
							var lx = m_Floor.PostList[ln];
							var hx = m_Floor.PostList[hn];
							var ly = PostY(fitValueA, fitValueB, ln);
							var hy = PostY(fitValueA, fitValueB, hn);

							if (ly == -1 || hy == -1)
								throw new InvalidOperationException("An error occurred during minimization");

							if (InspectError(lx, hx, ly, hy, _Logmask, _Logmdct))
							{
								// outside error bounds/begin search area.  Split it. 
								var ret0 = FitLine(fits, lowSortPosition, sortPosition - lowSortPosition, out var ly0, out var ly1);
								var ret1 = FitLine(fits, sortPosition, highSortPosition - sortPosition, out var hy0, out var hy1);

								if (ret0 != 0)
								{
									ly0 = ly;
									ly1 = hy0;
								}
								if (ret1 != 0)
								{
									hy0 = ly1;
									hy1 = hy;
								}

								if (ret0 != 0 && ret1 != 0)
								{
									fitValueA[i] = -200;
									fitValueB[i] = -200;
								}
								else
								{
									// store new edge values 
									fitValueB[ln] = ly0;
									if (ln == 0) fitValueA[ln] = ly0;
									fitValueA[i]  = ly1;
									fitValueB[i]  = hy0;
									fitValueA[hn] = hy1;
									if (hn == 1) fitValueB[hn] = hy1;

									if (ly1 >= 0 || hy0 >= 0)
									{
										// store new neighbor values 
										for (var j = sortPosition - 1; j >= 0; j--)
											if (hineighbor[j] == hn)
												hineighbor[j] = i;
											else
												break;

										for (var j = sortPosition + 1; j < m_Posts; j++)
											if (loneighbor[j] == ln)
												loneighbor[j] = i;
											else
												break;
									}
								}
							}
							else
							{
								fitValueA[i] = -200;
								fitValueB[i] = -200;
							}
						}
					}
				}

				var output = new int[m_Posts];

				output[0] = PostY(fitValueA, fitValueB, 0);
				output[1] = PostY(fitValueA, fitValueB, 1);

				// fill in posts marked as not using a fit; we will zero
				// back out to 'unused' when encoding them so int as curve
				// interpolation doesn't force them into use 
				for (var i = 2; i < m_Posts; i++)
				{
					var ln = m_LowNeighbor[i - 2];
					var hn = m_HighNeighbor[i - 2];
					var x0 = m_Floor.PostList[ln];
					var x1 = m_Floor.PostList[hn];
					y0 = output[ln];
					y1 = output[hn];

					var predicted = RenderPoint(x0, x1, y0, y1, m_Floor.PostList[i]);
					var vx        = PostY(fitValueA, fitValueB, i);

					if (vx >= 0 && predicted != vx)
						output[i] = vx;
					else
						output[i] = predicted | 0x8000;
				}

				return output;
			}

			return null;
		}

		bool InspectError(int _X0, int _X1, int _Y0, int _Y1, float[] _Mask, in Span<float> _Mdct)
		{
			var dy      = _Y1 - _Y0;
			var adx     = _X1 - _X0;
			var ady     = Math.Abs(dy);
			var baseVal = dy / adx;
			var sy      = dy < 0 ? baseVal - 1 : baseVal + 1;
			var x       = _X0;
			var y       = _Y0;
			var err     = 0;
			var val     = DecibelQuant(_Mask[x]);
			var n       = 0;

			ady -= Math.Abs(baseVal * adx);

			var mse = y - val;
			mse *= mse;
			n++;
			if (_Mdct[x] + m_Floor.TwoFitAtten >= _Mask[x])
			{
				if (y + m_Floor.MaxOver < val)
					return true;

				if (y - m_Floor.MaxUnder > val)
					return true;
			}

			while (++x < _X1)
			{
				err += ady;
				if (err >= adx)
				{
					err -= adx;
					y   += sy;
				}
				else
				{
					y += baseVal;
				}

				val =  DecibelQuant(_Mask[x]);
				mse += (y - val) * (y - val);
				n++;

				if (_Mdct[x] + m_Floor.TwoFitAtten >= _Mask[x])
					if (val != 0)
					{
						if (y + m_Floor.MaxOver < val)
							return true;

						if (y - m_Floor.MaxUnder > val)
							return true;
					}
			}

			if (m_Floor.MaxOver * m_Floor.MaxOver / n > m_Floor.MaxError)
				return false;

			if (m_Floor.MaxUnder * m_Floor.MaxUnder / n > m_Floor.MaxError)
				return false;

			// ReSharper disable once PossibleLossOfFraction
			if (mse / n > m_Floor.MaxError)
				return true;

			return false;
		}

		int FitLine(in Span<FitAccumulation> _Acc, int _Offset, int _Fits, out int _Y0, out int _Y1)
		{
			_Y0 = -200;
			_Y1 = -200;

			double xb = 0, yb = 0, x2B = 0, xyb = 0, bn = 0;

			var x0 = _Acc[_Offset + 0].X0;
			var x1 = _Acc[_Offset + _Fits - 1].X1;

			for (var i = 0; i < _Fits; i++)
			{
				var weight = (_Acc[_Offset + i].Bn + _Acc[_Offset + i].An) * m_Floor.TwoFitWeight / (_Acc[_Offset + i].An + 1) +
					1.0;

				xb  += _Acc[_Offset + i].Xb + _Acc[_Offset + i].Xa * weight;
				yb  += _Acc[_Offset + i].Yb + _Acc[_Offset + i].Ya * weight;
				x2B += _Acc[_Offset + i].X2B + _Acc[_Offset + i].X2A * weight;
				xyb += _Acc[_Offset + i].Xyb + _Acc[_Offset + i].Xya * weight;
				bn  += _Acc[_Offset + i].Bn + _Acc[_Offset + i].An * weight;
			}

			if (_Y0 >= 0)
			{
				xb  += x0;
				yb  += _Y0;
				x2B += x0 * x0;
				xyb += _Y0 * x0;
				bn++;
			}

			if (_Y1 >= 0)
			{
				xb  += x1;
				yb  += _Y1;
				x2B += x1 * x1;
				xyb += _Y1 * x1;
				bn++;
			}

			{
				var denom = bn * x2B - xb * xb;

				if (denom > 0)
				{
					var a = (yb * x2B - xyb * xb) / denom;
					var b = (bn * xyb - xb * yb) / denom;
					_Y0 = (int)Math.Round(a + b * x0);
					_Y1 = (int)Math.Round(a + b * x1);

					// limit to our range! 
					if (_Y0 > 1023) _Y0 = 1023;
					if (_Y1 > 1023) _Y1 = 1023;
					if (_Y0 < 0) _Y0    = 0;
					if (_Y1 < 0) _Y1    = 0;

					return 0;
				}
				_Y0 = 0;
				_Y1 = 0;
				return 1;
			}
		}

		static int RenderPoint(int _X0, int _X1, int _Y0, int _Y1, int _X)
		{
			_Y0 &= 0x7fff; // mask off flag
			_Y1 &= 0x7fff;

			var dy  = _Y1 - _Y0;
			var adx = _X1 - _X0;
			var ady = Math.Abs(dy);
			var err = ady * (_X - _X0);
			var off = err / adx;

			if (dy < 0)
				return _Y0 - off;

			return _Y0 + off;
		}

		static int PostY(in Span<int> _A, in Span<int> _B, int _Pos)
		{
			if (_A[_Pos] < 0)
				return _B[_Pos];

			if (_B[_Pos] < 0)
				return _A[_Pos];

			return (_A[_Pos] + _B[_Pos]) >> 1;
		}

		int AccumulateFit(float[] _Flr, in Span<float> _Mdct, int _X0, int _X1, ref FitAccumulation _Fits, int _N)
		{
			int xa = 0, ya = 0, x2A = 0, xya = 0, na = 0, xb = 0, yb = 0, x2B = 0, xyb = 0, nb = 0;

			_Fits.X0 = _X0;
			_Fits.X1 = _X1;

			if (_X1 >= _N)
				_X1 = _N - 1;

			for (var i = _X0; i <= _X1; i++)
			{
				var quantized = DecibelQuant(_Flr[i]);
				if (quantized != 0)
					if (_Mdct[i] + m_Floor.TwoFitAtten >= _Flr[i])
					{
						xa  += i;
						ya  += quantized;
						x2A += i * i;
						xya += i * quantized;
						na++;
					}
					else
					{
						xb  += i;
						yb  += quantized;
						x2B += i * i;
						xyb += i * quantized;
						nb++;
					}
			}

			_Fits.Xa  = xa;
			_Fits.Ya  = ya;
			_Fits.X2A = x2A;
			_Fits.Xya = xya;
			_Fits.An  = na;

			_Fits.Xb  = xb;
			_Fits.Yb  = yb;
			_Fits.X2B = x2B;
			_Fits.Xyb = xyb;
			_Fits.Bn  = nb;

			return na;
		}

		static int DecibelQuant(float _X)
		{
			var i = (int)(_X * 7.3142857f + 1023.5f);

			if (i > 1023)
				return 1023;

			if (i < 0)
				return 0;

			return i;
		}

		public bool Encode(
			EncodeBuffer           _Buffer,
			IList<IStaticCodeBook> _StaticBooks,
			CodeBook[]             _Books,
			int[]                  _Post,
			int[]                  _Ilogmask,
			int                    _PCMEnd,
			int                    _N
		)
		{
			Span<int> output = stackalloc int[Posit + 2];

			// quantize values to multiplier spec 
			if (_Post != null)
			{
				for (var i = 0; i < m_Posts; i++)
				{
					var val = _Post[i] & 0x7fff;
					switch (m_Floor.Mult)
					{
						case 1: // 1024 . 256 
							val >>= 2;
							break;
						case 2: // 1024 . 128 
							val >>= 3;
							break;
						case 3: // 1024 . 86 
							val /= 12;
							break;
						case 4: // 1024 . 64 
							val >>= 4;
							break;
					}
					_Post[i] = val | (_Post[i] & 0x8000);
				}

				output[0] = _Post[0];
				output[1] = _Post[1];

				// find prediction values for each post and subtract them 
				for (var i = 2; i < m_Posts; i++)
				{
					var ln = m_LowNeighbor[i - 2];
					var hn = m_HighNeighbor[i - 2];
					var x0 = m_Floor.PostList[ln];
					var x1 = m_Floor.PostList[hn];
					var y0 = _Post[ln];
					var y1 = _Post[hn];

					var predicted = RenderPoint(x0, x1, y0, y1, m_Floor.PostList[i]);

					if ((_Post[i] & 0x8000) != 0 || predicted == _Post[i])
					{
						_Post[i]   = predicted | 0x8000; // in case there was roundoff jitter in interpolation 
						output[i] = 0;
					}
					else
					{
						var headroom = m_QuantQ - predicted < predicted
							? m_QuantQ - predicted
							: predicted;

						var val = _Post[i] - predicted;

						// at this point the 'deviation' value is in the range +/- max
						// range, but the real, unique range can always be mapped to
						// only [0-maxrange).  So we want to wrap the deviation into
						// this limited range, but do it in the way that least screws
						// an essentially gaussian probability distribution. 

						if (val < 0)
							if (val < -headroom)
								val = headroom - val - 1;
							else
								val = -1 - (val << 1);
						else if (val >= headroom)
							val += headroom;
						else
							val <<= 1;

						output[i] =  val;
						_Post[ln]  &= 0x7fff;
						_Post[hn]  &= 0x7fff;
					}
				}

				// we have everything we need. pack it output 
				// mark nontrivial floor 
				_Buffer.Write(1, 1);

				// beginning/end post 
				var encodedQ = Encoding.Log(m_QuantQ - 1);
				_Buffer.Write((uint)output[0], encodedQ);
				_Buffer.Write((uint)output[1], encodedQ);


				// partition by partition 
				for (int i = 0, j = 2; i < m_Floor.PartitionClass.Length; i++)
				{
					var   c        = m_Floor.PartitionClass[i];
					var   cdim     = m_Floor.ClassDimensions[c];
					var   csubbits = m_Floor.ClassSubs[c];
					var   csub     = 1 << csubbits;
					var   cval     = 0;
					var   cshift   = 0;
					int   k;
					int[] bookas = { 0, 0, 0, 0, 0, 0, 0, 0 };

					// generate the partition's first stage cascade value 
					if (csubbits != 0)
					{
						var maxval = new int[8];
						for (k = 0; k < csub; k++)
						{
							var bookNumber = m_Floor.ClassSubBook[c][k];
							if (bookNumber < 0)
								maxval[k] = 1;
							else
								maxval[k] = _StaticBooks[m_Floor.ClassSubBook[c][k]].LengthList.Length;
						}
						for (k = 0; k < cdim; k++)
						{
							for (var l = 0; l < csub; l++)
							{
								var val = output[j + k];
								if (val < maxval[l])
								{
									bookas[k] = l;
									break;
								}
							}
							cval   |= bookas[k] << cshift;
							cshift += csubbits;
						}

						// write it 
						_Buffer.WriteBook(_Books[m_Floor.ClassBook[c]], cval);
					}

					// write post values 
					for (k = 0; k < cdim; k++)
					{
						var book = m_Floor.ClassSubBook[c][bookas[k]];
						if (book >= 0)
							if (output[j + k] < _Books[book].Entries)
								_Buffer.WriteBook(_Books[book], output[j + k]);
					}

					j += cdim;
				}

				// generate quantized floor equivalent to what we'd unpack in decode 
				// render the lines 
				var hx = 0;
				var lx = 0;
				var ly = _Post[0] * m_Floor.Mult;

				for (var j = 1; j < m_Posts; j++)
				{
					var current = m_ForwardIndex[j];
					var hy      = _Post[current] & 0x7fff;
					if (hy == _Post[current])
					{
						hy *= m_Floor.Mult;
						hx =  m_Floor.PostList[current];

						RenderLine0(_N, lx, hx, ly, hy, _Ilogmask);

						lx = hx;
						ly = hy;
					}
				}
				for (var j = hx; j < _PCMEnd / 2; j++) _Ilogmask[j] = ly; // be certain 
				return true;
			}

			_Buffer.Write(0, 1);
			Array.Clear(_Ilogmask, 0, _PCMEnd / 2);
			return false;
		}

		static void RenderLine0(int _N, int _X0, int _X1, int _Y0, int _Y1, int[] _D)
		{
			var dy  = _Y1 - _Y0;
			var adx = _X1 - _X0;
			var ady = Math.Abs(dy);
			var b   = dy / adx;
			var sy  = dy < 0 ? b - 1 : b + 1;
			var x   = _X0;
			var y   = _Y0;
			var err = 0;

			ady -= Math.Abs(b * adx);

			if (_N > _X1)
				_N = _X1;

			if (x < _N)
				_D[x] = y;

			while (++x < _N)
			{
				err += ady;
				if (err >= adx)
				{
					err -= adx;
					y   += sy;
				}
				else
				{
					y += b;
				}
				_D[x] = y;
			}
		}

		struct FitAccumulation
		{
			public int X0;
			public int X1;
			public int Xa;
			public int Ya;
			public int X2A;
			public int Xya;
			public int An;
			public int Xb;
			public int Yb;
			public int X2B;
			public int Xyb;
			public int Bn;
		}
	}
}
