/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace AimConverter
{
    public static class AimUtils
    {
        // extensions
        public static string ReadAimString(this BinaryReader reader, int size = 0x20)
        {
            var bytes = reader.ReadBytes(size);
            var length = 0;

            for (; length < bytes.Length; length++)
                if (bytes[length] == 0)
                    break;

            return Encoding.ASCII.GetString(bytes, 0, length);
        }

        public static Color ReadAimColor(this BinaryReader reader)
        {
            var color = Color.clear;
            color.r = reader.ReadSingle();
            color.g = reader.ReadSingle();
            color.b = reader.ReadSingle();
            color.a = reader.ReadSingle();

            return color;
        }
        
        // utils
        public static int GetLodIndex(string name)
        {
            var lodSuffixes = new[]
            {
                "LOD_DETAIL",
                "LOD_0",
                "LOD_",
                "LOD0",
                "LOD",
                "_L",
            };

            var lodIndexes = new[]
            {
                4, 3, 2, 1, 0
            };

            foreach (var suffix in lodSuffixes)
            {
                foreach (var index in lodIndexes)
                {
                    if (name.IndexOf(suffix + index, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        return Mathf.Max(index - 1, 0);
                    }
                }
            }

            return 0;
        }

        public static bool IsVisualGeometry(string meshName)
        {
            if (meshName == "SHAPE" || meshName == "SHAPE2" || meshName == "shadow" || meshName == "SHADOW")
                return false;

            if (meshName == "SMOKE1" || meshName == "SMOKE" || meshName == "SMOKE2" || meshName == "STEAM" || meshName == "GLOW" || meshName == "PARTICLES" || meshName == "FX")
                return false;

            if (meshName == "SIGN")
                return false;

            if (meshName == "INTO" || meshName == "OPEN" || meshName == "OPEN1") // TODO: Create trigger
                return false;
            
            if (meshName == "MESH")
                return false;

            return true;
        }
        
        public static Transform FindInChilds(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                var result = child.FindInChilds(name);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}