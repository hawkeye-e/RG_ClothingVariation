using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;

namespace RGClothingVariation.PatchInitialize
{
    internal class Hook
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;


        //Create a list with combined clothes data and replace the original listing
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TitleScene), nameof(TitleScene.ReleaseResources))]
        internal static void ReleaseResources()
        {
            Patches.InitializeCustomClothesMapping();
        }
    }
}
