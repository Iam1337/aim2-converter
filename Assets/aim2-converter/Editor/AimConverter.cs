/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AimConverter
{
    public class AimConverter
    {
        public static string ContentDirectory => CreateDirectory("Assets", "AimContent");
        public static IAimProcessor DefaultProcessor => _defaultProcessor;

        private static readonly IAimProcessor _defaultProcessor = new AimDefaultProcessor();

        public static AimModel LoadModel(IAimProcessor processor, string modelPath)
        {
            using var stream = new FileStream(modelPath, FileMode.Open);
            using var reader = new BinaryReader(stream);

            try
            {
                var model = new AimModel(Path.GetFileNameWithoutExtension(modelPath).Trim());
                model.ReadData(reader);
                
                foreach (var subMesh in model.SubMeshes)
                {
                    subMesh.ImportType = processor.SetupImportType(subMesh);
                }

                return model;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CONVERTER] Failed open model: {modelPath}\n{e}");
            }

            return null;
        }
        
        public static GameObject Import(IAimProcessor processor, AimModel model, string targetContentDirectory, string sourceTextureDirectory)
        {
            if (model == null)
                throw new ArgumentException(nameof(model));

            var modelObject = (GameObject)null;
            var prefabObject = (GameObject)null;
            
            try
            {
                var modelDirectory = CreateDirectory(targetContentDirectory, model.Name);
                var modelDictionary = new Dictionary<AimSubMesh, string>();
                
                modelObject = CreateModel(model, modelDirectory, processor.ModelScale, modelDictionary, true);
                SetupMaterials(processor, model, modelObject, modelDirectory, sourceTextureDirectory, modelDictionary, true);
                
                prefabObject = CreatePrefab(processor, model, modelObject, modelDictionary);
                PrefabUtility.SaveAsPrefabAsset(prefabObject, Path.Combine(modelDirectory, $"{model.Name.Replace("MOD_", "PREFAB_")}.prefab"));
            }
            catch (Exception exception)
            {
                if (modelObject != null)
                    Object.DestroyImmediate(modelObject);
                if (prefabObject != null)
                    Object.DestroyImmediate(prefabObject);

                prefabObject = null;
                
                Debug.LogError($"[CONVERTER] Model: {model.Name}\n{exception}");
            }

            return prefabObject;
        }
        
        public static GameObject Preview(IAimProcessor processor, AimModel model, string sourceTextureDirectory)
        {
            if (model == null)
                throw new ArgumentException(nameof(model));

            var modelObject = (GameObject)null;
            
            try
            {
                var modelDictionary = new Dictionary<AimSubMesh, string>();
                
                modelObject = CreateModel(model, null, processor.ModelScale, modelDictionary, false);
                SetupMaterials(processor, model, modelObject, null, sourceTextureDirectory, modelDictionary, false);
            }
            catch (Exception exception)
            {
                if (modelObject != null)
                    Object.DestroyImmediate(modelObject);

                modelObject = null;
                
                Debug.LogError($"[CONVERTER] Model: {model.Name}\n{exception}");
            }

            return modelObject;
        }

        private static string CreateDirectory(string parent, string name)
        {
            var path = Path.Combine(parent, name);
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }

            return path;
        }
        
        private static GameObject CreateModel(AimModel model, string modelDirectory, float modelScale, IDictionary<AimSubMesh, string> modelDictionary, bool dumpAsset)
        {
            var modelObject = new GameObject(model.Name);
            modelObject.transform.localPosition = Vector3.zero;
            modelObject.transform.localRotation = Quaternion.identity;
            modelObject.transform.localScale = Vector3.one;

            var index = 0;
            foreach (var subMesh in model.SubMeshes)
            {
                if ((subMesh.ImportType & ImportType.Visual) == 0)
                    continue;
                
                var subMeshName = $"GEOMETRY{index++}";
                var subMeshObject = new GameObject();
                subMeshObject.name = subMeshName;
                subMeshObject.transform.parent = modelObject.transform;
                subMeshObject.transform.localPosition = Vector3.zero;
                subMeshObject.transform.localRotation = Quaternion.identity;
                subMeshObject.transform.localScale = Vector3.one;

                try
                {
                    var subMeshFilter = subMeshObject.AddComponent<MeshFilter>();
                    subMeshFilter.mesh = subMesh.BuildMesh(modelScale);

                    var material = new Material(Shader.Find("Standard"));
                    material.name = subMeshName;

                    var subMeshRender = subMeshObject.AddComponent<MeshRenderer>();
                    subMeshRender.material = material;
                    
                    modelDictionary.Add(subMesh, subMeshName);
                }
                catch (Exception e)
                {
                    Object.DestroyImmediate(subMeshObject);

                    Debug.LogError($"[CONVERTER] Exclude submesh {subMesh.Name} because:\n{e}");
                }
            }

            if (dumpAsset)
            {
                var modelPath = Path.Combine(modelDirectory, model.Name) + ".fbx";
                ModelExporter.ExportObject(modelPath, modelObject);
                Object.DestroyImmediate(modelObject);
                return PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(modelPath)) as GameObject;
            }

            return modelObject;
        }

        private static GameObject CreatePrefab(IAimProcessor processor, AimModel model, GameObject modelObject, IDictionary<AimSubMesh, string> modelDictionary)
        {
            var prefabObject = new GameObject();
            prefabObject.name = modelObject.name;

            modelObject.transform.parent = prefabObject.transform;
            modelObject.transform.localPosition = Vector3.zero;
            modelObject.transform.localRotation = Quaternion.identity;
            modelObject.transform.localScale = Vector3.one;
            
            processor.ProcessPrefabObject(prefabObject, modelObject);

            foreach (var subMesh in model.SubMeshes)
            {
                var subMeshObject = (GameObject)null;
                if (modelDictionary.TryGetValue(subMesh, out var subMeshName))
                {
                    var subMeshTransform = modelObject.transform.FindInChilds(subMeshName);
                    if (subMeshTransform != null)
                        subMeshObject = subMeshTransform.gameObject;
                }

                processor.ProcessPrefabSubMesh(prefabObject, modelObject, subMesh, subMeshObject);
            }

            return prefabObject;
        }

        private static void SetupMaterials(IAimProcessor processor, AimModel model, GameObject modelObject, string modelDirectory, string sourceTextureDirectory, Dictionary<AimSubMesh, string> modelDictionary, bool dumpAsset)
        {
            foreach (var pair in modelDictionary)
            {
                var subMesh = pair.Key;
                var subMeshName = pair.Value;
                
                var subMeshObject = modelObject.transform.FindInChilds(subMeshName);
                if (subMeshObject == null)
                {
                    Debug.LogError($"[CONVERTER] SubMesh with name {subMeshName} not found in model {model.Name}");
                    continue;
                }

                var subMeshRender = subMeshObject.GetComponent<MeshRenderer>();
                if (subMeshRender == null)
                {
                    Debug.LogError($"[CONVERTER] Renderer with name {subMeshName} not found in model {model.Name}");
                    continue;
                }

                var material = CreateMaterial(processor, subMesh, subMeshName, modelDirectory, sourceTextureDirectory, dumpAsset);
                if (material != null)
                    subMeshRender.material = material;
            }
        }

        private static Material CreateMaterial(IAimProcessor processor, AimSubMesh subMesh, string subMeshName, string modelDirectory, string sourceTextureDirectory, bool dumpAsset)
        {
            var albedoTexture = CreateTexture(subMesh.AlbedoName, subMeshName, modelDirectory, sourceTextureDirectory, dumpAsset);
            var albedoColor = subMesh.AlbedoColor;

            var specularTexture = CreateTexture(subMesh.SpecularName, $"{subMeshName}_SPEC", modelDirectory, sourceTextureDirectory, dumpAsset);
            var specularColor = subMesh.SpecularColor;

            var unknown3Texture = CreateTexture(subMesh.Unknown3Name, $"{subMeshName}_UNK3", modelDirectory, sourceTextureDirectory, dumpAsset);
            var unknown4Texture = CreateTexture(subMesh.Unknown4Name, $"{subMeshName}_UNK4", modelDirectory, sourceTextureDirectory, dumpAsset);

            var normalTexture = (Texture2D)null;
            if (processor.UseBumpMap)
            {
                normalTexture = CreateNormalTexture(subMesh.AlbedoName, $"{subMeshName}_BUMP", modelDirectory, sourceTextureDirectory,
                    processor.BumpMapStrength, dumpAsset);
            }

            var material = processor.CreateMaterial(subMesh.MaterialType,
                                                     albedoTexture, albedoColor,
                                                     specularTexture, specularColor,
                                                     unknown3Texture,
                                                     unknown4Texture, 
                                                     normalTexture);

            if (material != null && dumpAsset)
                AssetDatabase.CreateAsset(material, Path.Combine(modelDirectory, $"MAT_{subMeshName}.mat"));

            return material;
        }

        private static Texture2D CreateTexture(string textureName, string assetName, string modelDirectory, string sourceTextureDirectory, bool save)
        {
            var albedoPath = Path.Combine(sourceTextureDirectory, textureName) + ".TM";
            if (!File.Exists(albedoPath))
                return null;

            using var stream = new FileStream(albedoPath, FileMode.Open);
            using var reader = new BinaryReader(stream);

            var textureRaw = new AimTexture(Path.GetFileNameWithoutExtension(albedoPath));
            textureRaw.LoadData(reader);

            var tempTexture = textureRaw.BuildTexture();
            tempTexture.Apply();

            if (save)
            {
                var tempPath = Path.Combine(modelDirectory, $"TEX_{assetName}.png");

                File.WriteAllBytes(tempPath, tempTexture.EncodeToPNG());
                AssetDatabase.ImportAsset(tempPath, ImportAssetOptions.Default);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(tempPath);
            }

            return tempTexture;
        }
        
        private static Texture2D CreateNormalTexture(string textureName, string assetName, string modelDirectory, string sourceTextureDirectory, float strength, bool save)
        {
            var albedoPath = Path.Combine(sourceTextureDirectory, textureName) + ".TM";
            if (!File.Exists(albedoPath))
                return null;

            using var stream = new FileStream(albedoPath, FileMode.Open);
            using var reader = new BinaryReader(stream);

            var textureRaw = new AimTexture(Path.GetFileNameWithoutExtension(albedoPath));
            textureRaw.LoadData(reader);
            
            var sourceTexture = textureRaw.BuildTexture();
            var normalTexture = new Texture2D (sourceTexture.width, sourceTexture.height, TextureFormat.ARGB32, true);
            
            strength = Mathf.Clamp(strength,0.0F,1.0F);
            
            // https://gamedev.stackexchange.com/questions/106703/create-a-normal-map-using-a-script-unity
            for (var y=0; y<normalTexture.height; y++) 
            {
                for (var x=0; x<normalTexture.width; x++) 
                {
                    var xLeft = sourceTexture.GetPixel(x-1,y).grayscale*strength;
                    var xRight = sourceTexture.GetPixel(x+1,y).grayscale*strength;
                    var yUp = sourceTexture.GetPixel(x,y-1).grayscale*strength;
                    var yDown = sourceTexture.GetPixel(x,y+1).grayscale*strength;
                    var xDelta = ((xLeft-xRight)+1)*0.5f;
                    var yDelta = ((yUp-yDown)+1)*0.5f;
                    normalTexture.SetPixel(x,y,new Color(xDelta,yDelta,1.0f,yDelta));
                }
            }
            
            Object.DestroyImmediate(sourceTexture);
            normalTexture.Apply();
            
            if (save)
            {
                var tempPath = Path.Combine(modelDirectory, $"TEX_{assetName}.png");

                File.WriteAllBytes(tempPath, normalTexture.EncodeToPNG());
                AssetDatabase.ImportAsset(tempPath, ImportAssetOptions.Default);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(tempPath);
            }
            
            return normalTexture;
        }
    }
}