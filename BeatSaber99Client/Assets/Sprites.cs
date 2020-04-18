using BeatSaberMarkupLanguage;
using UnityEngine;

namespace BeatSaber99Client.Assets
{
    public static class Sprites
    {
        public static Sprite logoIcon;

        public static void Init()
        {
            logoIcon = Utilities.FindSpriteInAssembly("BeatSaber99Client.Assets.Logo.png");
            logoIcon.texture.wrapMode = TextureWrapMode.Clamp;
        }
    }
}