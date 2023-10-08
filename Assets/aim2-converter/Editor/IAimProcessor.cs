/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using UnityEngine;

namespace AimConverter
{
    public interface IAimProcessor
    {
        int Index { get; }
        string Name { get; }
        float ModelScale { get; }
        bool UseBumpMap { get; }
        float BumpMapStrength { get; }
        
        ImportType SetupImportType(AimSubMesh subMesh);
        void ProcessPrefabObject(GameObject prefabObject, GameObject modelObject);
        void ProcessPrefabSubMesh(GameObject prefabObject, GameObject modelObject, AimSubMesh subMesh, GameObject subMeshObject);
        Material CreateMaterial(MaterialType materialType,
            Texture2D albedoTexture, Color albedoColor,
            Texture2D specularTexture, Color specularColor,
            Texture2D unknown3Texture,
            Texture2D unknown4Texture, Texture2D normalTexture);
    }
}