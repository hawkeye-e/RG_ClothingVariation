using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using UnityEngine;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;

namespace RGClothingVariation.HSceneScreen
{
    internal class Hook
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        //Set the flag of H Scene started
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.CharaInit))]
        private static void CharaInitPost(HScene __instance)
        {
            Patches.HSceneInit(__instance);
        }

        //Remove the flag when H Scene end
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.OnDestroy))]
        private static void OnDestroyPost()
        {
            Patches.HSceneEnd();
        }


        //Update the clothes id before that free h scene start
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.FreeHStartHpoint))]
        private static void FreeHStartHpointPost(HScene __instance)
        {
            Patches.InitCharacterClothesData(__instance);
        }

        //Reflect the clothes state in H scene in case the character load clothes set card
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.UpdateClothesStateAll))]
        internal static void UpdateClothesStateAllPost(Chara.ChaControl __instance)
        {
            if (!StateManager.Instance.IsHScene) return;

            Patches.ResetClothesStatesDictionary(__instance);
        }

        //Reflect the clothes state after the user click change all clothes state
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetClothesStateAll))]
        private static void SetClothesStateAllPost(Chara.ChaControl __instance, byte state)
        {
            Patches.UpdateClothesStateDictionaryAll(__instance, state);
        }

        //Record the clothes state when user click next status for a clothes slot
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetClothesStateNext))]
        private static void SetClothesStateNextPre(Chara.ChaControl __instance, ref byte __state, int clothesKind)
        {
            __state = __instance.FileStatus.clothesState[clothesKind];
        }

        //Reflect the clothes state after the user click change single clothes state
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara.ChaControl), nameof(Chara.ChaControl.SetClothesStateNext))]
        private static void SetClothesStateNextPost(Chara.ChaControl __instance, byte __state, int clothesKind)
        {
            Patches.UpdateClothesStateSingle(__instance, __state, clothesKind);
        }


        //Force the clothes state based on the value of DictHSceneClothesStates
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
        private static void SetActivePost(GameObject __instance, bool __state, bool value)
        {
            Patches.SetHSceneGameObjectActive(__instance);
        }

        //Handle the hide/show of the character clothes when "Location Change" icon button is clicked (not the heart one)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnClickMoveBt))]
        private static void OnClickMoveBtPost()
        {
            Patches.UpdateClothesStateForLocationChange();
        }

        //Set the flags after config change of the males
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ConfigMaleState))]
        private static void ConfigMaleStatePost(HScene __instance, RG.Config.HSystem config)
        {
            Patches.MaleClothesConfigChanged(__instance, config);
        }

        //Reset clothes states after location changed button ended
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.ReturnFromMovePoint))]
        private static void ReturnFromMovePointPost()
        {
            StateManager.Instance.HSceneForceAllClothesHidden = false;
            Patches.ResetFemaleClothesStates();
        }

        //Reset clothes states after location changed
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetClothStateStartMotion))]
        private static void SetClothStateStartMotionPost(HScene __instance, int _cha, HScene.AnimationListInfo info)
        {
            StateManager.Instance.HSceneVisibleCharacterInstanceIDList.Add(__instance._chaFemales[_cha].GetInstanceID());
            Patches.ResetFemaleClothesStates(_cha);
        }

        //Sex position(Taii) changed. Handles the case of FFM to MMF or MF and prevent showing the female clothes wrongly
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.OnChangePlaySelect), new[] { typeof(GameObject) })]
        private static void OnChangePlaySelectPost(GameObject objClick)
        {
            StateManager.Instance.HSceneVisibleCharacterInstanceIDList.Clear();
        }


        //Reset clothes states after location changed
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPositionPre(Transform _trans, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool _isWorld)
        {
            Patches.ResetFemaleClothesStates();
        }

        //Reset clothes states after location changed
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.SetPosition), new[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool) })]
        private static void SetPosition2Pre(HScene __instance, Vector3 pos, Quaternion rot, Vector3 offsetpos, Vector3 offsetrot, bool _FadeStart, bool isWorld)
        {
            Patches.ResetFemaleClothesStates();
        }

        //Reset clothes states after location change button action ended
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSprite), nameof(HSceneSprite.ReturnFromMovePointFinishChange))]
        private static void ReturnFromMovePointFinishChange()
        {
            StateManager.Instance.HSceneForceAllClothesHidden = false;
            Patches.ResetFemaleClothesStates();
        }

        //Initialize the clothes states for non Free H case
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.Init))]
        private static void Init(HSceneSpriteClothCondition __instance)
        {
            StateManager.Instance.HSceneClothButtonGroup = __instance;

            //comes from action scene, reset clothes states
            if (ActionScene.Instance != null)
                Patches.ResetFemaleClothesStates();
        }

        //Update the states of clothes state button when clothes set is loaded
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.SetClothCharacter))]
        private static void SetClothCharacterPost(HSceneSpriteClothCondition __instance, bool init)
        {
            Patches.ReflectClothesButtonState(__instance);
        }

        //Update the states of clothes state button when single clothes state change is clicked
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.OnClickCloth))]
        private static void OnClickClothPost(HSceneSpriteClothCondition __instance, int _cloth)
        {
            Patches.ReflectClothesButtonState(__instance);
        }

        //Update the states of clothes state button when all clothes state change is clicked
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSceneSpriteClothCondition), nameof(HSceneSpriteClothCondition.OnClickAllCloth))]
        private static void OnClickAllClothPost(HSceneSpriteClothCondition __instance)
        {
            Patches.ReflectClothesButtonState(__instance);
        }


    }
}
