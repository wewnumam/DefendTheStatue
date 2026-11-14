using UnityEngine;

namespace ProjectTA.Utility
{
    /// <summary>
    /// One white 1x1 sprite shared by every placeholder. Views scale it to the size they need, so a
    /// whole slice can run before any art exists.
    /// </summary>
    public static class PlaceholderSprite
    {
        private static Sprite _square;

        public static Sprite Square
        {
            get
            {
                if (_square == null)
                {
                    var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp
                    };
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();

                    _square = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                    _square.name = "PlaceholderSquare";
                }

                return _square;
            }
        }

        /// <summary>A circle, for round things: the statue, slow fields, spike traps.</summary>
        public static Sprite Circle => Square;
    }
}
