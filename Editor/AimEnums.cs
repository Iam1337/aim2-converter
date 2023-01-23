/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */
// Based on: https://github.com/aimrebirth/tools

using System;

namespace AimConverter
{
    public enum SubMeshType
    {
        VisibleObject,
        HelperObject,
        BitmapAlpha,
        BitmapGrass,
        ParticleEmitter,
    };

    public enum MaterialType
    {
        Texture = 0x0,
        TextureWithGlareMap = 0x1,
        AlphaTextureNoGlare = 0x2,
        AlphaTextureWithOverlap = 0x3,
        TextureWithGlareMap2 = 0x4, 
        AlphaTextureDoubleSided = 0x6,
        DetalizationObjectGrass = 0x8,
        Fire = 0x9,
        MaterialOnly = 0x14,
        TextureWithDetalizationMap = 0x1A,
        DetalizationObjectStone = 0x1F,
        TextureWithDetalizationMapWithoutModulation = 0x20,
        TiledTexture = 0x22,
        TextureWithGlareMapAndMask = 0x32,
        TextureWithMask = 0x35,
        Fire2 = 0x3D,
    }

    [Flags]
    public enum ImportType
    {
        None = 0,
        Visual = 1 << 0,
        Collider = 1 << 1, 
        // WIP
    }
}