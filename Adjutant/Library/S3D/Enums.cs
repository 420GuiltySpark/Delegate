﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.S3D
{
    public enum TagType : int
    {
        None = 0,
        SceneData = 1,
        CacheBlock = 2,
        Shader = 3,
        ShaderCache = 4,
        TexturesInfo = 5,
        Textures = 6,
        TexturesMips64 = 7,
        SoundData = 8,
        Sounds = 9,
        WaveBanks_mem = 10,
        WaveBanks_strm_file = 11,
        Templates = 12,
        VoiceSplines = 13,
        Strings = 14,
        Ragdolls = 15,
        Scene = 16,
        Hkx = 17,
        Gfx = 18,
        TexturesDistanceFile = 19,
        CheckPointTexFile = 20,
        LoadingScreenGfx = 21,
        SceneGrs = 22,
        SceneScr = 23,
        SceneAnimbin = 24,
        SceneRain = 25,
        SceneCDT = 26,
        SceneSM = 27,
        SceneSLO = 28,
        SceneVis = 29,
        AnimStream = 30,
        AnimBank = 31,
    }
}
