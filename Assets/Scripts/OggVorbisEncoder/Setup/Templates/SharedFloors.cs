using OggVorbisEncoder.Setup.Templates.FloorBooks;

namespace OggVorbisEncoder.Setup.Templates
{
	public static class SharedFloors
	{
		static readonly IStaticCodeBook[] m_Floor128X4Books =
		{
			new Line128X4Class0(),
			new Line128X4Sub0(),
			new Line128X4Sub1(),
			new Line128X4Sub2(),
			new Line128X4Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor256X4Books =
		{
			new Line256X4Class0(),
			new Line256X4Sub0(),
			new Line256X4Sub1(),
			new Line256X4Sub2(),
			new Line256X4Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor128X7Books =
		{
			new Line128X7Class0(),
			new Line128X7Class1(),
			new Line128X70Sub1(),
			new Line128X70Sub2(),
			new Line128X70Sub3(),
			new Line128X71Sub1(),
			new Line128X71Sub2(),
			new Line128X71Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor256X7Books =
		{
			new Line256X7Class0(),
			new Line256X7Class1(),
			new Line256X70Sub1(),
			new Line256X70Sub2(),
			new Line256X70Sub3(),
			new Line256X71Sub1(),
			new Line256X71Sub2(),
			new Line256X71Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor128X11Books =
		{
			new Line128X11Class1(),
			new Line128X11Class2(),
			new Line128X11Class3(),
			new Line128X110Sub0(),
			new Line128X111Sub0(),
			new Line128X111Sub1(),
			new Line128X112Sub1(),
			new Line128X112Sub2(),
			new Line128X112Sub3(),
			new Line128X113Sub1(),
			new Line128X113Sub2(),
			new Line128X113Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor128X17Books =
		{
			new Line128X17Class1(),
			new Line128X17Class2(),
			new Line128X17Class3(),
			new Line128X170Sub0(),
			new Line128X171Sub0(),
			new Line128X171Sub1(),
			new Line128X172Sub1(),
			new Line128X172Sub2(),
			new Line128X172Sub3(),
			new Line128X173Sub1(),
			new Line128X173Sub2(),
			new Line128X173Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor256X4LowBooks =
		{
			new Line256X4LowClass0(),
			new Line256X4LowSub0(),
			new Line256X4LowSub1(),
			new Line256X4LowSub2(),
			new Line256X4LowSub3()
		};

		static readonly IStaticCodeBook[] m_Floor1024X27Books =
		{
			new Line1024X27Class1(),
			new Line1024X27Class2(),
			new Line1024X27Class3(),
			new Line1024X27Class4(),
			new Line1024X270Sub0(),
			new Line1024X271Sub0(),
			new Line1024X271Sub1(),
			new Line1024X272Sub0(),
			new Line1024X272Sub1(),
			new Line1024X273Sub1(),
			new Line1024X273Sub2(),
			new Line1024X273Sub3(),
			new Line1024X274Sub1(),
			new Line1024X274Sub2(),
			new Line1024X274Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor2048X27Books =
		{
			new Line2048X27Class1(),
			new Line2048X27Class2(),
			new Line2048X27Class3(),
			new Line2048X27Class4(),
			new Line2048X270Sub0(),
			new Line2048X271Sub0(),
			new Line2048X271Sub1(),
			new Line2048X272Sub0(),
			new Line2048X272Sub1(),
			new Line2048X273Sub1(),
			new Line2048X273Sub2(),
			new Line2048X273Sub3(),
			new Line2048X274Sub1(),
			new Line2048X274Sub2(),
			new Line2048X274Sub3()
		};

		static readonly IStaticCodeBook[] m_Floor512X17Books =
		{
			new Line512X17Class1(),
			new Line512X17Class2(),
			new Line512X17Class3(),
			new Line512X170Sub0(),
			new Line512X171Sub0(),
			new Line512X171Sub1(),
			new Line512X172Sub1(),
			new Line512X172Sub2(),
			new Line512X172Sub3(),
			new Line512X173Sub1(),
			new Line512X173Sub2(),
			new Line512X173Sub3()
		};

		public static readonly IStaticCodeBook[][] FloorBooks =
		{
			m_Floor128X4Books,
			m_Floor256X4Books,
			m_Floor128X7Books,
			m_Floor256X7Books,
			m_Floor128X11Books,
			m_Floor128X17Books,
			m_Floor256X4LowBooks,
			m_Floor1024X27Books,
			m_Floor2048X27Books,
			m_Floor512X17Books
		};

		public static readonly Floor[] Floor =
		{
			/* 0: 128 x 4 */
			new(
				new[] { 0 }, new[] { 4 }, new[] { 2 }, new[] { 0 },
				new[] { new[] { 1, 2, 3, 4 } },
				4, new[] { 0, 128, 33, 8, 16, 70 },
				60, 30, 500, 1, 18, 128
			),
			/* 1: 256 x 4 */
			new(
				new[] { 0 }, new[] { 4 }, new[] { 2 }, new[] { 0 },
				new[] { new[] { 1, 2, 3, 4 } },
				4, new[] { 0, 256, 66, 16, 32, 140 },
				60, 30, 500, 1, 18, 256
			),
			/* 2: 128 x 7 */
			new(
				new[] { 0, 1 }, new[] { 3, 4 }, new[] { 2, 2 }, new[] { 0, 1 },
				new[] { new[] { -1, 2, 3, 4 }, new[] { -1, 5, 6, 7 } },
				4, new[] { 0, 128, 14, 4, 58, 2, 8, 28, 90 },
				60, 30, 500, 1, 18, 128
			),
			/* 3: 256 x 7 */
			new(
				new[] { 0, 1 }, new[] { 3, 4 }, new[] { 2, 2 }, new[] { 0, 1 },
				new[] { new[] { -1, 2, 3, 4 }, new[] { -1, 5, 6, 7 } },
				4, new[] { 0, 256, 28, 8, 116, 4, 16, 56, 180 },
				60, 30, 500, 1, 18, 256
			),
			/* 4: 128 x 11 */
			new(
				new[] { 0, 1, 2, 3 }, new[] { 2, 3, 3, 3 }, new[] { 0, 1, 2, 2 }, new[] { -1, 0, 1, 2 },
				new[] { new[] { 3 }, new[] { 4, 5 }, new[] { -1, 6, 7, 8 }, new[] { -1, 9, 10, 11 } },
				2, new[] { 0, 128, 8, 33, 4, 16, 70, 2, 6, 12, 23, 46, 90 },
				60, 30, 500, 1, 18, 128
			),
			/* 5: 128 x 17 */
			new(
				new[] { 0, 1, 1, 2, 3, 3 }, new[] { 2, 3, 3, 3 }, new[] { 0, 1, 2, 2 }, new[] { -1, 0, 1, 2 },
				new[] { new[] { 3 }, new[] { 4, 5 }, new[] { -1, 6, 7, 8 }, new[] { -1, 9, 10, 11 } },
				2, new[] { 0, 128, 12, 46, 4, 8, 16, 23, 33, 70, 2, 6, 10, 14, 19, 28, 39, 58, 90 },
				60, 30, 500, 1, 18, 128
			),
			/* 6: 256 x 4 (low bitrate version) */
			new(
				new[] { 0 }, new[] { 4 }, new[] { 2 }, new[] { 0 },
				new[] { new[] { 1, 2, 3, 4 } },
				4, new[] { 0, 256, 66, 16, 32, 140 },
				60, 30, 500, 1, 18, 256
			),
			/* 7: 1024 x 27 */
			new(
				new[] { 0, 1, 2, 2, 3, 3, 4, 4 }, new[] { 3, 4, 3, 4, 3 }, new[] { 0, 1, 1, 2, 2 },
				new[] { -1, 0, 1, 2, 3 },
				new[] { new[] { 4 }, new[] { 5, 6 }, new[] { 7, 8 }, new[] { -1, 9, 10, 11 }, new[] { -1, 12, 13, 14 } },
				2, new[]
				{
					0, 1024, 93, 23, 372, 6, 46, 186, 750, 14, 33, 65, 130, 260, 556,
					3, 10, 18, 28, 39, 55, 79, 111, 158, 220, 312, 464, 650, 850
				},
				60, 30, 500, 3, 18, 1024
			),
			/* 8: 2048 x 27 */
			new(
				new[] { 0, 1, 2, 2, 3, 3, 4, 4 }, new[] { 3, 4, 3, 4, 3 }, new[] { 0, 1, 1, 2, 2 },
				new[] { -1, 0, 1, 2, 3 },
				new[] { new[] { 4 }, new[] { 5, 6 }, new[] { 7, 8 }, new[] { -1, 9, 10, 11 }, new[] { -1, 12, 13, 14 } },
				2, new[]
				{
					0, 2048, 186, 46, 744, 12, 92, 372, 1500, 28, 66, 130, 260, 520, 1112,
					6, 20, 36, 56, 78, 110, 158, 222, 316, 440, 624, 928, 1300, 1700
				},
				60, 30, 500, 3, 18, 2048
			),
			/* 9: 512 x 17 */
			new(
				new[] { 0, 1, 1, 2, 3, 3 }, new[] { 2, 3, 3, 3 }, new[] { 0, 1, 2, 2 }, new[] { -1, 0, 1, 2 },
				new[] { new[] { 3 }, new[] { 4, 5 }, new[] { -1, 6, 7, 8 }, new[] { -1, 9, 10, 11 } },
				2, new[]
				{
					0, 512, 46, 186, 16, 33, 65, 93, 130, 278,
					7, 23, 39, 55, 79, 110, 156, 232, 360
				},
				60, 30, 500, 1, 18, 512
			),
			/* 10: X x 0 (LFE floor; edge posts only) */
			new(
				new[] { 0 }, new[] { 0 }, new[] { 0 }, new[] { -1 },
				new[] { new[] { -1 } },
				2, new[] { 0, 12 },
				60, 30, 500, 1, 18, 10
			)
		};
	}
}