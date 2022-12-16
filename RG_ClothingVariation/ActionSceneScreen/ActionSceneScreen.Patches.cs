using BepInEx.Logging;
using RG.Scene;
using RG.Scene.Action.Core;
using UnityEngine;

namespace RGClothingVariation.ActionSceneScreen
{
    internal class Patches
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        //Set the clothes state of the model in main game
        internal static void ReflectClothesState(Chara.ChaControl character, int coordinateType, int targetClothesType, byte state)
        {
            //havent set any clothes state, do nothing
            if (character.NowCoordinate.clothes.parts.Count == Constant.ClothesPartCount)
            {
                return;
            }

            Actor actor = null;
            if(ActionScene.Instance != null)
                foreach (var a in ActionScene.Instance._actors)
                {
                    if (a.Chara.GetInstanceID() == character.GetInstanceID())
                        actor = a;
                }

            var clothesStateInfo = character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber];
            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                int clothesType = (int)clothesStateInfo.colorInfo[i].layout.x;
                var cmpClothes = character.CmpClothes[i];

                if (clothesType == targetClothesType)
                {
                    character.FileStatus.clothesState[i] = System.Math.Max(state, (clothesStateInfo.hideOpt[i] || clothesStateInfo.hideOpt[i + Constant.ClothesPartCount]) ? (byte)1 : (byte)0);
                    if (cmpClothes != null)
                    {
                        cmpClothes.gameObject.SetActive(state != 2);

                        if (state != 2)
                        {
                            var isTopHalf = clothesStateInfo.hideOpt[i];
                            var isBottomHalf = clothesStateInfo.hideOpt[i + Constant.ClothesPartCount];

                            if (cmpClothes.objTopDef != null)
                                cmpClothes.objTopDef.SetActive(!isTopHalf);
                            if (cmpClothes.objTopHalf != null)
                                cmpClothes.objTopHalf.SetActive(isTopHalf);
                            if (cmpClothes.objBotDef != null)
                                cmpClothes.objBotDef.SetActive(!isBottomHalf);
                            if (cmpClothes.objBotHalf != null)
                                cmpClothes.objBotHalf.SetActive(isBottomHalf);
                        }
                    }
                    
                }

            }

            //special handling for clinics examination
            if (actor?.Status.PostedPointID.GetValueOrDefault() == 12
                        && actor?.Status.MapID == 2
                        && actor?.Status.Form == 1)
            {
                if (targetClothesType == 1)
                {
                    for (int i = 0; i < Constant.ClothesPartCount; i++)
                    {
                        int clothesType = (int)clothesStateInfo.colorInfo[i].layout.x;
                        if (clothesType == 0)
                        {
                            
                            var cmpClothes = character.CmpClothes[i];
                            if (cmpClothes?.objBotDef != null)
                            {
                                character.FileStatus.clothesState[i] = state;
                                cmpClothes.gameObject.SetActive(state != 2);
                            }
                        }
                    }
                }

                if (targetClothesType == 3)
                {
                    for (int i = 0; i < Constant.ClothesPartCount; i++)
                    {
                        int clothesType = (int)clothesStateInfo.colorInfo[i].layout.x;
                        if (clothesType == 2)
                        {
                            var cmpClothes = character.CmpClothes[i];
                            if (cmpClothes?.objBotDef != null)
                            {
                                character.FileStatus.clothesState[i] = state;
                                cmpClothes.gameObject.SetActive(state != 2);
                            }
                        }
                    }
                }
            }

        }

        internal static void SetActionSceneGameObjectActive(GameObject targetObject, bool originalState, bool value)
        {
            if (ActionScene.Instance == null) return;

            if (StateManager.Instance.IsHScene) return;

            var character = targetObject.GetComponentInParent<Chara.ChaControl>();
            if (character != null)
            {
                if (!Util.IsCharacterFemaleBody(character)) return;

                //Find the corresponding actor
                Actor actor = null;
                if (ActionScene.Instance?._actors != null)
                    foreach (var a in ActionScene.Instance._actors)
                    {
                        if (a.Chara?.GetInstanceID() == character.GetInstanceID())
                        {
                            actor = a;
                            break;
                        }
                    }

                if (Util.IsGameObjectNameClothesState(targetObject.name))
                {
                    //Get the clothes info
                    var cmps = targetObject.GetComponentsInParent<Chara.CmpClothes>(true);
                    var cmp = cmps[cmps.Count - 1];
                    var slotNumber = Util.GetClothesSlotNumberByObjectName(cmp.name);

                    var isTopHalf = character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[slotNumber];
                    var isBottomHalf = character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[slotNumber + Constant.ClothesPartCount];
                    
                    //Check the case of toilet
                    bool toiletOverrideValue = false;
                    if (Manager.Game.ActionMap?.APTContainer?.Toilet?.AttachedActor?.Chara.GetInstanceID() == character.GetInstanceID())
                    {
                        var clothesType = (int)character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x;
                        if (clothesType == 0 || clothesType == 2)
                        {
                            toiletOverrideValue = true;
                        }
                    }

                    if (targetObject.name == Constant.ClothesStateObjectName.TopFull)
                    {
                        targetObject.active = !isTopHalf;

                    }
                    else if (targetObject.name == Constant.ClothesStateObjectName.TopHalf)
                    {
                        targetObject.active = isTopHalf;
                    }
                    else if (targetObject.name == Constant.ClothesStateObjectName.BottomFull)
                    {
                        if (toiletOverrideValue)
                            targetObject.active = false;
                        else
                            targetObject.active = !isBottomHalf;
                    }
                    else if (targetObject.name == Constant.ClothesStateObjectName.BottomHalf)
                    {
                        if (toiletOverrideValue)
                            targetObject.active = true;
                        else
                            targetObject.active = isBottomHalf;
                    }
                    
                }
                else if (Util.IsGameObjectNameClothesPart(targetObject.name))
                {
                    //check if the character is staying in toilet

                    if (Manager.Game.ActionMap?.APTContainer?.Toilet?.AttachedActor?.Chara.GetInstanceID() == character.GetInstanceID())
                    {
                        //Recover the status first as we do not set the status based on the slot number
                        targetObject.active = originalState;

                        //we should set the clothes status based on the type instead of slot
                        int targetClothesType = Util.GetClothesSlotNumberByObjectName(targetObject.name);

                        var clothesStateInfo = character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber];
                        for (int i = 0; i < Constant.ClothesPartCount; i++)
                        {
                            int clothesType = (int)clothesStateInfo.colorInfo[i].layout.x;
                            if (clothesType == targetClothesType && character.CmpClothes[i] != null)
                            {
                                if (clothesType == 1 || clothesType == 3 || clothesType == 5)
                                    character.CmpClothes[i].gameObject.active = value;
                                else if (clothesType == 6)
                                    character.CmpClothes[i].gameObject.active = true;
                                else if (clothesType == 7)
                                {
                                    if (ActionScene.IsPrivateMap(ActionScene.Instance.MapID))
                                        character.CmpClothes[i].gameObject.active = false;
                                    else
                                        character.CmpClothes[i].gameObject.active = true;
                                }

                            }
                            if ((targetClothesType == 1 || targetClothesType == 3 || targetClothesType == 5) && (clothesType == 0 || clothesType == 2) && character.CmpClothes[i] != null)
                            {
                                //need to handle clothes that cover both top and bottom
                                if (character.CmpClothes[i].objBotDef != null)
                                    character.CmpClothes[i].objBotDef.active = value;
                                if (character.CmpClothes[i].objBotHalf != null)
                                    character.CmpClothes[i].objBotHalf.active = !value;
                            }

                        }
                    }
                    else
                    {
                        var slotNumber = Util.GetClothesSlotNumberByObjectName(targetObject.name);

                        if (ActionScene.IsPrivateMap(ActionScene.Instance.MapID))
                        {
                            int clothesType = (int)character.NowCoordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x;
                            if (clothesType == 7)
                            {
                                if (actor.StateID != RG.Define.StateID.Exit)
                                {
                                    //force no shoes in private room
                                    targetObject.active = false;
                                }
                            }
                            else
                            {
                                if (character.FileStatus.clothesState[slotNumber] == 1 && character.VisibleAll)
                                    targetObject.active = true;
                            }
                        }
                        else
                        {
                            if (character.FileStatus.clothesState[slotNumber] == 1 && character.VisibleAll)
                                targetObject.active = true;
                        }

                    }

                }
            }
        }

        internal static void UpdateActionSceneClothesStates(Actor actor)
        {
            if (Util.IsCharacterFemaleBody(actor.Chara))
            {
                if (actor.Status.MapID == 2 && actor.Status.PostedPointID.GetValueOrDefault() == 12
                    && actor.Status.Form == 1)
                {
                    //Case of examination by doctor in clinics
                    for (int i = 0; i < Constant.ClothesPartCount; i++)
                    {
                        byte state = Constant.GeneralClothesStates.Full;
                        if (i == Constant.ClothesPart.Bottom || i == Constant.ClothesPart.InnerBottom || i == Constant.ClothesPart.PantyHose)
                            state = Constant.GeneralClothesStates.Nude;
                        Patches.ReflectClothesState(actor.Chara, actor.Chara.FileStatus.coordinateType, i, state);
                    }
                }
                else if (actor.StateID == RG.Define.StateID.Idle)
                {
                    //If the top slot is empty, the system will not trigger the SetClothesState function for the top part and will cause the display of clothes state wrong
                    //here force to trigger it
                    for (int i = 0; i < Constant.ClothesPartCount; i++)
                    {
                        byte state = Constant.GeneralClothesStates.Full;
                        if (i == Constant.ClothesPart.Shoes && actor.IsInPrivateRoom()) state = Constant.GeneralClothesStates.Nude;
                        Patches.ReflectClothesState(actor.Chara, actor.Chara.FileStatus.coordinateType, i, state);
                    }
                }
            }
        }

        internal static void ConvertFemaleClothesSetCard(Chara.ChaFileCoordinate __instance, string path)
        {
            string[] pathSplit = path.Replace("\\", "/").Split('/');
            if (pathSplit[pathSplit.Length - 2] == "female")
            {
                Util.ConvertCoordinateData(__instance);
            }
        }

    }
}
