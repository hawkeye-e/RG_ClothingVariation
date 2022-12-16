using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using UnityEngine;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;

namespace RGClothingVariation.ActionSceneScreen
{
    internal class Hook
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetClothesState))]
        private static void SetClothesState(Chara.ChaControl __instance, int clothesKind, byte state, bool next)
        {
            if (RG.Scene.ActionScene.Instance != null && !StateManager.Instance.IsHScene)
            {
                Patches.ReflectClothesState(__instance, __instance.FileStatus.coordinateType, clothesKind, state);
            }
        }

        //change to full list clothes id for each character
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.RegisterActor))]
        private static void RegisterActorPost(Actor actor)
        {
            Util.InitializeCharacterClothesExtraFields(actor.Chara);
        }

        //An exception will be thrown when opening door and the character stop moving. The following is added to prevent this problem
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.ResetDynamicBoneALL))]
        private static Exception CatchResetDynamicBoneALLErrors(Exception __exception)
        {
            if (__exception != null)
            {
                //Log.LogWarning("ResetDynamicBoneALL error thrown");
            }
            return null;
        }

        //Remember the state
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
        private static void SetActivePre(GameObject __instance, out bool __state, bool value)
        {
            __state = __instance.active;
        }

        //Set the clothes correct state for the action scene
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
        private static void SetActivePost(GameObject __instance, bool __state, bool value)
        {
            //TODO: revamp the logic for ActionScene
            Patches.SetActionSceneGameObjectActive(__instance, __state, value);
        }

        //Force to update the shoe state
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), nameof(Actor.ChangeState), new[] { typeof(RG.Define.StateID), typeof(bool) })]
        private static void ChangeState1Post(Actor __instance, RG.Define.StateID stateID, bool forceReset)
        {
            __instance.RefreshShoesState();
        }

        //Force to update the shoe state
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), nameof(Actor.ChangeState), new[] { typeof(int), typeof(bool) })]
        private static void ChangeState2Post(Actor __instance, int stateType, bool forceReset)
        {
            __instance.RefreshShoesState();
        }
       
        //Reflect the clothes states for the action scene
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.CheckStatus))]
        private static void CheckStatusPost(Actor actor)
        {
            if (ActionScene.Instance == null) return;
            Patches.UpdateActionSceneClothesStates(actor);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaFileCoordinate), nameof(Chara.ChaFileCoordinate.LoadFile), new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        private static void LoadFile2Post(Chara.ChaFileCoordinate __instance, string path, bool clothes, bool accessory, bool hair, bool skipPng)
        {
            Patches.ConvertFemaleClothesSetCard(__instance, path);
        }

    }

}
