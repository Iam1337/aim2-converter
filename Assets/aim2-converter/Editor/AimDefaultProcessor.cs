/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using UnityEngine;

namespace AimConverter
{
    public class AimDefaultProcessor : IAimProcessor
    {
        #region Private Static Vars

        private static readonly int _mainTextureKey = Shader.PropertyToID("_MainTex");
        private static readonly int _mainColorKey = Shader.PropertyToID("_Color");
        private static readonly int _detailMaskKey = Shader.PropertyToID("_DetailMask");
        private static readonly int _detailColorKey = Shader.PropertyToID("_DetailColor");

        #endregion

        #region Public Vars

        public int Index => -1;
        public string Name => "Default Processor";
        public float ModelScale => 0.0254f;
        public bool UseBumpMap => false;
        public float BumpMapStrength => 0;

        #endregion

        #region Public Methods

        public ImportType SetupImportType(AimSubMesh subMesh)
        {
            var meshName = subMesh.NicifyName;
            if (!AimUtils.IsVisualGeometry(meshName))
                return ImportType.None;

            var lodIndex = AimUtils.GetLodIndex(meshName);
            if (lodIndex > 0)
                return ImportType.None;

            return ImportType.Visual;
        }

        public void ProcessPrefabObject(GameObject prefabObject, GameObject modelObject)
        { }

        public void ProcessPrefabSubMesh(GameObject prefabObject, GameObject modelObject, AimSubMesh subMesh, GameObject subMeshObject)
        { }

        public Material CreateMaterial(MaterialType materialType, Texture2D albedoTexture, Color albedoColor,
            Texture2D specularTexture, Color specularColor, Texture2D unknown3Texture, Texture2D unknown4Texture,
            Texture2D normalTexture)
        {
            var material = (Material)null;
            
            // TODO: Create shaders for importer.
            switch (materialType)
            {
                case MaterialType.MaterialOnly:
                case MaterialType.Texture:
                case MaterialType.TextureWithGlareMap:
                case MaterialType.TextureWithGlareMap2:
                case MaterialType.TextureWithGlareMapAndMask:
                case MaterialType.TextureWithMask:
                case MaterialType.DetalizationObjectStone:
                case MaterialType.TextureWithDetalizationMap:
                case MaterialType.TextureWithDetalizationMapWithoutModulation:
                case MaterialType.AlphaTextureNoGlare:
                case MaterialType.AlphaTextureWithOverlap:
                case MaterialType.AlphaTextureDoubleSided:
                case MaterialType.DetalizationObjectGrass:
                case MaterialType.Fire:
                case MaterialType.TiledTexture:
                case MaterialType.Fire2:
                {
                    material = new Material(Shader.Find("Standard (Specular setup)"));
                    material.SetTexture(_mainTextureKey, albedoTexture);
                    material.SetColor(_mainColorKey, albedoColor);
            
                    break;
                }
                default:
                {
                    Debug.Log($"[DEFAULT PROCESSOR] Unknown material type: {materialType}");
                    break;
                }
            }

            return material;
        }
        
        #endregion
    }
}
