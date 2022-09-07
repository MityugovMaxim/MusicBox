using System;
using System.Linq;

namespace OggVorbisEncoder.Setup
{

public class Mapping
{
    public Mapping(
        int _Submaps,
        int[] _ChannelMuxList,
        int[] _FloorSubMap,
        int[] _ResidueSubMap,
        int _CouplingSteps,
        int[] _CouplingMag,
        int[] _CouplingAng)
    {
        if (_FloorSubMap?.Length != _ResidueSubMap?.Length)
            throw new ArgumentException($"{nameof(_FloorSubMap)} and {nameof(_ResidueSubMap)} must be the same size");

        if (_CouplingMag?.Length != _CouplingAng?.Length)
            throw new ArgumentException($"{nameof(_CouplingMag)} and {nameof(_CouplingAng)} must be the same size");

        SubMaps = _Submaps;
        ChannelMuxList = _ChannelMuxList;
        FloorSubMap = _FloorSubMap;
        ResidueSubMap = _ResidueSubMap;
        CouplingSteps = _CouplingSteps;
        CouplingMag = _CouplingMag;
        CouplingAng = _CouplingAng;
    }

    public int SubMaps { get; }

    public int[] ChannelMuxList { get; } // up to 256 channels in a Vorbis stream

    public int[] FloorSubMap { get; } // [mux] submap to floors
    public int[] ResidueSubMap { get; } // [mux] submap to residue

    public int CouplingSteps { get; }

    public int[] CouplingMag { get; }
    public int[] CouplingAng { get; }

    public Mapping Clone() => new Mapping(
        SubMaps,
        ChannelMuxList.ToArray(),
        FloorSubMap.ToArray(),
        ResidueSubMap.ToArray(),
        CouplingSteps,
        CouplingMag.ToArray(),
        CouplingAng.ToArray());
}
}
