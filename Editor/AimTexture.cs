using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AimConverter
{
    public class AimTexture
    {
        private unsafe class Block
        {
            [StructLayout(LayoutKind.Explicit)]
            private struct AlphaPart
            {
                [FieldOffset(0)] public ulong Alpha;
                [FieldOffset(0)] public fixed byte A[8];
            }

            [StructLayout(LayoutKind.Explicit)]
            private struct ColorPart
            {
                [FieldOffset(0)] public ulong Color;
                [FieldOffset(0)] public fixed ushort C[4];
            }

            public readonly Color32[] Pixels = new Color32[16];
            public bool HasAlpha;

            public void LoadData(BinaryReader reader)
            {
                AlphaPart rawAlpha;
                ColorPart rawColor;
                var alpha = new byte[8];
                var color = new Color32[4];
                
                rawAlpha.Alpha = reader.ReadUInt64();
                rawColor.Color = reader.ReadUInt64();
                
                // alpha
                alpha[0] = rawAlpha.A[0];
                alpha[1] = rawAlpha.A[1];
                if (alpha[0] > alpha[1])
                {
                    alpha[2] = (byte)((6 * alpha[0] + 1 * alpha[1]) / 7.0);
                    alpha[3] = (byte)((5 * alpha[0] + 2 * alpha[1]) / 7.0);
                    alpha[4] = (byte)((4 * alpha[0] + 3 * alpha[1]) / 7.0);
                    alpha[5] = (byte)((3 * alpha[0] + 4 * alpha[1]) / 7.0);
                    alpha[6] = (byte)((2 * alpha[0] + 5 * alpha[1]) / 7.0);
                    alpha[7] = (byte)((1 * alpha[0] + 6 * alpha[1]) / 7.0);
                }
                else
                {
                    alpha[2] = (byte)((4 * alpha[0] + 1 * alpha[1]) / 5.0);
                    alpha[3] = (byte)((3 * alpha[0] + 2 * alpha[1]) / 5.0);
                    alpha[4] = (byte)((2 * alpha[0] + 3 * alpha[1]) / 5.0);
                    alpha[5] = (byte)((1 * alpha[0] + 4 * alpha[1]) / 5.0);
                    alpha[6] = 0;
                    alpha[7] = 255;
                }

                // color
                color[0] = GetColor565(rawColor.C[0]);
                color[1] = GetColor565(rawColor.C[1]);
                if (rawColor.C[0] > rawColor.C[1])
                {
                    color[2] = interpolate(color[0], color[1], 2f / 3f);
                    color[3] = interpolate(color[0], color[1], 1f / 3f);
                }
                else
                {
                    color[2] = interpolate(color[0], color[1], 1f / 2f);
                    color[3] = new Color32(0, 0, 0, 0);
                }

                for (var i = 0; i < 16; i++)
                {
                    Pixels[i] = color[(rawColor.Color >> (32 + i * 2)) & 0b11];
                    Pixels[i].a = alpha[(rawAlpha.Alpha >> (16 + i * 3)) & 0b111];

                    HasAlpha = Pixels[i].a < byte.MaxValue;
                }
            }

            private Color32 interpolate(Color32 c0, Color32 c1, float m)
            {
                var r = new Color32();
                for (var i = 0; i < 4; i++)
                    r[i] = (byte)(c0[i] * m + c1[i] * (1 - m));

                return r;
            }

            // TODO: To utils
            private Color32 GetColor565(ushort c)
            {
                var Table5 = new byte[]
                {
                    0, 8, 16, 25, 33, 41, 49, 58, 66, 74, 82, 90, 99, 107, 115, 123, 132,
                    140, 148, 156, 165, 173, 181, 189, 197, 206, 214, 222, 230, 239, 247, 255
                };
                var Table6 = new byte[]
                {
                    0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 45, 49, 53, 57, 61, 65, 69,
                    73, 77, 81, 85, 89, 93, 97, 101, 105, 109, 113, 117, 121, 125, 130, 134, 138,
                    142, 146, 150, 154, 158, 162, 166, 170, 174, 178, 182, 186, 190, 194, 198,
                    202, 206, 210, 215, 219, 223, 227, 231, 235, 239, 243, 247, 251, 255
                };

                var r = new Color32();
                r.b = Table5[(c & 0x1F)];
                r.g = Table6[(c >> 5) & 0x3F];
                r.r = Table5[(c >> 11) & 0x1F];

                return r;
            }

        }

        public string Name => _name;
        public int Width => _width;
        public int Height => _height;
        public bool HasAlpha => _hasAlpha;

        private readonly string _name;
        private int _width;
        private int _height;
        private bool _hasAlpha;
        private Block[] _blocks;

        public AimTexture(string name)
        {
            _name = name;
        }

        public void LoadData(BinaryReader reader)
        {
            _width = reader.ReadInt32();
            _height = reader.ReadInt32();

            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            var dx5 = reader.ReadByte() > 0;
            reader.BaseStream.Seek(0x4C, SeekOrigin.Begin);

            if (dx5)
            {
                var blocksCount = _width * _height / 16;
                
                _blocks = new Block[blocksCount];
                for (var i = 0; i < blocksCount; i++)
                {
                    var block = new Block();
                    block.LoadData(reader);

                    _hasAlpha |= block.HasAlpha;
                    _blocks[i] = block;
                }
            }
            else
            {
                Debug.LogWarning("[TEXTURE] Unsupported texture type.");
            }
        }

        public Texture2D BuildTexture()
        {
            var texture = new Texture2D(_width, _height, TextureFormat.RGBA32, true);
            texture.SetPixels32(GetPixelsDX5());
            texture.Apply();

            return texture;
        }

        internal Color32[] GetPixelsDX5()
        {
            var buffer = new Color32[_width * _height];
            var xsegs = _width / 4;
            for (int index = 0; index < _blocks.Length; index++)
            {
                var block = _blocks[index];
                var xseg = index % xsegs;
                var yseg = index / xsegs;
                for (int i = 0; i < 4; i++)
                {
                    var row = _height - 1 - (yseg * 4 + i);
                    var col = xseg * 4;

                    var ras = row * _width + col;
                    Array.Copy(block.Pixels, i * 4, buffer, ras, 4);
                }
            }

            return buffer;
        }
    }
}