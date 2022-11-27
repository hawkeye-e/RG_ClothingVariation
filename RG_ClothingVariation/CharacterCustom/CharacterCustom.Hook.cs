using BepInEx.Logging;
using HarmonyLib;
using CharaCustom;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

//Note:
//Custom use of hideOpt: true: half-dressing, false: full-dressing
//Coordinatetype = outside / home / bath

namespace RGClothingVariation.CharacterCustom
{
    internal class Hook
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        //Reflect the selected state after clothes category changed
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CvsC_Clothes), nameof(CvsC_Clothes.UpdateCustomUI))]
        private static void UpdateCustomUIPost(CvsC_Clothes __instance)
        {

            Patches.UpdateClothesStateToggleButton(__instance.chaCtrl, __instance.coordinateType, __instance.SNo);
            Patches.ReflectClothesState(__instance.chaCtrl, __instance.coordinateType);
        }

        //Reset Clothes state when switching coordinate type
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeCoordinateType), new[] { typeof(Chara.ChaFileDefine.CoordinateType), typeof(bool), typeof(bool) })]
        private static void ChangeCoordinateTypePost(Chara.ChaControl __instance, Chara.ChaFileDefine.CoordinateType type, bool reload, bool forceChange)
        {
            Patches.ResetClothesState(__instance);
            if (RG.Scene.ActionScene.Instance != null)
            {
                Patches.ReflectClothesState(__instance, (int)type);
            }
        }

        //Update the clothes state after changing clothes
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBase), nameof(CustomBase.ChangeClothesState))]
        private static void ChangeClothesStatePost(CustomBase __instance, int stateNo)
        {
            Patches.ReflectClothesState(__instance.chaCtrl, __instance.chaCtrl.FileStatus.coordinateType);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.AssignCoordinate), new[] { typeof(Chara.ChaFileDefine.CoordinateType) })]
        private static void AssignCoordinatePost(Chara.ChaControl __instance, Chara.ChaFileDefine.CoordinateType type)
        {
            Patches.ReflectClothesState(__instance, (int)type);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBase), nameof(CustomBase.ChangeClothesStateAuto))]
        private static void ChangeClothesStateAutoPost(CustomBase __instance, int stateNo)
        {
            if (stateNo == 0)
                Patches.ReflectClothesState(__instance.chaCtrl, __instance.chaCtrl.FileStatus.coordinateType);
        }

        //Always show the "Others" menu for female body
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CvsC_Clothes), nameof(CvsC_Clothes.RestrictClothesMenu))]
        private static void RestrictClothesMenuPost(CvsC_Clothes __instance)
        {
            Patches.ShowTabOthers(__instance);
        }

        //Copy the clothes state when using the copy function
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CvsC_Copy), nameof(CvsC_Copy.Copy))]
        private static void CvsC_Copy_CopyPost(CvsC_Copy __instance)
        {
            Patches.CopyClothesStates(__instance);
            Patches.ReflectClothesState(__instance.chaCtrl, __instance.chaCtrl.FileStatus.coordinateType);
        }

        //Attach the UI to the Character Custom Screen
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharaCustom.CustomControl), nameof(CharaCustom.CustomControl.InitializeUI))]
        private static void InitializeUI(CustomControl __instance)
        {
            Patches.SetupStateManager(__instance);
            Patches.SetupCustomUIForClothesState(__instance);
        }
    }
}
