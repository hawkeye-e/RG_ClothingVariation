using UnityEngine;
using UnityEngine.UI;
using CharaCustom;

namespace RGClothingVariation
{
    internal class StateManager
    {
        internal static StateManager Instance;

        internal AssetBundle ClothesStateAB = null;
        internal Canvas ClothesStateCanvas = null;
        internal Toggle FullDressingToggle = null;
        internal Toggle HalfDressingToggle = null;
        internal Chara.ChaControl CharacterControl = null;
        internal CvsC_Clothes ClothesCanvas = null;
        internal Font YuGothicFont = null;
    }
}