using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.Definitions
{
    public enum DefinitionSet
    {
        Unknown = -1,
        Halo1Xbox = 0,
        Halo1PC = 1,
        Halo1CE = 2,
        Halo2Xbox = 3,
        Halo2Vista = 4,
        Halo3Beta = 5,
        Halo3Retail = 6,
        Halo3ODST = 7,
        HaloReachBeta = 8,
        HaloReachRetail = 9,
        Halo4Beta = 10,
        Halo4Retail = 11
    }

    public enum Language
    {
        English = 0,
        Japanese = 1,
        German = 2,
        French = 3,
        Spanish = 4,
        LatinAmericanSpanish = 5,
        Italian = 6,
        Korean = 7,
        Chinese = 8,
        Unknown0 = 9,
        Portuguese = 10,
        Unknown1 = 11
    }

    public enum TextureFormat
    {
        None = -1,
        A8 = 0,
        Y8 = 1,
        AY8 = 2,
        A8Y8 = 3,
        Unused4 = 4,
        Unused5 = 5,
        R5G6B5 = 6,
        Unused7 = 7,
        A1R5G5B5 = 8,
        A4R4G4B4 = 9,
        X8R8G8B8 = 10,
        A8R8G8B8 = 11,
        Unused12 = 12,
        Unused13 = 13,
        DXT1 = 14,
        DXT3 = 15,
        DXT5 = 16,
        P8_bump = 17,
        P8 = 18,
        ARGBFP32 = 19,
        RGBFP32 = 20,
        RGBFP16 = 21,
        U8V8 = 22,
        Unknown23 = 23,
        Unknown24 = 24,
        Unknown25 = 25,
        Unknown26 = 26,
        Unknown27 = 27,
        Unknown28 = 28,
        Unknown29 = 29,
        Unknown30 = 30,
        DXT5a = 31,
        Unknown32 = 32,
        DXN = 33, //Reach unused?
        CTX1 = 34, //Reach unused?
        DXT3a_alpha = 35,
        DXT3a_mono = 36,
        DXT5a_alpha = 37,
        DXT5a_mono = 38, //Reach DXN
        DXN_mono_alpha = 39, //Reach CTX1
        Unknown40 = 40, //Reach DXT3a_mono
        Unknown41 = 41, //Reach DXT3a_alpha
        Unknown42 = 42, //Reach DXT5a_mono
        Unknown43 = 43, //Reach DXT5a_alpha
        Unknown44 = 44  //Reach DXNMA
    }

    public enum TextureType
    {
        Texture2D = 0,
        Texture3D = 1,
        CubeMap = 2,
        Sprite = 3,
        UIBitmap = 4
    }

    public enum SampleRate
    {
        _22050Hz = 0,
        _44100Hz = 1
    }

    public enum SoundType
    {
        Mono = 0,
        Stereo = 1,
        Unknown2 = 2, //2 and 3 probably surround stereo
        Unknown3 = 3
    }
}
