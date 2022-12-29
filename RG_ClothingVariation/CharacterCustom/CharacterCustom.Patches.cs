using BepInEx.Logging;
using CharaCustom;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Illusion.Collections.Generic.Optimized;

namespace RGClothingVariation.CharacterCustom
{
    internal class Patches
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;
        internal static void UpdateClothesStateToggleButton(Chara.ChaControl character, int coordinateType, int sno)
        {
            if (character.ChaFile.Coordinate[coordinateType].clothes.parts.Count == Constant.ClothesPartCount)
            {
                //No clothes state is set, default full dressing
                StateManager.Instance.TopFullDressingToggle.isOn = true;
                StateManager.Instance.BottomFullDressingToggle.isOn = true;
            }
            else
            {
                if (character.ChaFile.Coordinate[coordinateType].clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[sno])
                    StateManager.Instance.TopHalfDressingToggle.SetIsOnWithoutNotify(true);
                else
                    StateManager.Instance.TopFullDressingToggle.SetIsOnWithoutNotify(true);

                if (character.ChaFile.Coordinate[coordinateType].clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[sno + Constant.ClothesPartCount])
                    StateManager.Instance.BottomHalfDressingToggle.SetIsOnWithoutNotify(true);
                else
                    StateManager.Instance.BottomFullDressingToggle.SetIsOnWithoutNotify(true);
            }
        }

        //Set the clothes state of the model in edit screen
        internal static void ReflectClothesState(Chara.ChaControl character, int coordinateType)
        {
            //havent set any clothes state, perform data conversion
            if (character.ChaFile.Coordinate[coordinateType].clothes.parts.Count == Constant.ClothesPartCount)
            {
                Util.ConvertCoordinateData(character.ChaFile.Coordinate[coordinateType]);

                for (int i = 0; i < Constant.ClothesPartCount; i++)
                {
                    character.ChangeClothes(i, character.ChaFile.Coordinate[coordinateType].clothes.parts[i].id);
                }
            }

            var clothesStateInfo = character.ChaFile.Coordinate[coordinateType].clothes.parts[Constant.ExtraFieldPartNumber];

            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                var isTopHalf = clothesStateInfo.hideOpt[i];
                var isBottomHalf = clothesStateInfo.hideOpt[i + Constant.ClothesPartCount];

                var id = character.ChaFile.Coordinate[coordinateType].clothes.parts[i].id;
                var origInfo = Util.FindClothesMappingKeyByValue(id);

                if ((i == 0 && origInfo.Item1 == 0))      //will have problem in the bust area of the model for the top slot case(no idea why)
                {
                    //will need to alter to correct status in SetActivePost
                    int overallClothesStatus = StateManager.GetOverallClothesStatus();
                    int overrideState = Util.GetOverrideClothesStateByClothesID(overallClothesStatus, origInfo.Item1);
                    character.FileStatus.clothesState[i] = (byte)Math.Max(overrideState, (isTopHalf || isBottomHalf) ? 1 : 0);
                }

                UpdateClothesGameObjectStatus(character, coordinateType, i, isTopHalf, isBottomHalf);
            }

        }

        public static void ClothesStateToggleOnClick(bool value)
        {
            if (StateManager.Instance.CharacterControl != null && value)
            {
                bool isTopHalfOn = StateManager.Instance.TopClothesStateCanvas.isActiveAndEnabled && StateManager.Instance.TopHalfDressingToggle.isOn;
                bool isBottomHalfOn = StateManager.Instance.BottomClothesStateCanvas.isActiveAndEnabled && StateManager.Instance.BottomHalfDressingToggle.isOn;
                Util.UpdateClothesState(StateManager.Instance.CharacterControl, StateManager.Instance.ClothesCanvas.coordinateType, StateManager.Instance.ClothesCanvas.SNo, isTopHalfOn, isBottomHalfOn);

                ReflectClothesState(StateManager.Instance.CharacterControl, StateManager.Instance.ClothesCanvas.coordinateType);

            }
        }

        internal static void UpdateClothesGameObjectStatus(Chara.ChaControl character, int coordinateType, int slotNumber, bool isTopHalf, bool isBottomHalf)
        {
            var partInfo = character.ChaFile.Coordinate[coordinateType].clothes.parts[slotNumber];
            var id = partInfo.id;
            var origInfo = Util.FindClothesMappingKeyByValue(id);
            var cmpClothes = character.CmpClothes[slotNumber];

            if (cmpClothes != null)
            {
                int overallClothesStatus = StateManager.GetOverallClothesStatus();
                var isGOActive = Util.GetOverrideClothesStateByClothesID(overallClothesStatus, origInfo.Item1) == 0;
                if (origInfo.Item1 == (int)Constant.ClothesPart.Gloves
                    || origInfo.Item1 == (int)Constant.ClothesPart.Socks
                    || origInfo.Item1 == (int)Constant.ClothesPart.Shoes
                    )
                {
                    cmpClothes.gameObject.SetActive(isGOActive);
                }
                else
                {
                    //handle the component outside the n_top_a etc
                    var cmps = cmpClothes.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (var mesh in cmps)
                    {
                        var parentCmp = mesh.transform.parent.GetComponent<Component>();
                        if (!Util.IsGameObjectNameClothesState(parentCmp.name))
                        {
                            parentCmp.gameObject.SetActive(isGOActive);
                        }
                    }

                    if (cmpClothes.objOpt01 != null)
                    {
                        for (int i = 0; i < cmpClothes.objOpt01.Length; i++)
                            cmpClothes.objOpt01[i].SetActive(!partInfo.hideOpt[0] && isGOActive);
                    }
                    if (cmpClothes.objOpt02 != null)
                    {
                        for (int i = 0; i < cmpClothes.objOpt02.Length; i++)
                            cmpClothes.objOpt02[i].SetActive(!partInfo.hideOpt[1] && isGOActive);

                    }

                    if (!(slotNumber == 0 && origInfo.Item1 == 0))       //will have problem in the bust area of the model for the top slot case(no idea why)
                    {
                        if (cmpClothes.objTopDef != null)
                            cmpClothes.objTopDef.SetActive(!isTopHalf && isGOActive);
                        if (cmpClothes.objTopHalf != null)
                            cmpClothes.objTopHalf.SetActive(isTopHalf && isGOActive);

                        cmpClothes.gameObject.SetActive(isGOActive);
                    }

                    if (cmpClothes.objBotDef != null)
                        cmpClothes.objBotDef.SetActive(!isBottomHalf && isGOActive);
                    if (cmpClothes.objBotHalf != null)
                        cmpClothes.objBotHalf.SetActive(isBottomHalf && isGOActive);

                }
            }
        }

        internal static Dictionary<int, (int, bool)> GetOptionalPartInstanceIDList(Chara.ChaControl character)
        {
            //Key: InstanceID, Value: (clothesType, hideoption)
            Dictionary<int, (int, bool)> list = new Dictionary<int, (int, bool)>();

            var coordinate = character.ChaFile.Coordinate[character.FileStatus.coordinateType];
            for (int i = 0; i < character.CmpClothes.Count; i++)
            {
                var cmp = character.CmpClothes[i];
                if (cmp != null)
                {
                    int clothesType = (int)coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[i].layout.x;

                    if (cmp.objOpt01 != null)
                        foreach (var opt in cmp.objOpt01)
                            list.Add(opt.GetInstanceID(), (clothesType, coordinate.clothes.parts[i].hideOpt[0]));

                    if (cmp.objOpt02 != null)
                        foreach (var opt in cmp.objOpt02)
                            list.Add(opt.GetInstanceID(), (clothesType, coordinate.clothes.parts[i].hideOpt[1]));
                }
            }

            return list;
        }


        internal static void ResetClothesState(Chara.ChaControl character)
        {
            for (int i = 0; i < Constant.ClothesPartCount; i++)
            {
                character.SetClothesState(i, 0);
            }
        }

        internal static void ShowClothesStateTab(CvsC_Clothes cvsClothes)
        {
            if (cvsClothes.chaCtrl.Sex != 0)
            {
                //Update the active state of the toggles depend on the current clothes allow state change or not
                var cmpClothes = StateManager.Instance.CharacterControl.CmpClothes[cvsClothes.SNo];
                StateManager.Instance.TopClothesStateCanvas.gameObject.active = false;
                StateManager.Instance.BottomClothesStateCanvas.gameObject.active = false;
                if (cmpClothes != null)
                {
                    if (cmpClothes.objTopDef != null && cmpClothes.objTopDef != null)
                    {
                        StateManager.Instance.TopClothesStateCanvas.gameObject.active = true;
                    }
                    if (cmpClothes.objBotDef != null && cmpClothes.objBotHalf != null)
                    {
                        StateManager.Instance.BottomClothesStateCanvas.gameObject.active = true;
                    }
                }

                //Set the tab to be visible if any of the toggle is active
                if (StateManager.Instance.TopClothesStateCanvas.gameObject.active || StateManager.Instance.BottomClothesStateCanvas.gameObject.active)
                    cvsClothes.ShowOrHideTab(true, 4);
            }
        }

        //Need to copy the clothes state part info
        internal static void CopyClothesStates(CvsC_Copy cvsCopy)
        {
            var sourcePartInfo = cvsCopy.chaCtrl.ChaFile.Coordinate[cvsCopy._ddSrc.Value].clothes.parts;
            var destPartInfo = cvsCopy.chaCtrl.ChaFile.Coordinate[cvsCopy._ddDst.Value].clothes.parts;

            if (sourcePartInfo.Length == Constant.ClothesPartCount)
                return;
            else
            {
                var pInfo = Util.ExpandClothesStateArray(destPartInfo);
                for (int i = 0; i < cvsCopy._tglSelect.Length - 1; i++) //exclude the last one(hair style)
                {
                    if (cvsCopy._tglSelect[i].isOn)
                    {
                        //State
                        pInfo[Constant.ExtraFieldPartNumber].hideOpt[i] = sourcePartInfo[Constant.ExtraFieldPartNumber].hideOpt[i];
                        pInfo[Constant.ExtraFieldPartNumber].hideOpt[i + Constant.ClothesPartCount] = sourcePartInfo[Constant.ExtraFieldPartNumber].hideOpt[i + Constant.ClothesPartCount];
                        //Clothes
                        pInfo[Constant.ExtraFieldPartNumber].colorInfo[i].layout = sourcePartInfo[Constant.ExtraFieldPartNumber].colorInfo[i].layout;
                    }
                }

            }
        }

        internal static void SetupStateManager(CustomControl customControl)
        {
            StateManager.Instance.ClothesCanvas = customControl.CvsC_Clothes;
            //Get the font the system is using
            StateManager.Instance.YuGothicFont = customControl.CvsC_Clothes.transform.Find("title").Find("textWinTitle").GetComponent<Text>().font;
            StateManager.Instance.CharacterControl = customControl.chaCtrl;

            StateManager.Instance.IsCharaCustomScreen = true;
        }

        internal static void SetupCustomUIForClothesState(CustomControl customControl)
        {
            //Female body only as there is no half-dressing state in male clothing
            if (customControl.modeSex == 0)
                return;

            //Locate the component
            var contentHolder = customControl.CvsC_Clothes.transform.Find("Setting").Find("Setting05").Find("Scroll View").Find("Viewport C_Clothes").Find("Content").GetComponent<Vertical​Layout​Group>();

            if (StateManager.Instance.ClothesStateAB == null)
            {
                StateManager.Instance.ClothesStateAB = AssetBundle.LoadFromMemory(Resource.clothesstate);
            }
            StateManager.Instance.TopClothesStateCanvas = Util.InstantiateFromBundle(StateManager.Instance.ClothesStateAB, "assets/prefab/cvsclothesstatetop.prefab").GetComponent<Canvas>();
            StateManager.Instance.TopClothesStateCanvas.gameObject.SetActive(true);
            StateManager.Instance.TopClothesStateCanvas.transform.SetParent(contentHolder.transform, false);

            StateManager.Instance.BottomClothesStateCanvas = Util.InstantiateFromBundle(StateManager.Instance.ClothesStateAB, "assets/prefab/cvsclothesstatebottom.prefab").GetComponent<Canvas>();
            StateManager.Instance.BottomClothesStateCanvas.gameObject.SetActive(true);
            StateManager.Instance.BottomClothesStateCanvas.transform.SetParent(contentHolder.transform, false);

            StateManager.Instance.TopFullDressingToggle = StateManager.Instance.TopClothesStateCanvas.transform.Find("togFull").GetComponent<Toggle>();
            StateManager.Instance.TopHalfDressingToggle = StateManager.Instance.TopClothesStateCanvas.transform.Find("togHalf").GetComponent<Toggle>();
            StateManager.Instance.BottomFullDressingToggle = StateManager.Instance.BottomClothesStateCanvas.transform.Find("togFull").GetComponent<Toggle>();
            StateManager.Instance.BottomHalfDressingToggle = StateManager.Instance.BottomClothesStateCanvas.transform.Find("togHalf").GetComponent<Toggle>();

            SetupClothesStateCanvas(StateManager.Instance.TopClothesStateCanvas, StateManager.Instance.TopFullDressingToggle, StateManager.Instance.TopHalfDressingToggle);
            SetupClothesStateCanvas(StateManager.Instance.BottomClothesStateCanvas, StateManager.Instance.BottomFullDressingToggle, StateManager.Instance.BottomHalfDressingToggle);
        }

        internal static void SetupClothesStateCanvas(Canvas canvas, Toggle fullDressingToggle, Toggle halfDressingToggle)
        {
            //Locate the text and set font
            var lblClothesState = canvas.transform.Find("lblClothesState").GetComponent<Text>();
            lblClothesState.font = StateManager.Instance.YuGothicFont;
            var lblFull = canvas.transform.Find("togFull").Find("lblFull").GetComponent<Text>();
            lblFull.font = StateManager.Instance.YuGothicFont;
            var lblHalf = canvas.transform.Find("togHalf").Find("lblHalf").GetComponent<Text>();
            lblHalf.font = StateManager.Instance.YuGothicFont;

            //Set the Toggle button action
            fullDressingToggle.isOn = false;
            halfDressingToggle.onValueChanged = new Toggle.ToggleEvent();
            halfDressingToggle.onValueChanged.AddListener((UnityAction<bool>)Patches.ClothesStateToggleOnClick);
            fullDressingToggle.onValueChanged = new Toggle.ToggleEvent();
            fullDressingToggle.onValueChanged.AddListener((UnityAction<bool>)Patches.ClothesStateToggleOnClick);
        }

        internal static void SetupCustomUIForClothesType(CustomControl customControl)
        {
            //Female body only
            if (customControl.modeSex == 0)
                return;

            //Locate the component
            var contentHolder = customControl.CvsC_Clothes.transform.Find("Setting").Find("Setting01").Find("SelectBox");

            StateManager.Instance.ClothesScrollController = customControl.CvsC_Clothes.transform.Find("Setting").Find("Setting01").Find("SelectBox").Find("Scroll View").GetComponent<CustomSelectScrollController>();
            StateManager.Instance.ClothesCanvasSelectName = customControl.CvsC_Clothes.transform.Find("Setting").Find("Setting01").Find("SelectBox").Find("SelectName").Find("SelectText").GetComponent<Text>();

            StateManager.Instance.SelectClothesCategoryCanvas = Util.InstantiateFromBundle(StateManager.Instance.ClothesStateAB, "assets/prefab/cvsclothescat.prefab").GetComponent<Canvas>();
            StateManager.Instance.SelectClothesCategoryCanvas.gameObject.SetActive(true);
            StateManager.Instance.SelectClothesCategoryCanvas.transform.SetParent(contentHolder, false);
            StateManager.Instance.SelectClothesCategoryCanvas.transform.SetAsFirstSibling();

            //Fix the position (any better way to rearrange the layout???)
            StateManager.Instance.SelectClothesCategoryCanvas.transform.position = new Vector3(contentHolder.transform.position.x, contentHolder.transform.position.y, contentHolder.transform.position.z);
            StateManager.Instance.SelectClothesCategoryCanvas.transform.localPosition = Vector3.zero;
            float categoryCanvasHeight = StateManager.Instance.SelectClothesCategoryCanvas.transform.GetChild(0).GetComponent<RectTransform>().rect.height;

            //Move other child
            for (int i = 1; i < contentHolder.transform.childCount; i++)
            {
                contentHolder.transform.GetChild(i).position -= new Vector3(0, categoryCanvasHeight, 0);
            }
            //Reduce the height of last child
            var lastChildRT = contentHolder.transform.GetChild(contentHolder.transform.childCount - 1).GetComponent<RectTransform>();
            lastChildRT.sizeDelta = new Vector2(lastChildRT.sizeDelta.x, lastChildRT.sizeDelta.y - categoryCanvasHeight);

            //Set the font of the label
            var tg = StateManager.Instance.SelectClothesCategoryCanvas.GetComponent<ToggleGroup>();
            foreach (var toggle in tg.m_Toggles)
            {
                var text = toggle.GetComponentInChildren<Text>();
                text.font = StateManager.Instance.YuGothicFont;
                toggle.onValueChanged = new Toggle.ToggleEvent();
                toggle.onValueChanged.AddListener((UnityAction<bool>)Patches.ClothesCategoryToggleOnClick);
            }

            //Set default value, default top
            UpdateClothesTypeToggle(StateManager.Instance.CharacterControl);
            UpdateClothesCanvasScrollViewText(StateManager.Instance.CharacterControl);
        }

        internal static void UpdateClothesCanvasScrollViewText(Chara.ChaControl characterControl, int slotNumber = 0)
        {
            var coordinate = characterControl.ChaFile.Coordinate[characterControl.FileStatus.coordinateType];
            int clothesType;
            string clothesName = "";
            clothesType = (int)coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x;
            var clothesID = coordinate.clothes.parts[slotNumber].id;

            foreach (var info in Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_top][clothesID])
            {
                if (info.ID == clothesID)
                {
                    clothesName = info.Name;
                    break;
                }
            }

            StateManager.Instance.ClothesCanvasSelectText = Util.GetClothesPartNameByClothesKind(clothesType) + " - " + clothesName;
            StateManager.Instance.ClothesCanvasSelectName.text = StateManager.Instance.ClothesCanvasSelectText;
        }

        internal static void UpdateClothesTypeToggle(Chara.ChaControl characterControl, int slotNumber = 0)
        {
            string selectedToggleName = Constant.ClothesCategoryToggleName.Top;
            var coordinate = characterControl.ChaFile.Coordinate[characterControl.FileStatus.coordinateType];
            if (coordinate.clothes.parts.Count > Constant.ClothesPartCount)
            {
                //Take the category of first clothes item
                if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 0)
                    selectedToggleName = Constant.ClothesCategoryToggleName.Top;
                else if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 1)
                    selectedToggleName = Constant.ClothesCategoryToggleName.Bottom;
                else if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 2)
                    selectedToggleName = Constant.ClothesCategoryToggleName.InnerTop;
                else if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 3)
                    selectedToggleName = Constant.ClothesCategoryToggleName.InnerBottom;
                else if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 4)
                    selectedToggleName = Constant.ClothesCategoryToggleName.Gloves;
                else if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 5)
                    selectedToggleName = Constant.ClothesCategoryToggleName.PantyHose;
                else if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 6)
                    selectedToggleName = Constant.ClothesCategoryToggleName.Socks;
                else if (coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x == 7)
                    selectedToggleName = Constant.ClothesCategoryToggleName.Shoes;
            }

            foreach (var t in StateManager.Instance.SelectClothesCategoryCanvas.GetComponent<ToggleGroup>().m_Toggles)
            {
                if (t.name == selectedToggleName)
                    t.SetIsOnWithoutNotify(true);
            }

        }

        public static void ClothesCategoryToggleOnClick(bool value)
        {
            if (StateManager.Instance.ClothesCanvas != null && value)
            {
                StateManager.Instance.IsClothesCategorySelectChanged = true;

                RefreshClothesList();
                StateManager.Instance.ClothesCanvasSelectName.text = StateManager.Instance.ClothesCanvasSelectText;

            }
        }

        internal static void RefreshClothesList()
        {
            Toggle toggle = null;
            foreach (var t in StateManager.Instance.SelectClothesCategoryCanvas.GetComponent<ToggleGroup>().m_Toggles)
            {
                if (t.isOn)
                {
                    toggle = t;
                    break;
                }
            }

            if (toggle != null)
            {
                Chara.ChaListDefine.CategoryNo cateNo = Chara.ChaListDefine.CategoryNo.fo_top;
                if (toggle.name == Constant.ClothesCategoryToggleName.Top)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_top;
                else if (toggle.name == Constant.ClothesCategoryToggleName.Bottom)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_bot;
                else if (toggle.name == Constant.ClothesCategoryToggleName.InnerTop)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_inner_t;
                else if (toggle.name == Constant.ClothesCategoryToggleName.InnerBottom)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_inner_b;
                else if (toggle.name == Constant.ClothesCategoryToggleName.Gloves)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_gloves;
                else if (toggle.name == Constant.ClothesCategoryToggleName.PantyHose)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_panst;
                else if (toggle.name == Constant.ClothesCategoryToggleName.Socks)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_socks;
                else if (toggle.name == Constant.ClothesCategoryToggleName.Shoes)
                    cateNo = Chara.ChaListDefine.CategoryNo.fo_shoes;

                var list = CvsC_Clothes.CreateSelectList(cateNo);
                StateManager.Instance.ClothesCanvas.sscClothesType.CreateList(list);

                var coordinate = StateManager.Instance.CharacterControl.ChaFile.Coordinate[StateManager.Instance.CharacterControl.FileStatus.coordinateType];
                foreach (var data in StateManager.Instance.ClothesCanvas.sscClothesType.scrollerDatas)
                {
                    if (data.info.Id == coordinate.clothes.parts[StateManager.Instance.ClothesCanvas.SNo].id)
                    {
                        StateManager.Instance.ClothesCanvas.sscClothesType.SelectInfo = data;
                    }
                }

                if (toggle.name == Constant.ClothesCategoryToggleName.Top
                    || toggle.name == Constant.ClothesCategoryToggleName.Bottom
                    || toggle.name == Constant.ClothesCategoryToggleName.Shoes
                    )
                {
                    StateManager.Instance.CategoryBasedClothesState = 0;
                    StateManager.Instance.ClothesCanvas.customBase.ChangeClothesStateAuto(0);
                }
                else
                {
                    StateManager.Instance.CategoryBasedClothesState = 1;
                    StateManager.Instance.ClothesCanvas.customBase.ChangeClothesStateAuto(1);
                }
            }
        }

        internal static void UpdateClothesSelectionList(int slotNumber, bool requireChangeState = false)
        {
            //Get the clothes category of the current selected slot
            var coordinate = StateManager.Instance.CharacterControl.ChaFile.Coordinate[StateManager.Instance.CharacterControl.FileStatus.coordinateType];
            int clothesType = (int)coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout.x;

            Chara.ChaListDefine.CategoryNo cateNo = Chara.ChaListDefine.CategoryNo.fo_top;
            if (clothesType == 0)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_top;
            else if (clothesType == 1)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_bot;
            else if (clothesType == 2)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_inner_t;
            else if (clothesType == 3)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_inner_b;
            else if (clothesType == 4)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_gloves;
            else if (clothesType == 5)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_panst;
            else if (clothesType == 6)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_socks;
            else if (clothesType == 7)
                cateNo = Chara.ChaListDefine.CategoryNo.fo_shoes;

            //Re-populate the selection list based on the selected category
            var list = CvsC_Clothes.CreateSelectList(cateNo);
            StateManager.Instance.ClothesCanvas.sscClothesType.CreateList(list);
            foreach (var data in StateManager.Instance.ClothesCanvas.sscClothesType.scrollerDatas)
            {
                if (data.info.Id == coordinate.clothes.parts[slotNumber].id)
                {
                    StateManager.Instance.ClothesCanvas.sscClothesType.SelectInfo = data;
                }
            }

            StateManager.Instance.IsClothesCategorySelectChanged = true;
            if (clothesType == 0
                || clothesType == 1
                || clothesType == 7
                )
            {
                StateManager.Instance.CategoryBasedClothesState = 0;
                if (requireChangeState)
                    StateManager.Instance.ClothesCanvas.customBase.ChangeClothesStateAuto(0);
            }
            else
            {
                StateManager.Instance.CategoryBasedClothesState = 1;
                if (requireChangeState)
                    StateManager.Instance.ClothesCanvas.customBase.ChangeClothesStateAuto(1);

            }
        }

        internal static void HandleThumbnailSaving(int stateNo)
        {
            if (stateNo == -1)
            {
                if (!StateManager.Instance.IsSavingThumbnail)
                {
                    StateManager.Instance.IsSavingThumbnail = true;
                    stateNo = 1;
                }
                else
                    StateManager.Instance.IsSavingThumbnail = false;
            }


            StateManager.Instance.UserSelectedClothesState = Math.Max(0, stateNo);
        }

        internal static void HandleCategorySwitching(CustomBase customBase, int stateNo)
        {
            if (StateManager.Instance.IsClothesCategorySelectChanged)
            {
                StateManager.Instance.CategoryBasedClothesState = stateNo;

                ReflectClothesState(customBase.chaCtrl, customBase.chaCtrl.FileStatus.coordinateType);
            }

            if (StateManager.Instance.IsMajorCategorySelectChanged)
            {
                UpdateClothesSelectionList(StateManager.Instance.ClothesCanvas.SNo);

                if (!StateManager.Instance.IsClothesCategorySelected)
                    StateManager.Instance.CategoryBasedClothesState = stateNo;

                ReflectClothesState(customBase.chaCtrl, customBase.chaCtrl.FileStatus.coordinateType);

                UpdateClothesCanvasScrollViewText(customBase.chaCtrl, StateManager.Instance.ClothesCanvas.SNo);

            }

            StateManager.Instance.IsClothesCategorySelectChanged = false;
            StateManager.Instance.IsMajorCategorySelectChanged = false;
        }

        internal static void ChangeClothes(Chara.ChaControl character, int slotNumber, int clothesID)
        {
            Chara.ChaFileCoordinate coordinate = character.ChaFile.Coordinate[character.FileStatus.coordinateType];
            //need to record the original id and value into extra field
            if (coordinate.clothes.parts.Count == Constant.ClothesPartCount)
            {
                coordinate.clothes.parts = Util.ExpandClothesStateArray(coordinate.clothes.parts);
            }
            //record the original clothes type and value to the extra field
            var origInfo = Util.FindClothesMappingKeyByValue(clothesID);
            coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[slotNumber].layout = new Vector2(origInfo.Item1, origInfo.Item2);

            coordinate.clothes.parts[slotNumber].id = clothesID;

            //Set the active part of the clothes if the clothes exists half dressing state
            var isTopHalf = coordinate.clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[slotNumber];
            var isBottomHalf = coordinate.clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[slotNumber + Constant.ClothesPartCount];

            UpdateClothesGameObjectStatus(character, character.FileStatus.coordinateType, slotNumber, isTopHalf, isBottomHalf);

            if (StateManager.Instance.IsCharaCustomScreen)
                UpdateClothesStateToggleButton(character, character.FileStatus.coordinateType, slotNumber);

        }

        internal static void SetCharaCustomGameObjectActive(GameObject targetObject)
        {
            if (!StateManager.Instance.IsCharaCustomScreen) return;

            var character = targetObject.GetComponentInParent<Chara.ChaControl>();
            if (character != null)
            {
                if (!Util.IsCharacterFemaleBody(character)) return;

                if (character.ChaFile.Coordinate[character.FileStatus.coordinateType].clothes.parts.Length == Constant.ClothesPartCount)
                    return;

                if ((Util.IsGameObjectNameClothesPart(targetObject.name) || Util.IsGameObjectNameClothesState(targetObject.name)))
                {

                    //Determine the correct clothes state
                    int overallClothesStatus = StateManager.GetOverallClothesStatus();
                    if (Util.IsGameObjectNameClothesPart(targetObject.name))
                    {

                        var cmps = targetObject.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        bool hasTopOrBottom = false;
                        foreach (var mesh in cmps)
                        {
                            var parentCmp = mesh.transform.parent.GetComponent<Component>();
                            if (parentCmp.name == Constant.ClothesStateObjectName.TopFull || parentCmp.name == Constant.ClothesStateObjectName.BottomFull)
                            {
                                //Set the branch back to active if it has half state
                                hasTopOrBottom = true;
                                targetObject.active = true;
                                break;
                            }
                        }

                        //otherwise it should depends on the overall status
                        if (!hasTopOrBottom)
                        {
                            var slotNumber = Util.GetClothesSlotNumberByObjectName(targetObject.name);
                            var clothesID = character.ChaFile.Coordinate[character.FileStatus.coordinateType].clothes.parts[slotNumber].id;
                            var origInfo = Util.FindClothesMappingKeyByValue(clothesID);
                            targetObject.active = Util.GetOverrideClothesStateByClothesID(overallClothesStatus, origInfo.Item1) == 0;
                        }

                    }
                    else if (Util.IsGameObjectNameClothesState(targetObject.name))
                    {
                        //Get the clothes info
                        var cmps = targetObject.GetComponentsInParent<Chara.CmpClothes>(true);
                        var cmp = cmps[cmps.Count - 1];
                        var slotNumber = Util.GetClothesSlotNumberByObjectName(cmp.name);
                        var clothesID = character.ChaFile.Coordinate[character.FileStatus.coordinateType].clothes.parts[slotNumber].id;

                        var origInfo = Util.FindClothesMappingKeyByValue(clothesID);
                        var clothesType = origInfo.Item1;
                        var isTopHalf = character.ChaFile.Coordinate[character.FileStatus.coordinateType].clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[slotNumber];
                        var isBottomHalf = character.ChaFile.Coordinate[character.FileStatus.coordinateType].clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[slotNumber + Constant.ClothesPartCount];
                        int overrideClothesState = Util.GetOverrideClothesStateByClothesID(overallClothesStatus, clothesType);
                        if (overrideClothesState == 2)
                        {
                            targetObject.active = false;
                        }
                        else
                        {
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
                                targetObject.active = !isBottomHalf;
                            }
                            else if (targetObject.name == Constant.ClothesStateObjectName.BottomHalf)
                            {
                                targetObject.active = isBottomHalf;
                            }
                        }
                    }
                }
                else
                {
                    //optional part of the clothes

                    var optionalPartList = GetOptionalPartInstanceIDList(character);
                    if (optionalPartList.ContainsKey(targetObject.GetInstanceID()))
                    {
                        //this is a optional part
                        var opInfo = optionalPartList[targetObject.GetInstanceID()];
                        int overallClothesStatus = StateManager.GetOverallClothesStatus();
                        int overrideClothesState = Util.GetOverrideClothesStateByClothesID(overallClothesStatus, opInfo.Item1);
                        if (overrideClothesState == 2)
                            targetObject.active = false;
                        else
                            targetObject.active = !opInfo.Item2;
                    }

                }
            }
        }

        internal static Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> GetMappedCategoryInfo(Chara.ChaListDefine.CategoryNo category)
        {
            if (category == Chara.ChaListDefine.CategoryNo.fo_top)
                return StateManager.Instance.ClothesTableCache.Top;
            else if (category == Chara.ChaListDefine.CategoryNo.fo_bot)
                return StateManager.Instance.ClothesTableCache.Bottom;
            else if (category == Chara.ChaListDefine.CategoryNo.fo_inner_t)
                return StateManager.Instance.ClothesTableCache.InnerTop;
            else if (category == Chara.ChaListDefine.CategoryNo.fo_inner_b)
                return StateManager.Instance.ClothesTableCache.InnerBottom;
            else if (category == Chara.ChaListDefine.CategoryNo.fo_gloves)
                return StateManager.Instance.ClothesTableCache.Gloves;
            else if (category == Chara.ChaListDefine.CategoryNo.fo_panst)
                return StateManager.Instance.ClothesTableCache.Pantyhose;
            else if (category == Chara.ChaListDefine.CategoryNo.fo_socks)
                return StateManager.Instance.ClothesTableCache.Socks;
            else if (category == Chara.ChaListDefine.CategoryNo.fo_shoes)
                return StateManager.Instance.ClothesTableCache.Shoes;

            return null;
        }

        internal static void SetClothesCategorySlotFlag(Selectable selectable)
        {
            if (selectable.name == "tglClothes"
                || selectable.name == "tglFace"
                || selectable.name == "tglBody"
                || selectable.name == "tglHair"
                || selectable.name == "tglAccessory"
                || selectable.name == "tglOption"
                )
            {
                StateManager.Instance.IsClothesCategorySelected = selectable.name == "tglClothes";
                StateManager.Instance.IsMajorCategorySelectChanged = true;
            }
            else if (selectable.name == "Top"
                || selectable.name == "Bot"
                || selectable.name == "InnerUp"
                || selectable.name == "InnerDown"
                || selectable.name == "Gloves"
                || selectable.name == "Panst"
                || selectable.name == "Socks"
                || selectable.name == "Shoes"
                )
            {
                StateManager.Instance.IsClothesSlotSelectChanged = true;
            }
        }

        internal static void SetClothesScrollerDisplayText(CustomSelectScrollController scroller)
        {
            if (scroller.GetInstanceID() == StateManager.Instance.ClothesScrollController.GetInstanceID())
                StateManager.Instance.ClothesCanvasSelectName.text = StateManager.Instance.ClothesCanvasSelectText;
        }

        internal static void SetClothesScrollerDisplayText(CustomSelectScrollController scroller, CustomSelectScrollController.ScrollData selectedData)
        {
            if (scroller.GetInstanceID() == StateManager.Instance.ClothesScrollController.GetInstanceID())
                StateManager.Instance.ClothesCanvasSelectText = Util.GetClothesPartNameByCategoryNo(selectedData.info.Category) + " - " + selectedData.info.Name;
            SetClothesScrollerDisplayText(scroller);
        }
    }
}
