/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */
// Based on: https://github.com/aimrebirth/tools

using System.IO;
using System.Linq;
using UnityEngine;

namespace AimConverter
{
    public class AimSubMesh
    {
        // shared
        public SubMeshType Type;
        public string Name;
        public string NicifyName => Name.Replace("?", "N").ToUpperInvariant();
        
        // import
        public ImportType ImportType;

        // materials
        public string AlbedoName;
        public string SpecularName;
        public string Unknown3Name;
        public string Unknown4Name;

        public Color DiffuseColor;
        public Color AlbedoColor;
        public Color SpecularColor;
        public Color EmissiveColor;
        public float Power;
        public MaterialType MaterialType;

        private bool _import;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector2[] _uvs;
        private int[] _triangles;

        public void ReadData(BinaryReader reader)
        {
            // header
            Type = (SubMeshType)reader.ReadUInt32();
            Name = reader.ReadAimString();
            AlbedoName = reader.ReadAimString();
            SpecularName = reader.ReadAimString();
            Unknown3Name = reader.ReadAimString();
            Unknown4Name = reader.ReadAimString();

            reader.ReadByte(); // lod1
            reader.ReadByte(); // lod2
            reader.ReadByte(); // lod3
            reader.ReadByte(); // lod4

            reader.ReadInt32(); // unk2
            reader.ReadInt32(); // unk2
            reader.ReadInt32(); // unk2
            reader.ReadInt32(); // unk3

            var size = (int)reader.ReadUInt32();

            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4
            reader.ReadSingle(); // unk4

            var subMeshBuffer = reader.ReadBytes(size);
            using var subMeshStream = new MemoryStream(subMeshBuffer);
            using var subMeshReader = new BinaryReader(subMeshStream);

            // animation count
            var animationCount = subMeshReader.ReadUInt32();

            // material
            DiffuseColor = subMeshReader.ReadAimColor();
            AlbedoColor = subMeshReader.ReadAimColor();
            SpecularColor = subMeshReader.ReadAimColor();
            EmissiveColor = subMeshReader.ReadAimColor();
            Power = subMeshReader.ReadSingle();
            MaterialType = (MaterialType)subMeshReader.ReadUInt32();

            // atex
            subMeshReader.ReadUInt16();
            subMeshReader.ReadByte();
            subMeshReader.ReadByte();
            subMeshReader.ReadSingle();

            subMeshReader.ReadUInt32(); // unk10

            subMeshReader.ReadUInt32(); // animationAuto
            subMeshReader.ReadSingle(); // animationCycle

            subMeshReader.ReadSingle(); // unk8
            subMeshReader.ReadUInt32(); // unk11
            subMeshReader.ReadUInt32(); // unk12

            subMeshReader.ReadUInt32(); // triangles_mult_7

            // aditional params
            subMeshReader.ReadUInt32();
            subMeshReader.ReadSingle();

            subMeshReader.ReadUInt32(); // damageModelsCount

            // rotation
            var rotationType = subMeshReader.ReadUInt32();
            var rotationSpeed = subMeshReader.ReadSingle();
            var axisX = subMeshReader.ReadSingle();
            var axisY = subMeshReader.ReadSingle();
            var axisZ = subMeshReader.ReadSingle();

            // flags
            var flags = subMeshReader.ReadUInt32();
            if (flags == 0)
                return;

            // geometry
            var verticesCount = subMeshReader.ReadUInt32();
            var trianglesCount = subMeshReader.ReadUInt32();

            _vertices = new Vector3[verticesCount];
            _normals = new Vector3[verticesCount];
            _uvs = new Vector2[verticesCount];
            _triangles = new int[trianglesCount];

            var tempVector3 = Vector3.zero;
            var tempVector2 = Vector2.zero;
            for (var i = 0; i < verticesCount; i++)
            {
                tempVector3.x = subMeshReader.ReadSingle();
                tempVector3.z = -subMeshReader.ReadSingle();
                tempVector3.y = subMeshReader.ReadSingle();
                _vertices[i] = tempVector3;

                if ((flags & 0x4) != 0)
                {
                    subMeshReader.ReadSingle(); // exclude wind
                }

                tempVector3.x = subMeshReader.ReadSingle();
                tempVector3.z = -subMeshReader.ReadSingle();
                tempVector3.y = subMeshReader.ReadSingle();
                _normals[i] = tempVector3;

                tempVector2.x = subMeshReader.ReadSingle();
                tempVector2.y = 1 - subMeshReader.ReadSingle();
                _uvs[i] = tempVector2;
            }

            for (var i = 0; i < trianglesCount; i++)
                _triangles[i] = subMeshReader.ReadUInt16();
        }

        public Mesh BuildMesh(float scale = 1f)
        {
            var mesh = new Mesh();
            mesh.SetVertices(_vertices.Select(v => v * scale).ToArray());
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(_triangles, 0);
            
            return mesh;
        }
    }
}