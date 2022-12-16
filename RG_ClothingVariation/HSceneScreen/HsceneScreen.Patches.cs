using BepInEx.Logging;
using HarmonyLib;
using RG.Scene;
using UnityEngine;
using RG.Scene.Action.Core;
using System;
using System.Collections.Generic;

namespace RGClothingVariation.HSceneScreen
{
    internal class Patches
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        internal static void HSceneInit(HScene hScene)
        {
            StateManager.Instance.IsHScene = true;
            StateManager.Instance.HSceneInstance = hScene;
            StateManager.Instance.DictHSceneClothesStates = new Dictionary<int, Dictionary<int, int>>();
        }

        internal static void HSceneEnd()
        {
            StateManager.Instance.IsHScene = false;
            StateManager.Instance.DictHSceneClothesStates.Clear();
            StateManager.Instance.DictHSceneClothesStates = null;
            StateManager.Instance.HSceneInstance = null;
            StateManager.Instance.HSceneConfig = null;
            StateManager.Instance.HSceneClothButtonGroup = null;
            StateManager.Instance.HSceneForceAllClothesHidden = false;
            StateManager.Instance.HSceneVisibleCharacterInstanceIDList.Clear();
        }

        internal static void InitCharacterClothesData(HScene hScene)
        {
            if (hScene._chaFemales != null)
            {
                foreach (var character in hScene._chaFemales)
                {
                    if (character != null)
                        Util.InitializeCharacterClothesExtraFields(character);
                }
            }
            if (hScene._chaMales != null)
            {
                foreach (var character in hScene._chaMales)
                {
                    if (character != null)
                        if (Util.IsCharacterFemaleBody(character))
                            Util.InitializeCharacterClothesExtraFields(character);
                }
            }
        }

        internal static void ResetClothesStatesDictionary(Chara.ChaControl character)
        {
            if (character.NowCoordinate.clothes.parts.Count == Constant.ClothesPartCount)
                return;

            StateManager.Instance.DictHSceneClothesStates.Remove(character.GetInstanceID());
            InitializeHSceneClothesStateDictionary(character);

            //Need to init the clothes state here
            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                int clothesKind = (int)character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[i].layout.x;
                var extraInfo = character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber];

                int topState = (extraInfo.hideOpt[i]) ? Constant.GeneralClothesStates.Half : Constant.GeneralClothesStates.Full;
                int bottomState = (extraInfo.hideOpt[i + Constant.ClothesPartCount]) ? Constant.GeneralClothesStates.Half : Constant.GeneralClothesStates.Full;

                if (character.CmpClothes[i] != null)
                {
                    if (clothesKind == Constant.ClothesPart.Gloves || clothesKind == Constant.ClothesPart.Socks || clothesKind == Constant.ClothesPart.Shoes)
                    {
                        Set2StateClothesObjectState(character, i, Constant.GeneralClothesStates.Full);
                    }
                    else if (clothesKind == Constant.ClothesPart.Top || clothesKind == Constant.ClothesPart.InnerTop)
                    {
                        Set3StateClothesObjectState(character, i, true, topState);
                        if (character.CmpClothes[i]?.objBotDef != null)
                        {
                            Set3StateClothesObjectState(character, i, false, bottomState);
                        }
                    }
                    else if (clothesKind == Constant.ClothesPart.Bottom || clothesKind == Constant.ClothesPart.InnerBottom)
                    {
                        Set3StateClothesObjectState(character, i, false, bottomState);
                    }
                    else if (clothesKind == Constant.ClothesPart.PantyHose)
                    {
                        if (character.CmpClothes[i].objBotDef != null)
                            Set3StateClothesObjectState(character, i, false, bottomState);
                        else
                            Set2StateClothesObjectState(character, i, Constant.GeneralClothesStates.Full);
                    }
                }

                //set it to nude to avoid transparent body
                character.FileStatus.clothesState[i] = 2;
            }
        }

        //Set up the dictionary based on the clothes state 
        internal static void InitializeHSceneClothesStateDictionary(Chara.ChaControl character)
        {
            if (StateManager.Instance.DictHSceneClothesStates.ContainsKey(character.GetInstanceID()))
                return;

            if (character.NowCoordinate.clothes.parts.Count == Constant.ExtraFieldPartNumber)
                return;

            var extraInfo = character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber];
            Dictionary<int, int> dictClothesState = new Dictionary<int, int>();
            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                int topState = Constant.GeneralClothesStates.Full;
                int bottomState = Constant.GeneralClothesStates.Full;

                int clothesType = (int)extraInfo.colorInfo[i].layout.x;
                if (character.CmpClothes[i] != null)
                {
                    if (clothesType == Constant.ClothesPart.Gloves || clothesType == Constant.ClothesPart.Socks || clothesType == Constant.ClothesPart.Shoes)
                    {
                        topState = character.CmpClothes[i].gameObject.active ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Nude;
                    }
                    else if (clothesType == Constant.ClothesPart.Top || clothesType == Constant.ClothesPart.InnerTop)
                    {
                        topState = character.CmpClothes[i].objTopDef.active ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Half;
                        if (character.CmpClothes[i].objBotDef != null)
                            bottomState = character.CmpClothes[i].objBotDef.active ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Half;
                    }
                    else if (clothesType == Constant.ClothesPart.Bottom || clothesType == Constant.ClothesPart.InnerBottom)
                    {
                        bottomState = character.CmpClothes[i].objBotDef.active ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Half;
                    }
                    else if (clothesType == Constant.ClothesPart.PantyHose)
                    {
                        if (character.CmpClothes[i].objBotDef != null)
                            topState = character.CmpClothes[i].objBotDef.active ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Half;
                        else
                            topState = character.CmpClothes[i].gameObject.active ? Constant.GeneralClothesStates.Full : Constant.GeneralClothesStates.Half;
                    }
                }

                dictClothesState.Add(i, topState);
                dictClothesState.Add(i + Constant.ClothesPartCount, bottomState);
            }
            StateManager.Instance.DictHSceneClothesStates.Add(character.GetInstanceID(), dictClothesState);
        }

        internal static void UpdateClothesStateDictionaryAll(Chara.ChaControl character, byte state)
        {
            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                int clothesKind = (int)character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[i].layout.x;
                character.FileStatus.clothesState[i] = Constant.GeneralClothesStates.Nude;


                if (character.CmpClothes[i] != null)
                {
                    if (clothesKind == Constant.ClothesPart.Gloves || clothesKind == Constant.ClothesPart.Socks || clothesKind == Constant.ClothesPart.Shoes)
                        Set2StateClothesObjectState(character, i, state);
                    else if (clothesKind == Constant.ClothesPart.Top || clothesKind == Constant.ClothesPart.InnerTop)
                    {
                        Set3StateClothesObjectState(character, i, true, state);
                        if (character.CmpClothes[i]?.objBotDef != null)
                        {
                            Set3StateClothesObjectState(character, i, false, state);
                        }
                    }
                    else if (clothesKind == Constant.ClothesPart.Bottom || clothesKind == Constant.ClothesPart.InnerBottom)
                        Set3StateClothesObjectState(character, i, false, state);
                    else if (clothesKind == Constant.ClothesPart.PantyHose)
                    {
                        if (character.CmpClothes[i].objBotDef != null)
                            Set3StateClothesObjectState(character, i, false, state);
                        else
                            Set2StateClothesObjectState(character, i, state);
                    }
                }
            }
        }

        internal static void UpdateClothesStateSingle(Chara.ChaControl character, byte originalState, int targetClothesType)
        {
            //recover the state
            int targetState = StateManager.Instance.HSceneClothButtonGroup._clothObjSets[targetClothesType].Obj.state + 1;
            if (targetState >= StateManager.Instance.HSceneClothButtonGroup._clothObjSets[targetClothesType].Obj.buttons.Count)
                targetState = Constant.GeneralClothesStates.Full;

            character.FileStatus.clothesState[targetClothesType] = originalState;

            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                //Find out the correct clothes with target clothes type that requires state change
                int currentClothesType = (int)character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[i].layout.x;
                if (targetClothesType == currentClothesType)
                {
                    character.FileStatus.clothesState[i] = Constant.GeneralClothesStates.Nude;


                    if (character.CmpClothes[i] != null)
                    {
                        if (targetClothesType == Constant.ClothesPart.Gloves || targetClothesType == Constant.ClothesPart.Socks || targetClothesType == Constant.ClothesPart.Shoes)
                            Set2StateClothesObjectState(character, i, targetState);
                        else if (targetClothesType == Constant.ClothesPart.Top || targetClothesType == Constant.ClothesPart.InnerTop)
                            Set3StateClothesObjectState(character, i, true, targetState);
                        else if (targetClothesType == Constant.ClothesPart.Bottom || targetClothesType == Constant.ClothesPart.InnerBottom)
                            Set3StateClothesObjectState(character, i, false, targetState);
                        else if (targetClothesType == Constant.ClothesPart.PantyHose)
                        {
                            if (character.CmpClothes[i].objBotDef != null)
                                Set3StateClothesObjectState(character, i, false, targetState);
                            else
                                Set2StateClothesObjectState(character, i, targetState);
                        }
                    }
                }
                else
                {
                    if ((currentClothesType == Constant.ClothesPart.Top && targetClothesType == Constant.ClothesPart.Bottom) 
                        || (currentClothesType == Constant.ClothesPart.InnerTop && targetClothesType == Constant.ClothesPart.InnerBottom))
                    {
                        //If setting the bottom half state, need to consider the case of clothes contain both top and half
                        if (character.CmpClothes[i]?.objBotDef != null)
                        {
                            Set3StateClothesObjectState(character, i, false, targetState);
                        }
                    }
                }

            }
        }

        internal static void SetHSceneGameObjectActive(GameObject targetObject)
        {
            if (!StateManager.Instance.IsHScene) return;

            var character = targetObject.GetComponentInParent<Chara.ChaControl>();
            if (character != null)
            {
                if (!Util.IsCharacterFemaleBody(character)) return;


                if (!(StateManager.Instance.HSceneVisibleCharacterInstanceIDList.Contains(character.GetInstanceID()) || character.IsVisibleInCamera))
                    return;

                if (StateManager.Instance.HSceneForceAllClothesHidden)
                    return;

                if (StateManager.Instance.DictHSceneClothesStates.ContainsKey(character.GetInstanceID()))
                {
                    var clothesState = StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()];

                    bool futanariSettingDisableAllOverride = false;
                    bool futanariSettingDisableShoeOverride = false;
                    if (StateManager.Instance.HSceneInstance?._chaMales[0]?.GetInstanceID() == character.GetInstanceID())
                    {
                        if (StateManager.Instance.HSceneConfig != null)
                        {
                            futanariSettingDisableAllOverride = !(StateManager.Instance.HSceneConfig.Cloth
                                && !StateManager.Instance.HSceneConfig.SimpleBody
                                && StateManager.Instance.HSceneConfig.Visible);
                            futanariSettingDisableShoeOverride = !(StateManager.Instance.HSceneConfig.Shoes
                                && !StateManager.Instance.HSceneConfig.SimpleBody
                                && StateManager.Instance.HSceneConfig.Visible);
                        }
                    }
                    else if (StateManager.Instance.HSceneInstance?._chaMales[1]?.GetInstanceID() == character.GetInstanceID())
                    {
                        if (StateManager.Instance.HSceneConfig != null)
                        {
                            futanariSettingDisableAllOverride = !(StateManager.Instance.HSceneConfig.SecondCloth
                                && !StateManager.Instance.HSceneConfig.SimpleBody
                                && StateManager.Instance.HSceneConfig.SecondVisible);
                            futanariSettingDisableShoeOverride = !(StateManager.Instance.HSceneConfig.SecondShoes
                                && !StateManager.Instance.HSceneConfig.SimpleBody
                                && StateManager.Instance.HSceneConfig.SecondVisible);
                        }
                    }

                    if (Util.IsGameObjectNameClothesState(targetObject.name))
                    {
                        //Get the clothes info
                        var cmps = targetObject.GetComponentsInParent<Chara.CmpClothes>(true);
                        var cmp = cmps[cmps.Count - 1];
                        var slotNumber = Util.GetClothesSlotNumberByObjectName(cmp.name);

                        var isTopFull = clothesState[slotNumber] == Constant.GeneralClothesStates.Full;
                        var isTopHalf = clothesState[slotNumber] == Constant.GeneralClothesStates.Half;
                        var isBottomFull = clothesState[slotNumber + Constant.ClothesPartCount] == Constant.GeneralClothesStates.Full;
                        var isBottomHalf = clothesState[slotNumber + Constant.ClothesPartCount] == Constant.GeneralClothesStates.Half;

                        if (targetObject.name == Constant.ClothesStateObjectName.TopFull)
                        {
                            targetObject.active = isTopFull;
                        }
                        else if (targetObject.name == Constant.ClothesStateObjectName.TopHalf)
                        {
                            targetObject.active = isTopHalf;
                        }
                        else if (targetObject.name == Constant.ClothesStateObjectName.BottomFull)
                        {
                            targetObject.active = isBottomFull;
                        }
                        else if (targetObject.name == Constant.ClothesStateObjectName.BottomHalf)
                        {
                            targetObject.active = isBottomHalf;
                        }

                    }
                    else if (Util.IsGameObjectNameClothesPart(targetObject.name))
                    {
                        var slotNumber = Util.GetClothesSlotNumberByObjectName(targetObject.name);
                        var isActive = !IsTopAndHalfNotActive(character, slotNumber);
                        int clothesType = (int)character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x;

                        if (clothesType == Constant.ClothesPart.Shoes)
                        {
                            if (futanariSettingDisableShoeOverride)
                                targetObject.active = false;
                            else
                                targetObject.active = isActive;
                        }
                        else
                        {
                            if (futanariSettingDisableAllOverride)
                                targetObject.active = false;
                            else
                                targetObject.active = isActive;
                        }

                    }
                }

            }
        }

        internal static void UpdateClothesStateForLocationChange()
        {
            //Check if the user is entering or exiting the Location Change mode
            bool isAnyFemaleVisible = false;
            foreach (var character in StateManager.Instance.HSceneInstance._chaFemales)
            {
                if (character != null)
                    isAnyFemaleVisible = isAnyFemaleVisible || character.IsVisibleInCamera;
            }

            if (isAnyFemaleVisible)
            {
                //Entering Location Change mode, prepare to hide
                StateManager.Instance.HSceneForceAllClothesHidden = true;

                StateManager.Instance.HSceneVisibleCharacterInstanceIDList.Clear();
                foreach (var character in StateManager.Instance.HSceneInstance._chaFemales)
                {
                    if (character != null)
                        if (character.IsVisibleInCamera)
                            StateManager.Instance.HSceneVisibleCharacterInstanceIDList.Add(character.GetInstanceID());
                }
                foreach (var character in StateManager.Instance.HSceneInstance._chaMales)
                {
                    if (character != null)
                        if (character.IsVisibleInCamera)
                            StateManager.Instance.HSceneVisibleCharacterInstanceIDList.Add(character.GetInstanceID());
                }
            }
            else
            {
                //Exiting Location Change mode, reset the states
                StateManager.Instance.HSceneForceAllClothesHidden = false;
                ResetFemaleClothesStates();
            }
        }

        internal static void MaleClothesConfigChanged(HScene hScene, RG.Config.HSystem config)
        {
            bool isRequireMale1ClothesRefresh = false;
            bool isRequireMale2ClothesRefresh = false;
            bool isRequireMale1ShoesRefresh = false;
            bool isRequireMale2ShoesRefresh = false;
            if (StateManager.Instance.HsceneMale1Clothes != config.Cloth)
                isRequireMale1ClothesRefresh = true;
            if (StateManager.Instance.HsceneMale2Clothes != config.SecondCloth)
                isRequireMale2ClothesRefresh = true;
            if (StateManager.Instance.HsceneMale1Shoes != config.Shoes)
                isRequireMale1ShoesRefresh = true;
            if (StateManager.Instance.HsceneMale2Shoes != config.SecondShoes)
                isRequireMale2ShoesRefresh = true;

            StateManager.Instance.HSceneConfig = config;
            StateManager.Instance.HsceneMale1Clothes = config.Cloth;
            StateManager.Instance.HsceneMale2Clothes = config.SecondCloth;
            StateManager.Instance.HsceneMale1Shoes = config.Shoes;
            StateManager.Instance.HsceneMale2Shoes = config.SecondShoes;

            if (hScene._chaMales[0] != null)
                if (hScene._chaMales[0].FileParam.futanari)
                {
                    if (isRequireMale1ClothesRefresh)
                    {
                        UpdateMaleClothesPartState(hScene._chaMales[0], false);
                    }
                    if (isRequireMale1ShoesRefresh)
                    {
                        UpdateMaleClothesPartState(hScene._chaMales[0], true);
                    }
                }

            if (hScene._chaMales[1] != null)
                if (hScene._chaMales[1].FileParam.futanari)
                {
                    if (isRequireMale2ClothesRefresh)
                    {
                        UpdateMaleClothesPartState(hScene._chaMales[1], false);
                    }

                    if (isRequireMale2ShoesRefresh)
                    {
                        UpdateMaleClothesPartState(hScene._chaMales[1], true);
                    }
                }
        }

        private static void UpdateMaleClothesPartState(Chara.ChaControl character, bool isShoesOnly)
        {
            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                int clothesType = (int)character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[i].layout.x;
                if ((clothesType != Constant.ClothesPart.Shoes && !isShoesOnly) || (clothesType == Constant.ClothesPart.Shoes && isShoesOnly) )
                    if (character.CmpClothes[i] != null)
                        character.CmpClothes[i].gameObject.SetActive(!character.CmpClothes[i].gameObject);
            }
        }

        private static void Set2StateClothesObjectState(Chara.ChaControl character, int slotNumber, int state)
        {
            if (character.CmpClothes[slotNumber] != null)
                if (state == Constant.TwoStateClothesStates.Nude)
                {
                    //Hide
                    character.CmpClothes[slotNumber].gameObject.active = false;

                    UpdateClothesState(character, slotNumber, Constant.GeneralClothesStates.Nude, true);
                    UpdateClothesState(character, slotNumber, Constant.GeneralClothesStates.Nude, false);
                }
                else if (state == Constant.TwoStateClothesStates.Full)
                {
                    //Show
                    character.CmpClothes[slotNumber].gameObject.active = true;
                    UpdateClothesState(character, slotNumber, state, true);
                    UpdateClothesState(character, slotNumber, state, false);
                }
        }

        private static void Set3StateClothesObjectState(Chara.ChaControl character, int slotNumber, bool isTop, int state)
        {
            if (character.CmpClothes[slotNumber] != null)
            {
                if (state == Constant.GeneralClothesStates.Nude)
                {
                    //Hide all
                    if (isTop)
                    {
                        if (character.CmpClothes[slotNumber].objTopDef != null)
                            character.CmpClothes[slotNumber].objTopDef.active = false;
                        if (character.CmpClothes[slotNumber].objTopHalf != null)
                            character.CmpClothes[slotNumber].objTopHalf.active = false;
                        UpdateClothesState(character, slotNumber, state, true);
                    }
                    else
                    {
                        if (character.CmpClothes[slotNumber].objBotDef != null)
                            character.CmpClothes[slotNumber].objBotDef.active = false;
                        if (character.CmpClothes[slotNumber].objBotHalf != null)
                            character.CmpClothes[slotNumber].objBotHalf.active = false;
                        UpdateClothesState(character, slotNumber, state, false);
                    }

                    if (IsTopAndHalfNotActive(character, slotNumber))
                        character.CmpClothes[slotNumber].gameObject.active = false;

                }
                else if (state == Constant.GeneralClothesStates.Full)
                {
                    //Show
                    character.CmpClothes[slotNumber].gameObject.active = true;
                    if (isTop)
                    {
                        if (character.CmpClothes[slotNumber].objTopDef != null)
                            character.CmpClothes[slotNumber].objTopDef.active = true;
                        if (character.CmpClothes[slotNumber].objTopHalf != null)
                            character.CmpClothes[slotNumber].objTopHalf.active = false;
                        UpdateClothesState(character, slotNumber, state, true);
                    }
                    else
                    {
                        if (character.CmpClothes[slotNumber].objBotDef != null)
                            character.CmpClothes[slotNumber].objBotDef.active = true;
                        if (character.CmpClothes[slotNumber].objBotHalf != null)
                            character.CmpClothes[slotNumber].objBotHalf.active = false;
                        UpdateClothesState(character, slotNumber, state, false);
                    }

                }
                else if (state == Constant.GeneralClothesStates.Half)
                {
                    //Half dressing
                    character.CmpClothes[slotNumber].gameObject.active = true;

                    if (isTop)
                    {
                        if (character.CmpClothes[slotNumber].objTopDef != null)
                            character.CmpClothes[slotNumber].objTopDef.active = false;
                        if (character.CmpClothes[slotNumber].objTopHalf != null)
                            character.CmpClothes[slotNumber].objTopHalf.active = true;
                        UpdateClothesState(character, slotNumber, state, true);
                    }
                    else
                    {
                        if (character.CmpClothes[slotNumber].objBotDef != null)
                            character.CmpClothes[slotNumber].objBotDef.active = false;
                        if (character.CmpClothes[slotNumber].objBotHalf != null)
                            character.CmpClothes[slotNumber].objBotHalf.active = true;
                        UpdateClothesState(character, slotNumber, state, false);
                    }

                }
            }
        }

        //Update the dictionary which will be used in SetActive
        internal static void UpdateClothesState(Chara.ChaControl character, int slotNumber, int state, bool isTop)
        {
            if (StateManager.Instance.DictHSceneClothesStates == null) return;

            Dictionary<int, int> clothesStateDict;
            if (!StateManager.Instance.DictHSceneClothesStates.ContainsKey(character.GetInstanceID()))
            {
                clothesStateDict = new Dictionary<int, int>();
                StateManager.Instance.DictHSceneClothesStates.Add(character.GetInstanceID(), clothesStateDict);
            }
            else
                clothesStateDict = StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()];

            if (!isTop)
                slotNumber += Constant.ClothesPartCount;

            if (clothesStateDict.ContainsKey(slotNumber))
                clothesStateDict[slotNumber] = state;
            else
                clothesStateDict.Add(slotNumber, state);
        }

        internal static bool IsTopAndHalfNotActive(Chara.ChaControl character, int slotNumber)
        {
            if (StateManager.Instance.DictHSceneClothesStates.ContainsKey(character.GetInstanceID()))
            {
                if (StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()].ContainsKey(slotNumber)
                    && StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()].ContainsKey(slotNumber + Constant.ClothesPartCount))
                {
                    return StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()][slotNumber] == Constant.GeneralClothesStates.Nude &&
                        StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()][slotNumber + Constant.ClothesPartCount] == Constant.GeneralClothesStates.Nude;
                }
            }

            return true;
        }

        internal static void ResetFemaleClothesStates()
        {
            if (StateManager.Instance.HSceneInstance != null)
            {
                for (int i = 0; i < StateManager.Instance.HSceneInstance._chaFemales.Count; i++)
                    ResetFemaleClothesStates(i);
            }
        }

        internal static void ResetFemaleClothesStates(int charIndex)
        {
            if (StateManager.Instance.HSceneInstance != null)
            {
                var character = StateManager.Instance.HSceneInstance._chaFemales[charIndex];

                if (character == null) return;

                for (int i = 0; i < Constant.ClothesPartCount; i++)
                {
                    //set the negative value to trigger setactive
                    if (character.CmpClothes[i] != null)
                    {
                        character.CmpClothes[i].gameObject.SetActive(false);
                        if (character.CmpClothes[i].objBotDef != null)
                            character.CmpClothes[i].objBotDef.gameObject.SetActive(false);
                        if (character.CmpClothes[i].objBotHalf != null)
                            character.CmpClothes[i].objBotHalf.gameObject.SetActive(false);
                        if (character.CmpClothes[i].objTopDef != null)
                            character.CmpClothes[i].objTopDef.gameObject.SetActive(false);
                        if (character.CmpClothes[i].objTopHalf != null)
                            character.CmpClothes[i].objTopHalf.gameObject.SetActive(false);
                    }

                }


            }
        }

        internal static void ReflectClothesButtonState(HSceneSpriteClothCondition instance)
        {
            //determine the button by checking the overall clothes state
            //Key: clothes type, value: state
            Dictionary<int, int> dictClothesStateGrouped = new Dictionary<int, int>();

            int targetChar = -1;
            foreach (var toggle in instance._hSceneSpriteChaChoice.tglCharas)
            {
                if (toggle.isOn)
                {
                    if (toggle.name.StartsWith("f"))
                    {
                        int.TryParse(toggle.name.Substring(1, 1), out targetChar);
                    }
                    break;
                }
            }

            if (targetChar == -1) return;

            var character = instance._females[targetChar - 1];
            var extraInfo = character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber];

            if (!StateManager.Instance.DictHSceneClothesStates.ContainsKey(character.GetInstanceID()))
                InitializeHSceneClothesStateDictionary(character);
            if (!StateManager.Instance.DictHSceneClothesStates.ContainsKey(character.GetInstanceID()))
                return;

            var dictClothesState = StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()];

            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                if (character.CmpClothes[i] != null)
                {
                    int clothesType = (int)extraInfo.colorInfo[i].layout.x;
                    int state = 0;
                    if (clothesType == Constant.ClothesPart.Gloves || clothesType == Constant.ClothesPart.Socks || clothesType == Constant.ClothesPart.Shoes)
                    {
                        state = Math.Min(1, dictClothesState[i]);
                    }
                    else if (clothesType == Constant.ClothesPart.Top || clothesType == Constant.ClothesPart.InnerTop)
                    {
                        state = dictClothesState[i];
                    }
                    else if (clothesType == Constant.ClothesPart.Bottom || clothesType == Constant.ClothesPart.InnerBottom || clothesType == Constant.ClothesPart.PantyHose)
                    {
                        state = dictClothesState[i + Constant.ClothesPartCount];
                    }

                    if (dictClothesStateGrouped.ContainsKey(clothesType))
                        dictClothesStateGrouped[clothesType] = Math.Max(state, dictClothesStateGrouped[clothesType]);
                    else
                        dictClothesStateGrouped.Add(clothesType, state);
                    
                    //Extra condition for clothes that have both top and bottom
                    if (clothesType == Constant.ClothesPart.Top || clothesType == Constant.ClothesPart.InnerTop)
                    {
                        if (character.CmpClothes[i]?.objBotDef != null)
                        {
                            state = dictClothesState[i + Constant.ClothesPartCount];
                            int targetClothesType = clothesType + 1;

                            if (dictClothesStateGrouped.ContainsKey(targetClothesType))
                                dictClothesStateGrouped[targetClothesType] = Math.Max(state, dictClothesStateGrouped[targetClothesType]);
                            else
                                dictClothesStateGrouped.Add(targetClothesType, state);
                        }
                    }
                }
            }

            //reset all buttons
            foreach (var objSet in instance._clothObjSets)
            {
                foreach (var btn in objSet.Obj.buttons)
                {
                    btn.gameObject.active = false;
                }

            }

            //Reflect the button state
            int maxState = 0;
            foreach (var kvp in dictClothesStateGrouped)
            {
                instance._clothObjSets[kvp.Key].Obj.gameObject.active = true;
                instance._clothObjSets[kvp.Key].Obj.SetButton(kvp.Value);
                maxState = Math.Max(maxState, kvp.Value);
            }
            instance._clothAllObjSet.Obj.SetButton(maxState);
        }
    }
}
