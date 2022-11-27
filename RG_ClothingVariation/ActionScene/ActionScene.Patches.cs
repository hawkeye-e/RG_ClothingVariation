using BepInEx.Logging;

namespace RGClothingVariation.ActionScene
{
    internal class Patches
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        //Set the clothes state of the model in main game
        internal static void ReflectClothesState(Chara.ChaControl character, int coordinateType, int clothesType)
        {
            //havent set any clothes state, do nothing
            if (character.NowCoordinate.clothes.parts.Count == 8)
            {
                return;
            }

            var clothesStateInfo = character.NowCoordinate.clothes.parts[8];
            character.FileStatus.clothesState[clothesType] = System.Math.Max(character.FileStatus.clothesState[clothesType], clothesStateInfo.hideOpt[clothesType] ? (byte)1 : (byte)0);
        }
    }
}
