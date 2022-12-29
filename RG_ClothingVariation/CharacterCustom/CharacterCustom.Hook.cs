using BepInEx.Logging;
using HarmonyLib;
using CharaCustom;
using UnityEngine.UI;
using UnityEngine;
using System;

//Note:
//Custom use of hideOpt: true: half-dressing, false: full-dressing | Top part id: 0-7, Bottom part id: 8-15
//              colorInfo.layout.x : clothes type
//              colorInfo.layout.y : clothes id
//              id: version number, in case find a better way to store the data and need data migration later
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
            if (!Util.IsCharacterFemaleBody(__instance.chaCtrl)) return;
 
            Patches.UpdateClothesStateToggleButton(__instance.chaCtrl, __instance.coordinateType, __instance.SNo);
            Patches.UpdateClothesTypeToggle(__instance.chaCtrl, __instance.SNo);

            if (StateManager.Instance.IsClothesSlotSelectChanged)       
            {
                Patches.UpdateClothesSelectionList(__instance.SNo, true);
            }

            Patches.UpdateClothesCanvasScrollViewText(__instance.chaCtrl, __instance.SNo);
            Patches.ReflectClothesState(__instance.chaCtrl, __instance.coordinateType);
        }

        //Reset Clothes state when switching coordinate type
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeCoordinateType), new[] { typeof(Chara.ChaFileDefine.CoordinateType), typeof(bool), typeof(bool) })]
        private static void ChangeCoordinateTypePost(Chara.ChaControl __instance, Chara.ChaFileDefine.CoordinateType type, bool reload, bool forceChange)
        {
            if (!Util.IsCharacterFemaleBody(__instance)) return;

            Patches.ResetClothesState(__instance);
            if (RG.Scene.ActionScene.Instance != null)
            {
                Patches.ReflectClothesState(__instance, (int)type);
            }
        }

        //Update the clothes state after changing clothes
        //stateno: 0 = default, 1 = full wear, 2=underwear, 3=nude
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBase), nameof(CustomBase.ChangeClothesState))]
        private static void ChangeClothesStatePost(CustomBase __instance, int stateNo)
        {
            if (!Util.IsCharacterFemaleBody(__instance.chaCtrl)) return;

            //Force to full wear when saving thumbnail
            Patches.HandleThumbnailSaving(stateNo);
            Patches.ReflectClothesState(__instance.chaCtrl, __instance.chaCtrl.FileStatus.coordinateType);
        }

        //Load Clothes set card
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.AssignCoordinate), new[] { typeof(Chara.ChaFileDefine.CoordinateType) })]
        private static void AssignCoordinatePost(Chara.ChaControl __instance, Chara.ChaFileDefine.CoordinateType type)
        {
            if (!Util.IsCharacterFemaleBody(__instance)) return;

            Patches.ReflectClothesState(__instance, (int)type);
        }

        //Force to change to no clothes first so that no transparent skin is shown
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomBase), nameof(CustomBase.ChangeClothesStateAuto))]
        private static void ChangeClothesStateAutoPre(CustomBase __instance, ref int stateNo)
        {
            if (StateManager.Instance.IsMajorCategorySelectChanged && StateManager.Instance.IsClothesCategorySelected)
                stateNo = 2;
        }

        //Handle the case of switching between category
        //stateno: 0 = default, 1 = underwear, 2 = nude
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomBase), nameof(CustomBase.ChangeClothesStateAuto))]
        private static void ChangeClothesStateAutoPost(CustomBase __instance, int stateNo)
        {
            if (!Util.IsCharacterFemaleBody(__instance.chaCtrl)) return;

            Patches.HandleCategorySwitching(__instance, stateNo);
        }

        //Update "Others" menu for female body
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CvsC_Clothes), nameof(CvsC_Clothes.RestrictClothesMenu))]
        private static void RestrictClothesMenuPost(CvsC_Clothes __instance)
        {
            if (!Util.IsCharacterFemaleBody(__instance.chaCtrl)) return;

            Patches.ShowClothesStateTab(__instance);
        }

        //Copy the clothes state when using the copy function
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CvsC_Copy), nameof(CvsC_Copy.Copy))]
        private static void CvsC_Copy_CopyPost(CvsC_Copy __instance)
        {
            if (!Util.IsCharacterFemaleBody(__instance.chaCtrl)) return;

            Patches.CopyClothesStates(__instance);
            Patches.ReflectClothesState(__instance.chaCtrl, __instance.chaCtrl.FileStatus.coordinateType);
        }

        //Attach the UI to the Character Custom Screen
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharaCustom.CustomControl), nameof(CharaCustom.CustomControl.InitializeUI))]
        private static void InitializeUI(CustomControl __instance)
        {
            if (!Util.IsCharacterFemaleBody(__instance.chaCtrl)) return;

            Patches.SetupStateManager(__instance);
            Patches.SetupCustomUIForClothesState(__instance);
            Util.InitializeCharacterClothesExtraFields(__instance.chaCtrl);
            Patches.SetupCustomUIForClothesType(__instance);

        }

        //Remove the flags when exit chara custom screen
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharaCustom.CharaCustom), nameof(CharaCustom.CharaCustom.OnDestroy))]
        private static void CharaCustomOnDestroyPost(CustomControl __instance)
        {
            StateManager.Instance.IsCharaCustomScreen = false;
        }

        //Check Change clothes
        //kind: can be viewed as slot number
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeClothes), new[] { typeof(int), typeof(int), typeof(bool) })]
        private static void ChangeClothesPost(Chara.ChaControl __instance, int kind, int id, bool forceChange)
        {
            if (!Util.IsCharacterFemaleBody(__instance)) return;

            Patches.ChangeClothes(__instance, kind, id);
        }

        //Update the active status of game object directly to avoid unwanted effect for clothes placed on unmatch category
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
        private static void SetActivePost(GameObject __instance, bool value)
        {
            //TODO: revamp the logic of SetGameObjectActive (chara custom)
            Patches.SetCharaCustomGameObjectActive(__instance);
        }


        //There is exception thrown for some clothes item in some clothes slot when saving as clothes set card.
        //The cause is unknown yet, but it seems there is nothing wrong in the card data saved so just catch and suppress
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ChangeSettingMannequin))]
        private static Exception CatchChangeSettingMannequinErrors(Exception __exception)
        {
            if (__exception != null)
            {
                //Log.LogWarning("ChangeSettingMannequin error thrown");
            }
            return null;
        }

        //Return the clothes list by selected category
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.CustomTableData), nameof(Chara.CustomTableData.GetCategoryInfo))]
        private static void GetCategoryInfo(Chara.ChaListDefine.CategoryNo category, ref Illusion.Collections.Generic.Optimized.Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> __result)
        {
            var output = Patches.GetMappedCategoryInfo(category);
            if (output != null)
                __result = output;
        }

        //Set the flags when category or clothes slot is clicked
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Selectable), nameof(Selectable.OnPointerUp))]
        private static void OnPointerUp(Selectable __instance, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!StateManager.Instance.IsCharaCustomScreen) return;
            Patches.SetClothesCategorySlotFlag(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomSelectScrollController), nameof(CustomSelectScrollController.OnPointerExit))]
        private static void OnPointerExit(CustomSelectScrollController __instance)
        {
            if (!Util.IsCharacterFemaleBody(StateManager.Instance.CharacterControl)) return;
            Patches.SetClothesScrollerDisplayText(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomSelectScrollController), nameof(CustomSelectScrollController.OnValueChange))]
        private static void OnValueChange(CustomSelectScrollController __instance, CustomSelectScrollController.ScrollData _data, bool _isOn)
        {
            if (!Util.IsCharacterFemaleBody(StateManager.Instance.CharacterControl)) return;
            Patches.SetClothesScrollerDisplayText(__instance, _data);
        }


        //Apply the changes of the data when load a character
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.Reload))]
        private static void Reload(Chara.ChaControl __instance, bool noChangeClothes, bool noChangeHead, bool noChangeHair, bool noChangeBody, bool forceChange)
        {
            if (!StateManager.Instance.IsCharaCustomScreen) return;
            Util.InitializeCharacterClothesExtraFields(__instance);
        }
        
    }
}
