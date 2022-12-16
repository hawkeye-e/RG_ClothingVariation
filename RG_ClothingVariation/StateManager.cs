using UnityEngine;
using UnityEngine.UI;
using CharaCustom;
using System.Collections.Generic;

using Illusion.Collections.Generic.Optimized;

namespace RGClothingVariation
{
    internal class StateManager
    {
        public StateManager()
        {
            ClothesMappingDict = new Dictionary<(int, int), int>();
            ClothesTableCache = new ClotheInfoTable();
            DictHSceneClothesStates = new Dictionary<int, Dictionary<int, int>>();
            HSceneVisibleCharacterInstanceIDList = new List<int>();
        }

        internal static StateManager Instance;

        internal AssetBundle ClothesStateAB = null;
        internal Canvas TopClothesStateCanvas = null;
        internal Canvas BottomClothesStateCanvas = null;
        internal Canvas SelectClothesCategoryCanvas = null;
        internal Toggle TopFullDressingToggle = null;
        internal Toggle TopHalfDressingToggle = null;
        internal Toggle BottomFullDressingToggle = null;
        internal Toggle BottomHalfDressingToggle = null;
        internal Chara.ChaControl CharacterControl = null;
        internal CvsC_Clothes ClothesCanvas = null;
        internal Font YuGothicFont = null;
        

        //Key: (ClothesType, ID), Value: Key in the full clothes list dictionary
        internal Dictionary<(int, int), int> ClothesMappingDict = null;
        internal bool isClothesMappingProcessed = false;

        internal int CategoryBasedClothesState = Constant.CategoryBasedClothesStates.Default;
        internal int UserSelectedClothesState = Constant.UserSelectedClothesStates.Default;

        internal ClotheInfoTable ClothesTableCache;

        internal bool IsClothesCategorySelectChanged = false;
        internal Text ClothesCanvasSelectName = null;
        internal string ClothesCanvasSelectText = "";
        internal CustomSelectScrollController ClothesScrollController = null;

        internal bool IsClothesSlotSelectChanged = false;
        internal bool IsMajorCategorySelectChanged = false;
        internal bool IsClothesCategorySelected = false;

        internal bool IsSavingThumbnail = false;

        internal bool IsCharaCustomScreen = false;
        internal bool IsHScene = false;

        internal HSceneSpriteClothCondition HSceneClothButtonGroup = null;

        //Key: ChaControl InstanceID, Value:  Dictionary[Key: SlotNumber(0-7 top, 8-15 bottom), Value: state ]
        internal Dictionary<int, Dictionary<int, int>> DictHSceneClothesStates = null;

        internal bool HSceneForceAllClothesHidden = false;

        internal bool HsceneMale1Clothes = true;
        internal bool HsceneMale2Clothes = true;
        internal bool HsceneMale1Shoes = true;
        internal bool HsceneMale2Shoes = true;
        internal RG.Config.HSystem HSceneConfig = null;
        
        internal HScene HSceneInstance = null;
        internal List<int> HSceneVisibleCharacterInstanceIDList = null;

        internal class ClotheInfoTable
        {
            public ClotheInfoTable()
            {
                Top = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
                Bottom = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
                InnerTop = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
                InnerBottom = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
                Gloves = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
                Pantyhose = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
                Socks = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
                Shoes = new Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();
            }

            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> Top;
            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> Bottom;
            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> InnerTop;
            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> InnerBottom;
            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> Gloves;
            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> Pantyhose;
            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> Socks;
            internal Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> Shoes;
        }

        internal static int GetOverallClothesStatus()
        {
            if (!Instance.IsCharaCustomScreen) return 0;

            int overallClothesStatus;
            if (Instance.UserSelectedClothesState != Constant.UserSelectedClothesStates.Default)
            {
                overallClothesStatus = Instance.UserSelectedClothesState;
            }
            else
            {
                if (Instance.CategoryBasedClothesState == Constant.CategoryBasedClothesStates.UnderwearOnly)
                    overallClothesStatus = Constant.UserSelectedClothesStates.UnderwearOnly;
                else if (Instance.CategoryBasedClothesState == Constant.CategoryBasedClothesStates.Nude)
                    overallClothesStatus = Constant.UserSelectedClothesStates.Nude;
                else
                    overallClothesStatus = Constant.UserSelectedClothesStates.Default;
            }
            return overallClothesStatus;
        }
    }
}