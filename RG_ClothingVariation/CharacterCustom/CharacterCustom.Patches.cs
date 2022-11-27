using BepInEx.Logging;
using CharaCustom;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace RGClothingVariation.CharacterCustom
{
    internal class Patches
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;
        internal static void UpdateClothesStateToggleButton(Chara.ChaControl character, int coordinateType, int sno)
        {
            if (character.ChaFile.Coordinate[coordinateType].clothes.parts.Count == 8)
            {
                //No clothes state is set, default full dressing
                Log.LogInfo("UpdateClothesStateToggleButton, part = 8 full wear, coordinateType: " + coordinateType);
                StateManager.Instance.FullDressingToggle.isOn = true;
            }
            else
            {
                Log.LogInfo("UpdateClothesStateToggleButton, part > 8, coordinateType: " + coordinateType + ", sno: " + sno + ", state: " + !character.ChaFile.Coordinate[coordinateType].clothes.parts[8].hideOpt[sno]);
                if (character.ChaFile.Coordinate[coordinateType].clothes.parts[8].hideOpt[sno])
                    StateManager.Instance.HalfDressingToggle.SetIsOnWithoutNotify(true);
                else
                    StateManager.Instance.FullDressingToggle.SetIsOnWithoutNotify(true);
            }
        }

        //Set the clothes state of the model in edit screen
        internal static void ReflectClothesState(Chara.ChaControl character, int coordinateType)
        {
            //havent set any clothes state, set default state
            if (character.ChaFile.Coordinate[coordinateType].clothes.parts.Count == 8)
            {
                Log.LogInfo("ReflectClothesState1 part = 8");
                for (int i = 0; i < 8; i++)
                {
                    character.FileStatus.clothesState[i] = 0;
                }
                return;
            }
            Log.LogInfo("ReflectClothesState coordinatetype: " + coordinateType);
            var clothesStateInfo = character.ChaFile.Coordinate[coordinateType].clothes.parts[8];

            for (int i = 0; i < 8; i++)
            {
                Log.LogInfo("i: " + i + ", value: " + clothesStateInfo.hideOpt[i] + ", setvalue: " + System.Math.Max(character.FileStatus.clothesState[i], clothesStateInfo.hideOpt[i] ? (byte)1 : (byte)0));
                //character.SetClothesState(i, System.Math.Max(character.FileStatus.clothesState[i], clothesStateInfo.hideOpt[i] ? (byte)1 : (byte)0)  );
                character.FileStatus.clothesState[i] = System.Math.Max(character.FileStatus.clothesState[i], clothesStateInfo.hideOpt[i] ? (byte)1 : (byte)0);
            }
        }

        public static void ClothesStateToggleOnClick(bool value)
        {
            Log.LogInfo("ClothesStateToggleOnClick");
            //var tg = clothesStateObject.GetComponent<ToggleGroup>();
            if (StateManager.Instance.CharacterControl != null)
            {
                
                Util.UpdateClothesState(StateManager.Instance.CharacterControl, StateManager.Instance.ClothesCanvas.coordinateType, StateManager.Instance.ClothesCanvas.SNo, StateManager.Instance.HalfDressingToggle.isOn);
                StateManager.Instance.CharacterControl.SetClothesState(StateManager.Instance.ClothesCanvas.SNo, StateManager.Instance.FullDressingToggle.isOn ? (byte)0 : (byte)1);
                ReflectClothesState(StateManager.Instance.CharacterControl, StateManager.Instance.ClothesCanvas.coordinateType);
            }

        }

        internal static void ResetClothesState(Chara.ChaControl character)
        {
            for (int i = 0; i < 8; i++)
            {
                character.SetClothesState(i, 0);
            }
        }

        internal static void ShowTabOthers(CvsC_Clothes cvsClothes)
        {
            if (cvsClothes.chaCtrl.Sex != 0)
                cvsClothes.ShowOrHideTab(true, 4);
        }
        
        //Need to copy the clothes state part info
        internal static void CopyClothesStates(CvsC_Copy cvsCopy)
        {   
            var sourcePartInfo = cvsCopy.chaCtrl.ChaFile.Coordinate[cvsCopy._ddSrc.Value].clothes.parts;
            var destPartInfo = cvsCopy.chaCtrl.ChaFile.Coordinate[cvsCopy._ddDst.Value].clothes.parts;

            if (sourcePartInfo.Length == 8)
                return;
            else
            {
                var pInfo = Util.ExpandClothesStateArray(destPartInfo);
                for (int i = 0; i < cvsCopy._tglSelect.Length; i++)
                {
                    if (cvsCopy._tglSelect[i].isOn)
                    {
                        pInfo[8].hideOpt[i] = sourcePartInfo[8].hideOpt[i];
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
            StateManager.Instance.ClothesStateCanvas = Util.InstantiateFromBundle(StateManager.Instance.ClothesStateAB, "assets/prefab/cvsclothesstate.prefab").GetComponent<Canvas>();
            StateManager.Instance.ClothesStateCanvas.gameObject.SetActive(true);
            StateManager.Instance.ClothesStateCanvas.transform.SetParent(contentHolder.transform, false);

            //Locate the text and set font
            var lblClothesState = StateManager.Instance.ClothesStateCanvas.transform.Find("lblClothesState").GetComponent<Text>();
            lblClothesState.font = StateManager.Instance.YuGothicFont;
            var lblFull = StateManager.Instance.ClothesStateCanvas.transform.Find("togFull").Find("lblFull").GetComponent<Text>();
            lblFull.font = StateManager.Instance.YuGothicFont;
            var lblHalf = StateManager.Instance.ClothesStateCanvas.transform.Find("togHalf").Find("lblHalf").GetComponent<Text>();
            lblHalf.font = StateManager.Instance.YuGothicFont;

            //Set the Toggle button action
            StateManager.Instance.FullDressingToggle = StateManager.Instance.ClothesStateCanvas.transform.Find("togFull").GetComponent<Toggle>();
            StateManager.Instance.HalfDressingToggle = StateManager.Instance.ClothesStateCanvas.transform.Find("togHalf").GetComponent<Toggle>();
            StateManager.Instance.FullDressingToggle.isOn = false;
            StateManager.Instance.HalfDressingToggle.onValueChanged = new Toggle.ToggleEvent();
            StateManager.Instance.HalfDressingToggle.onValueChanged.AddListener((UnityAction<bool>)Patches.ClothesStateToggleOnClick);
            StateManager.Instance.FullDressingToggle.onValueChanged = new Toggle.ToggleEvent();
            StateManager.Instance.FullDressingToggle.onValueChanged.AddListener((UnityAction<bool>)Patches.ClothesStateToggleOnClick);
        }
    }
}
