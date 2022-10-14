namespace OggVorbisEncoder.Setup.Templates.Psyche
{

internal static class Psy44
{
    internal static readonly double[] Lowpass =
    {
        13.9, 15.1, 15.8, 16.5, 17.2, 18.9, 20.1, 48, 999, 999, 999, 999
    };

    internal static readonly Att3[] ToneMasterAtt =
    {
        new Att3(new[] {35, 21, 9}, 0, 0), /* -1 */
        new Att3(new[] {30, 20, 8}, -2, 1.25f), /* 0 */
        new Att3(new[] {25, 12, 2}, 0, 0), /* 1 */
        new Att3(new[] {20, 9, -3}, 0, 0), /* 2 */
        new Att3(new[] {20, 9, -4}, 0, 0), /* 3 */
        new Att3(new[] {20, 9, -4}, 0, 0), /* 4 */
        new Att3(new[] {20, 6, -6}, 0, 0), /* 5 */
        new Att3(new[] {20, 3, -10}, 0, 0), /* 6 */
        new Att3(new[] {18, 1, -14}, 0, 0), /* 7 */
        new Att3(new[] {18, 0, -16}, 0, 0), /* 8 */
        new Att3(new[] {18, -2, -16}, 0, 0), /* 9 */
        new Att3(new[] {12, -2, -20}, 0, 0) /* 10 */
    };

    internal static readonly NoiseGuard[] NoiseGuards =
    {
        new NoiseGuard(3, 3, 15),
        new NoiseGuard(3, 3, 15),
        new NoiseGuard(10, 10, 100),
        new NoiseGuard(10, 10, 100)
    };

    internal static readonly CompandBlock[] Compand =
    {
        /* sub-mode Z short */
        new CompandBlock(new[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, /* 7dB */
            8, 9, 10, 11, 12, 13, 14, 15, /* 15dB */
            16, 17, 18, 19, 20, 21, 22, 23, /* 23dB */
            24, 25, 26, 27, 28, 29, 30, 31, /* 31dB */
            32, 33, 34, 35, 36, 37, 38, 39 /* 39dB */
        }),
        /* mode_Z nominal short */
        new CompandBlock(new[]
        {
            0, 1, 2, 3, 4, 5, 6, 6, /* 7dB */
            7, 7, 7, 7, 6, 6, 6, 7, /* 15dB */
            7, 8, 9, 10, 11, 12, 13, 14, /* 23dB */
            15, 16, 17, 17, 17, 18, 18, 19, /* 31dB */
            19, 19, 20, 21, 22, 23, 24, 25 /* 39dB */
        }),
        /* mode A short */
        new CompandBlock(new[]
        {
            0, 1, 2, 3, 4, 5, 5, 5, /* 7dB */
            6, 6, 6, 5, 4, 4, 4, 4, /* 15dB */
            4, 4, 5, 5, 5, 6, 6, 6, /* 23dB */
            7, 7, 7, 8, 8, 8, 9, 10, /* 31dB */
            11, 12, 13, 14, 15, 16, 17, 18 /* 39dB */
        }),
        /* sub-mode Z long */
        new CompandBlock(new[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, /* 7dB */
            8, 9, 10, 11, 12, 13, 14, 15, /* 15dB */
            16, 17, 18, 19, 20, 21, 22, 23, /* 23dB */
            24, 25, 26, 27, 28, 29, 30, 31, /* 31dB */
            32, 33, 34, 35, 36, 37, 38, 39 /* 39dB */
        }),
        /* mode_Z nominal long */
        new CompandBlock(new[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, /* 7dB */
            8, 9, 10, 11, 12, 12, 13, 13, /* 15dB */
            13, 14, 14, 14, 15, 15, 15, 15, /* 23dB */
            16, 16, 17, 17, 17, 18, 18, 19, /* 31dB */
            19, 19, 20, 21, 22, 23, 24, 25 /* 39dB */
        }),
        /* mode A long */
        new CompandBlock(new[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, /* 7dB */
            8, 8, 7, 6, 5, 4, 4, 4, /* 15dB */
            4, 4, 5, 5, 5, 6, 6, 6, /* 23dB */
            7, 7, 7, 8, 8, 8, 9, 10, /* 31dB */
            11, 12, 13, 14, 15, 16, 17, 18 /* 39dB */
        })
    };

    internal static readonly double[] GlobalMapping = { 0, 1, 1, 1.5, 2, 2, 2.5, 2.7, 3.0, 3.7, 4, 4 };

    internal static readonly PsyGlobal[] Global =
    {
        new PsyGlobal(
            8,
            new[] {20f, 14f, 12f, 12f, 12f, 12f, 12},
            new[] {-60f, -30f, -40f, -40f, -40f, -40f, -40},
            2, -75f, -6f,
            new[] {99},
            new[] {new[] {99}, new[] {99}},
            new[] {0},
            new[] {0},
            new[] {new[] {0}, new[] {0}}),
        new PsyGlobal(
            8,
            new[] {14f, 10f, 10f, 10f, 10f, 10f, 10},
            new[] {-40f, -30f, -25f, -25f, -25f, -25f, -25},
            2, -80f,
            -6f,
            new[] {99},
            new[] {new[] {99}, new[] {99}},
            new[] {0},
            new[] {0},
            new[] {new[] {0}, new[] {0}}
        ),
        new PsyGlobal(
            8,
            new[] {12f, 10f, 10f, 10f, 10f, 10f, 10},
            new[] {-20f, -20f, -15f, -15f, -15f, -15f, -15},
            0, -80f,
            -6f,
            new[] {99},
            new[] {new[] {99}, new[] {99}},
            new[] {0},
            new[] {0},
            new[] {new[] {0}, new[] {0}}
        ),
        new PsyGlobal(8,
            new[] {10f, 8f, 8f, 8f, 8f, 8f, 8},
            new[] {-20f, -15f, -12f, -12f, -12f, -12f, -12},
            0, -80f,
            -6f,
            new[] {99},
            new[] {new[] {99}, new[] {99}},
            new[] {0},
            new[] {0},
            new[] {new[] {0}, new[] {0}}
        ),
        new PsyGlobal(8,
            new[] {10f, 6f, 6f, 6f, 6f, 6f, 6},
            new[] {-15f, -15f, -12f, -12f, -12f, -12f, -12},
            0, -85f,
            -6f,
            new[] {99},
            new[] {new[] {99}, new[] {99}},
            new[] {0},
            new[] {0},
            new[] {new[] {0}, new[] {0}}
        )
    };

    internal static readonly AdjStereo[] StereoModes =
    {
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         -1  */
        new AdjStereo(
            new[] {4, 4, 4, 4, 4, 4, 4, 3, 2, 2, 1, 0, 0, 0, 0},
            new[] {8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 5, 4, 3},
            new[] {1f, 2, 3, 4, 4, 4, 4, 4, 4, 5, 6, 7, 8, 8, 8},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*    0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         0  */
        new AdjStereo(new[] {4, 4, 4, 4, 4, 4, 4, 3, 2, 1, 0, 0, 0, 0, 0},
            new[] {8, 8, 8, 8, 6, 6, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            new[] {1f, 2, 3, 4, 4, 5, 6, 6, 6, 6, 6, 8, 8, 8, 8},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         1  */
        new AdjStereo(
            new[] {3, 3, 3, 3, 3, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0},
            new[] {8, 8, 8, 8, 6, 6, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            new[] {1f, 2, 3, 4, 4, 5, 6, 6, 6, 6, 6, 8, 8, 8, 8},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         2  */
        new AdjStereo(
            new[] {3, 3, 3, 3, 3, 3, 3, 2, 1, 1, 0, 0, 0, 0, 0},
            new[] {8, 8, 6, 6, 5, 5, 4, 4, 4, 4, 4, 4, 3, 2, 1},
            new[] {3f, 4, 4, 5, 5, 6, 6, 6, 6, 6, 6, 8, 8, 8, 8},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         3  */
        new AdjStereo(
            new[] {2, 2, 2, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0},
            new[] {5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2, 1},
            new[] {4f, 4, 5, 6, 6, 6, 6, 6, 8, 8, 10, 10, 10, 10, 10},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         4  */
        new AdjStereo(
            new[] {2, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 3, 3, 2, 1, 0},
            new[] {6f, 6, 6, 8, 8, 8, 8, 8, 8, 8, 10, 10, 10, 10, 10},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         5  */
        new AdjStereo(
            new[] {2, 2, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0},
            new[] {6f, 7, 8, 8, 8, 10, 10, 12, 12, 12, 12, 12, 12, 12, 12},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         6  */
        new AdjStereo(
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {3, 3, 3, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {8f, 8, 8, 10, 10, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         7  */
        new AdjStereo(
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {3, 3, 3, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {8f, 8, 10, 10, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         8  */
        new AdjStereo(
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {8f, 10, 10, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14         9  */
        new AdjStereo(
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {4f, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99}),
        /*  0   1   2   3   4   5   6   7   8   9  10  11  12  13  14        10  */
        new AdjStereo(
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {4f, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4},
            new[] {99f, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99})
    };

    internal static readonly double[] RateMapCoupled =
    {
        22500, 32000, 40000, 48000, 56000, 64000, 80000,
        96000, 112000, 128000, 160000, 250001
    };

    internal static readonly double[] RateMapUncoupled =
    {
        32000, 48000, 60000, 70000, 80000, 86000,
        96000, 110000, 120000, 140000, 160000, 240001
    };

    internal static readonly double[] QualityMapping = { -.1, .0, .1, .2, .3, .4, .5, .6, .7, .8, .9, 1.0 };
    internal static readonly int[] BlockSizeShort = { 512, 256, 256, 256, 256, 256, 256, 256, 256, 256, 256 };

    internal static readonly int[] BlockSizeLong =
    {
        4096, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
        2048
    };

    internal static readonly int[] NoiseStartShort = { 32, 16, 16, 16, 32, 9999, 9999, 9999, 9999, 9999, 9999 };
    internal static readonly int[] NoiseStartLong = { 256, 128, 128, 256, 512, 9999, 9999, 9999, 9999, 9999, 9999 };
    internal static readonly int[] NoisePartShort = { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 };
    internal static readonly int[] NoisePartLong = { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
    internal static readonly double[] NoiseThresh = { .2, .2, .2, .4, .6, 9999, 9999, 9999, 9999, 9999, 9999 };
    internal static readonly double[] NoiseThresh5Only = { 0.5, 0.5 };

    internal static readonly int[] FloorMappingA = { 1, 0, 0, 2, 2, 4, 5, 5, 5, 5, 5 };
    internal static readonly int[] FloorMappingB = { 8, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7 };
    //internal static readonly int[] FloorMappingC ={10,10,10,10,10,10,10,10,10,10,10};
    internal static readonly int[][] FloorMapping =
    {
        FloorMappingA,
        FloorMappingB
        //FloorMappingC, 
    };

    internal static readonly AdjBlock[] VpToneMaskAdjLongBlock =
    {
        new AdjBlock(new[] {-3, -8, -13, -15, -10, -10, -10, -10, -10, -10, -10, 0, 0, 0, 0, 0, 0}), /* -1 */
        new AdjBlock(new[] {-4, -10, -14, -16, -15, -14, -13, -12, -12, -12, -11, -1, -1, -1, -1, -1, 0}), /* 0 */
        new AdjBlock(new[] {-6, -12, -14, -16, -15, -15, -14, -13, -13, -12, -12, -2, -2, -1, -1, -1, 0}), /* 1 */
        new AdjBlock(new[] {-12, -13, -14, -16, -16, -16, -15, -14, -13, -12, -12, -6, -3, -1, -1, -1, 0}), /* 2 */
        new AdjBlock(new[] {-15, -15, -15, -16, -16, -16, -16, -14, -13, -13, -13, -10, -4, -2, -1, -1, 0}), /* 3 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7, -3, -1, -1, 0}), /* 4 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7, -3, -1, -1, 0}), /* 5 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -8, -4, -2, -2, 0}), /* 6 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 7 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 8 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 9 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}) /* 10 */
    };

    internal static readonly AdjBlock[] VpToneMaskAdjOtherBlock =
    {
        new AdjBlock(new[] {-3, -8, -13, -15, -10, -10, -9, -9, -9, -9, -9, 1, 1, 1, 1, 1, 1}), /* -1 */
        new AdjBlock(new[] {-4, -10, -14, -16, -14, -13, -12, -12, -11, -11, -10, 0, 0, 0, 0, 0, 0}), /* 0 */
        new AdjBlock(new[] {-6, -12, -14, -16, -15, -15, -14, -13, -13, -12, -12, -2, -2, -1, 0, 0, 0}), /* 1 */
        new AdjBlock(new[] {-12, -13, -14, -16, -16, -16, -15, -14, -13, -12, -12, -5, -2, -1, 0, 0, 0}), /* 2 */
        new AdjBlock(new[] {-15, -15, -15, -16, -16, -16, -16, -14, -13, -13, -13, -10, -4, -2, 0, 0, 0}), /* 3 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7, -3, -1, -1, 0}), /* 4 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -13, -11, -7, -3, -1, -1, 0}), /* 5 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -8, -4, -2, -2, 0}), /* 6 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 7 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 8 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}), /* 9 */
        new AdjBlock(new[] {-16, -16, -16, -16, -16, -16, -16, -15, -14, -14, -14, -12, -9, -4, -2, -2, 0}) /* 10 */
    };
}
}