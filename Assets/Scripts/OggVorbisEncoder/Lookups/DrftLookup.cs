using System;

namespace OggVorbisEncoder.Lookups
{
	public class DrftLookup
	{
		readonly        int[]   m_SplitCache;
		public readonly float[] m_TrigCache;

		public DrftLookup(int _N)
		{
			N = _N;

			m_TrigCache  = new float[3 * _N];
			m_SplitCache = new int[32];

			Fdrffti(_N);
		}

		public int N { get; }

		void Fdrffti(int _N)
		{
			if (_N == 1)
				return;

			Drfti1(_N);
		}

		void Drfti1(int _N)
		{
			int[]       ntryh = { 4, 2, 3, 5 };
			const float tpi   = 6.28318530717958648f;
			int         ntry  = 0, i, j = -1;
			var         nl    = _N;
			var         nf    = 0;

			int nr;
			do
			{
				j++;
				if (j < 4)
					ntry = ntryh[j];
				else
					ntry += 2;

				do
				{
					var nq = nl / ntry;

					nr = nl - ntry * nq;
					if (nr != 0)
						break;

					nf++;
					m_SplitCache[nf + 1] = ntry;
					nl                  = nq;
					if (ntry != 2 || nf == 1)
						continue;

					for (i = 1; i < nf; i++)
					{
						var ib = nf - i + 1;
						m_SplitCache[ib + 1] = m_SplitCache[ib];
					}

					m_SplitCache[2] = 2;
				}
				while (nl != 1);
			}
			while (nr != 1);

			m_SplitCache[0] = _N;
			m_SplitCache[1] = nf;
			var argh    = tpi / _N;
			var setting = _N;
			var nfm1    = nf - 1;
			var l1      = 1;

			if (nfm1 == 0)
				return;

			for (var k1 = 0; k1 < nfm1; k1++)
			{
				var ip  = m_SplitCache[k1 + 2];
				var ld  = 0;
				var l2  = l1 * ip;
				var ido = _N / l2;
				var ipm = ip - 1;

				for (j = 0; j < ipm; j++)
				{
					ld += l1;
					i  =  setting;
					var argld = ld * argh;
					var fi    = 0f;
					int ii;
					for (ii = 2; ii < ido; ii += 2)
					{
						fi += 1f;
						var arg = fi * argld;
						m_TrigCache[i++] = (float)Math.Cos(arg);
						m_TrigCache[i++] = (float)Math.Sin(arg);
					}
					setting += ido;
				}
				l1 = l2;
			}
		}

		public void Forward(float[] _Data)
		{
			if (N == 1)
				return;

			var nf = m_SplitCache[1];
			var na = 1;
			var l2 = N;
			var iw = N;

			for (var k1 = 0; k1 < nf; k1++)
			{
				var kh   = nf - k1;
				var ip   = m_SplitCache[kh + 1];
				var l1   = l2 / ip;
				var ido  = N / l2;
				var idl1 = ido * l1;
				iw -= (ip - 1) * ido;
				na =  1 - na;

				if (ip != 4)
				{
					if (ip != 2)
					{
						if (ido == 1)
							na = 1 - na;

						if (na != 0)
						{
							Dradfg(ido, ip, l1, idl1, m_TrigCache, m_TrigCache, m_TrigCache, _Data, _Data, N + iw - 1);
							na = 0;
						}
						else
						{
							Dradfg(ido, ip, l1, idl1, _Data, _Data, _Data, m_TrigCache, m_TrigCache, N + iw - 1);
							na = 1;
						}
					}
					else
					{
						if (na != 0)
							Dradf2(ido, l1, m_TrigCache, _Data, N + iw - 1);
						else
							Dradf2(ido, l1, _Data, m_TrigCache, N + iw - 1);
					}
				}
				else
				{
					var ix2 = iw + ido;
					var ix3 = ix2 + ido;

					if (na != 0)
						Dradf4(ido, l1, m_TrigCache, _Data, N + iw - 1, N + ix2 - 1, N + ix3 - 1);
					else
						Dradf4(ido, l1, _Data, m_TrigCache, N + iw - 1, N + ix2 - 1, N + ix3 - 1);
				}

				l2 = l1;
			}

			if (na == 1)
				return;

			for (var i = 0; i < N; i++)
				_Data[i] = m_TrigCache[i];
		}

		void Dradf4(int _Ido, int _L1, float[] _Cc, float[] _Ch, int _Wa1, int _Wa2, int _Wa3)
		{
			const float hsqt2 = .70710678118654752f;
			int         k, t5, t6;
			float       ti1;
			float       tr1, tr2;
			var         t0 = _L1 * _Ido;
			var         t1 = t0;
			var         t4 = t1 << 1;
			var         t2 = t1 + (t1 << 1);
			var         t3 = 0;

			for (k = 0; k < _L1; k++)
			{
				tr1 = _Cc[t1] + _Cc[t2];
				tr2 = _Cc[t3] + _Cc[t4];

				_Ch[t5 = t3 << 2]         = tr1 + tr2;
				_Ch[(_Ido << 2) + t5 - 1]  = tr2 - tr1;
				_Ch[(t5 += _Ido << 1) - 1] = _Cc[t3] - _Cc[t4];
				_Ch[t5]                   = _Cc[t2] - _Cc[t1];

				t1 += _Ido;
				t2 += _Ido;
				t3 += _Ido;
				t4 += _Ido;
			}

			if (_Ido < 2)
				return;

			if (_Ido > 2)
			{
				t1 = 0;
				for (k = 0; k < _L1; k++)
				{
					t2 = t1;
					t4 = t1 << 2;
					t5 = (t6 = _Ido << 1) + t4;
					for (var i = 2; i < _Ido; i += 2)
					{
						t3 =  t2 += 2;
						t4 += 2;
						t5 -= 2;

						t3 += t0;
						var cr2 = m_TrigCache[_Wa1 + i - 2] * _Cc[t3 - 1] + m_TrigCache[_Wa1 + i - 1] * _Cc[t3];
						var ci2 = m_TrigCache[_Wa1 + i - 2] * _Cc[t3] - m_TrigCache[_Wa1 + i - 1] * _Cc[t3 - 1];
						t3 += t0;
						var cr3 = m_TrigCache[_Wa2 + i - 2] * _Cc[t3 - 1] + m_TrigCache[_Wa2 + i - 1] * _Cc[t3];
						var ci3 = m_TrigCache[_Wa2 + i - 2] * _Cc[t3] - m_TrigCache[_Wa2 + i - 1] * _Cc[t3 - 1];
						t3 += t0;
						var cr4 = m_TrigCache[_Wa3 + i - 2] * _Cc[t3 - 1] + m_TrigCache[_Wa3 + i - 1] * _Cc[t3];
						var ci4 = m_TrigCache[_Wa3 + i - 2] * _Cc[t3] - m_TrigCache[_Wa3 + i - 1] * _Cc[t3 - 1];

						tr1 = cr2 + cr4;
						var tr4 = cr4 - cr2;
						ti1 = ci2 + ci4;
						var ti4 = ci2 - ci4;

						var ti2 = _Cc[t2] + ci3;
						var ti3 = _Cc[t2] - ci3;
						tr2 = _Cc[t2 - 1] + cr3;
						var tr3 = _Cc[t2 - 1] - cr3;

						_Ch[t4 - 1] = tr1 + tr2;
						_Ch[t4]     = ti1 + ti2;

						_Ch[t5 - 1] = tr3 - ti4;
						_Ch[t5]     = tr4 - ti3;

						_Ch[t4 + t6 - 1] = ti4 + tr3;
						_Ch[t4 + t6]     = tr4 + ti3;

						_Ch[t5 + t6 - 1] = tr2 - tr1;
						_Ch[t5 + t6]     = ti1 - ti2;
					}
					t1 += _Ido;
				}

				if ((_Ido & 1) != 0)
					return;
			}

			t2 = (t1 = t0 + _Ido - 1) + (t0 << 1);
			t3 = _Ido << 2;
			t4 = _Ido;
			t5 = _Ido << 1;
			t6 = _Ido;

			for (k = 0; k < _L1; k++)
			{
				ti1 = -hsqt2 * (_Cc[t1] + _Cc[t2]);
				tr1 = hsqt2 * (_Cc[t1] - _Cc[t2]);

				_Ch[t4 - 1]      = tr1 + _Cc[t6 - 1];
				_Ch[t4 + t5 - 1] = _Cc[t6 - 1] - tr1;

				_Ch[t4]      = ti1 - _Cc[t1 + t0];
				_Ch[t4 + t5] = ti1 + _Cc[t1 + t0];

				t1 += _Ido;
				t2 += _Ido;
				t4 += t3;
				t6 += _Ido;
			}
		}

		void Dradf2(int _Ido, int _L1, float[] _Cc, float[] _Ch, int _Wa1)
		{
			var t1 = 0;
			var t2 = _L1 * _Ido;
			var t0 = t2;
			var t3 = _Ido << 1;
			for (var k = 0; k < _L1; k++)
			{
				_Ch[t1 << 1]            =  _Cc[t1] + _Cc[t2];
				_Ch[(t1 << 1) + t3 - 1] =  _Cc[t1] - _Cc[t2];
				t1                     += _Ido;
				t2                     += _Ido;
			}

			if (_Ido < 2)
				return;

			if (_Ido > 2)
			{
				t1 = 0;
				t2 = t0;
				for (var k = 0; k < _L1; k++)
				{
					t3 = t2;
					var t4 = (t1 << 1) + (_Ido << 1);
					var t5 = t1;
					var t6 = t1 + t1;
					for (var i = 2; i < _Ido; i += 2)
					{
						t3 += 2;
						t4 -= 2;
						t5 += 2;
						t6 += 2;
						var tr2 = m_TrigCache[_Wa1 + i - 2] * _Cc[t3 - 1] + m_TrigCache[_Wa1 + i - 1] * _Cc[t3];
						var ti2 = m_TrigCache[_Wa1 + i - 2] * _Cc[t3] - m_TrigCache[_Wa1 + i - 1] * _Cc[t3 - 1];
						_Ch[t6]     = _Cc[t5] + ti2;
						_Ch[t4]     = ti2 - _Cc[t5];
						_Ch[t6 - 1] = _Cc[t5 - 1] + tr2;
						_Ch[t4 - 1] = _Cc[t5 - 1] - tr2;
					}
					t1 += _Ido;
					t2 += _Ido;
				}

				if (_Ido % 2 == 1)
					return;
			}

			t3 =  t2 = (t1 = _Ido) - 1;
			t2 += t0;
			for (var k = 0; k < _L1; k++)
			{
				_Ch[t1]     =  -_Cc[t2];
				_Ch[t1 - 1] =  _Cc[t3];
				t1         += _Ido << 1;
				t2         += _Ido;
				t3         += _Ido;
			}
		}

		void Dradfg(
			int     _Ido,
			int     _IP,
			int     _L1,
			int     _Idl1,
			float[] _Cc,
			float[] _C1,
			float[] _C2,
			float[] _Ch,
			float[] _Ch2,
			int     _Wa
		)
		{
			const float tpi = 6.283185307179586f;

			var arg  = tpi / _IP;
			var dcp  = (float)Math.Cos(arg);
			var dsp  = (float)Math.Sin(arg);
			var ipph = (_IP + 1) >> 1;
			var ipp2 = _IP;
			var idp2 = _Ido;
			var nbd  = (_Ido - 1) >> 1;
			var t0   = _L1 * _Ido;
			var t10  = _IP * _Ido;

			int i, j, k, l, ik, t1, t2, t3, t4, t5, t6, t7, t8, t9;

			if (_Ido != 1)
			{
				for (ik = 0; ik < _Idl1; ik++)
					_Ch2[ik] = _C2[ik];

				t1 = 0;
				for (j = 1; j < _IP; j++)
				{
					t1 += t0;
					t2 =  t1;
					for (k = 0; k < _L1; k++)
					{
						_Ch[t2] =  _C1[t2];
						t2     += _Ido;
					}
				}

				var setting = -_Ido;
				t1 = 0;

				if (nbd > _L1)
					for (j = 1; j < _IP; j++)
					{
						t1      += t0;
						setting += _Ido;
						t2      =  -_Ido + t1;
						for (k = 0; k < _L1; k++)
						{
							var idij = setting - 1;
							t2 += _Ido;
							t3 =  t2;
							for (i = 2; i < _Ido; i += 2)
							{
								idij       += 2;
								t3         += 2;
								_Ch[t3 - 1] =  m_TrigCache[_Wa + idij - 1] * _C1[t3 - 1] + m_TrigCache[_Wa + idij] * _C1[t3];
								_Ch[t3]     =  m_TrigCache[_Wa + idij - 1] * _C1[t3] - m_TrigCache[_Wa + idij] * _C1[t3 - 1];
							}
						}
					}
				else
					for (j = 1; j < _IP; j++)
					{
						setting += _Ido;
						var idij = setting - 1;
						t1 += t0;
						t2 =  t1;
						for (i = 2; i < _Ido; i += 2)
						{
							idij += 2;
							t2   += 2;
							t3   =  t2;
							for (k = 0; k < _L1; k++)
							{
								_Ch[t3 - 1] =  m_TrigCache[_Wa + idij - 1] * _C1[t3 - 1] + m_TrigCache[_Wa + idij] * _C1[t3];
								_Ch[t3]     =  m_TrigCache[_Wa + idij - 1] * _C1[t3] - m_TrigCache[_Wa + idij] * _C1[t3 - 1];
								t3         += _Ido;
							}
						}
					}

				t1 = 0;
				t2 = ipp2 * t0;
				if (nbd < _L1)
					for (j = 1; j < ipph; j++)
					{
						t1 += t0;
						t2 -= t0;
						t3 =  t1;
						t4 =  t2;
						for (i = 2; i < _Ido; i += 2)
						{
							t3 += 2;
							t4 += 2;
							t5 =  t3 - _Ido;
							t6 =  t4 - _Ido;
							for (k = 0; k < _L1; k++)
							{
								t5         += _Ido;
								t6         += _Ido;
								_C1[t5 - 1] =  _Ch[t5 - 1] + _Ch[t6 - 1];
								_C1[t6 - 1] =  _Ch[t5] - _Ch[t6];
								_C1[t5]     =  _Ch[t5] + _Ch[t6];
								_C1[t6]     =  _Ch[t6 - 1] - _Ch[t5 - 1];
							}
						}
					}
				else
					for (j = 1; j < ipph; j++)
					{
						t1 += t0;
						t2 -= t0;
						t3 =  t1;
						t4 =  t2;
						for (k = 0; k < _L1; k++)
						{
							t5 = t3;
							t6 = t4;
							for (i = 2; i < _Ido; i += 2)
							{
								t5         += 2;
								t6         += 2;
								_C1[t5 - 1] =  _Ch[t5 - 1] + _Ch[t6 - 1];
								_C1[t6 - 1] =  _Ch[t5] - _Ch[t6];
								_C1[t5]     =  _Ch[t5] + _Ch[t6];
								_C1[t6]     =  _Ch[t6 - 1] - _Ch[t5 - 1];
							}
							t3 += _Ido;
							t4 += _Ido;
						}
					}
			}

			for (ik = 0; ik < _Idl1; ik++)
				_C2[ik] = _Ch2[ik];

			t1 = 0;
			t2 = ipp2 * _Idl1;
			for (j = 1; j < ipph; j++)
			{
				t1 += t0;
				t2 -= t0;
				t3 =  t1 - _Ido;
				t4 =  t2 - _Ido;
				for (k = 0; k < _L1; k++)
				{
					t3     += _Ido;
					t4     += _Ido;
					_C1[t3] =  _Ch[t3] + _Ch[t4];
					_C1[t4] =  _Ch[t4] - _Ch[t3];
				}
			}

			var ar1 = 1f;
			var ai1 = 0f;
			t1 = 0;
			t2 = ipp2 * _Idl1;
			t3 = (_IP - 1) * _Idl1;
			for (l = 1; l < ipph; l++)
			{
				t1 += _Idl1;
				t2 -= _Idl1;
				var ar1H = dcp * ar1 - dsp * ai1;
				ai1 = dcp * ai1 + dsp * ar1;
				ar1 = ar1H;
				t4  = t1;
				t5  = t2;
				t6  = t3;
				t7  = _Idl1;

				for (ik = 0; ik < _Idl1; ik++)
				{
					_Ch2[t4++] = _C2[ik] + ar1 * _C2[t7++];
					_Ch2[t5++] = ai1 * _C2[t6++];
				}

				var dc2 = ar1;
				var ds2 = ai1;
				var ar2 = ar1;
				var ai2 = ai1;

				t4 = _Idl1;
				t5 = (ipp2 - 1) * _Idl1;
				for (j = 2; j < ipph; j++)
				{
					t4 += _Idl1;
					t5 -= _Idl1;

					var ar2H = dc2 * ar2 - ds2 * ai2;
					ai2 = dc2 * ai2 + ds2 * ar2;
					ar2 = ar2H;

					t6 = t1;
					t7 = t2;
					t8 = t4;
					t9 = t5;
					for (ik = 0; ik < _Idl1; ik++)
					{
						_Ch2[t6++] += ar2 * _C2[t8++];
						_Ch2[t7++] += ai2 * _C2[t9++];
					}
				}
			}

			t1 = 0;
			for (j = 1; j < ipph; j++)
			{
				t1 += _Idl1;
				t2 =  t1;
				for (ik = 0; ik < _Idl1; ik++)
					_Ch2[ik] += _C2[t2++];
			}

			if (_Ido < _L1)
			{
				for (i = 0; i < _Ido; i++)
				{
					t1 = i;
					t2 = i;
					for (k = 0; k < _L1; k++)
					{
						_Cc[t2] =  _Ch[t1];
						t1     += _Ido;
						t2     += t10;
					}
				}
			}
			else
			{
				t1 = 0;
				t2 = 0;
				for (k = 0; k < _L1; k++)
				{
					t3 = t1;
					t4 = t2;

					for (i = 0; i < _Ido; i++)
						_Cc[t4++] = _Ch[t3++];

					t1 += _Ido;
					t2 += t10;
				}
			}

			t1 = 0;
			t2 = _Ido << 1;
			t3 = 0;
			t4 = ipp2 * t0;
			for (j = 1; j < ipph; j++)
			{
				t1 += t2;
				t3 += t0;
				t4 -= t0;

				t5 = t1;
				t6 = t3;
				t7 = t4;

				for (k = 0; k < _L1; k++)
				{
					_Cc[t5 - 1] =  _Ch[t6];
					_Cc[t5]     =  _Ch[t7];
					t5         += t10;
					t6         += _Ido;
					t7         += _Ido;
				}
			}

			if (_Ido == 1)
				return;

			if (nbd >= _L1)
			{
				t1 = -_Ido;
				t3 = 0;
				t4 = 0;
				t5 = ipp2 * t0;
				for (j = 1; j < ipph; j++)
				{
					t1 += t2;
					t3 += t2;
					t4 += t0;
					t5 -= t0;
					t6 =  t1;
					t7 =  t3;
					t8 =  t4;
					t9 =  t5;
					for (k = 0; k < _L1; k++)
					{
						for (i = 2; i < _Ido; i += 2)
						{
							var ic = idp2 - i;
							_Cc[i + t7 - 1]  = _Ch[i + t8 - 1] + _Ch[i + t9 - 1];
							_Cc[ic + t6 - 1] = _Ch[i + t8 - 1] - _Ch[i + t9 - 1];
							_Cc[i + t7]      = _Ch[i + t8] + _Ch[i + t9];
							_Cc[ic + t6]     = _Ch[i + t9] - _Ch[i + t8];
						}
						t6 += t10;
						t7 += t10;
						t8 += _Ido;
						t9 += _Ido;
					}
				}

				return;
			}

			t1 = -_Ido;
			t3 = 0;
			t4 = 0;
			t5 = ipp2 * t0;
			for (j = 1; j < ipph; j++)
			{
				t1 += t2;
				t3 += t2;
				t4 += t0;
				t5 -= t0;
				for (i = 2; i < _Ido; i += 2)
				{
					t6 = idp2 + t1 - i;
					t7 = i + t3;
					t8 = i + t4;
					t9 = i + t5;
					for (k = 0; k < _L1; k++)
					{
						_Cc[t7 - 1] =  _Ch[t8 - 1] + _Ch[t9 - 1];
						_Cc[t6 - 1] =  _Ch[t8 - 1] - _Ch[t9 - 1];
						_Cc[t7]     =  _Ch[t8] + _Ch[t9];
						_Cc[t6]     =  _Ch[t9] - _Ch[t8];
						t6         += t10;
						t7         += t10;
						t8         += _Ido;
						t9         += _Ido;
					}
				}
			}
		}
	}
}