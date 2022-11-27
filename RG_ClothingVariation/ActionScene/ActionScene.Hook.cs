using BepInEx.Logging;
using HarmonyLib;

namespace RGClothingVariation.ActionScene
{
    internal class Hook
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetClothesState))]
        private static void SetClothesState(Chara.ChaControl __instance, int clothesKind, byte state, bool next)
        {
            if (RG.Scene.ActionScene.Instance != null)
            {
                Patches.ReflectClothesState(__instance, __instance.FileStatus.coordinateType, clothesKind);
            }
        }

        //Handle clothes state when changing Clothes in game
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeNowCoordinate), new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        private static void ChangeNowCoordinate2Pre(Chara.ChaControl __instance, string path, bool reload, bool forceChange, bool clothes, bool accessory, bool hair)
        {
            __instance.NowCoordinate.clothes.parts = Util.ExpandClothesStateArray(__instance.NowCoordinate.clothes.parts);
        }
    }


}
