using UnityEngine;

namespace AimConverter.UI
{
    public static class AimTextures
    {
        private static Texture2D _splitterTexture;

        public static Texture2D Splitter
        {
            get
            {
                if (_splitterTexture == null)
                {
                    _splitterTexture = new Texture2D(2, 2);
                    _splitterTexture.hideFlags = HideFlags.DontSave;

                    var colors = new Color32[_splitterTexture.height * _splitterTexture.width];

                    for (var i = 0; i < colors.Length; i++)
                    {
                        colors[i] = new Color(0f, 0f, 0f, 0.5f);
                    }

                    _splitterTexture.SetPixels32(colors);
                    _splitterTexture.Apply();
                }

                return _splitterTexture;
            }
        }
    }
}