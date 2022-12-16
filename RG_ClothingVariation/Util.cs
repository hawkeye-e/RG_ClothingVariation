using BepInEx.Logging;
using UnhollowerBaseLib;
using UnityEngine;
using UnhollowerRuntimeLib;
using System.IO;

namespace RGClothingVariation
{
    internal class Util
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        internal static bool IsCharacterFemaleBody(Chara.ChaControl character)
        {
            if (!(character.Sex == 1 || character.FileParam.futanari))
                return false;
            else
                return true;
        }
        internal static GameObject InstantiateFromBundle(AssetBundle bundle, string assetName)
        {
            var asset = bundle.LoadAsset(assetName, Il2CppType.From(typeof(GameObject)));
            var obj = Object.Instantiate(asset);
            foreach (var rootGameObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.GetInstanceID() == obj.GetInstanceID())
                {
                    rootGameObject.name = assetName;
                    return rootGameObject;
                }
            }
            throw new FileLoadException("Could not instantiate asset " + assetName);
        }

        internal static void InitializeCharacterClothesExtraFields(Chara.ChaControl character)
        {
            //Apply to female body only
            if (!IsCharacterFemaleBody(character)) return;

            //The ID in the full list may changed due to adding/removing clothes asset, need to check and replace with corrected full list ID
            for (int i = 0; i < character.ChaFile.Coordinate.Count; i++)
            {
                ConvertCoordinateData(character.ChaFile.Coordinate[i]);
            }
            ConvertCoordinateData(character.NowCoordinate);


            for (int j = 0; j < Constant.ClothesPartCount; j++)
            {
                character.ChangeClothes(j, character.NowCoordinate.clothes.parts[j].id);
            }
        }

        internal static Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo> ExpandClothesStateArray(Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo> partArray)
        {
            if (partArray.Count > Constant.ClothesPartCount)
            {
                //already expanded
                return partArray;
            }

            var clothesStateInfo = new Chara.ChaFileClothes.PartsInfo();
            clothesStateInfo.id = -999;
            clothesStateInfo.hideOpt = new Il2CppStructArray<bool>(Constant.ClothesPartCount * 2);
            clothesStateInfo.colorInfo = new Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo.ColorInfo>(Constant.ClothesPartCount);
            for (int i = 0; i < Constant.ClothesPartCount; i++) {
                //default clothes state: Full dressing
                clothesStateInfo.hideOpt[i] = false;
                clothesStateInfo.hideOpt[i + Constant.ClothesPartCount] = false;
                //copy also the original clothes type and id
                clothesStateInfo.colorInfo[i] = new Chara.ChaFileClothes.PartsInfo.ColorInfo();
                clothesStateInfo.colorInfo[i].layout = new Vector2(i, partArray[i].id);
            }
            //clothesStateInfo.colorInfo = new Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo.ColorInfo>(0);
            clothesStateInfo.breakRate = 0f;
            clothesStateInfo._id_k__BackingField = -999;
            clothesStateInfo._hideOpt_k__BackingField = clothesStateInfo.hideOpt;
            clothesStateInfo.id = Constant.DataStructureVersion;
            

            Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo> pInfo = new Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo>(9);
            for (int i = 0; i < partArray.Length; i++)
            {
                pInfo[i] = partArray[i];
            }
            pInfo[partArray.Length] = clothesStateInfo;
            return pInfo;
        }

        internal static void ConvertCoordinateData(Chara.ChaFileCoordinate coordinate)
        {
            if (coordinate.clothes.parts.Length == Constant.ClothesPartCount)
            {
                coordinate.clothes.parts = ExpandClothesStateArray(coordinate.clothes.parts);
                //fill in the full list id to the status field
                for (int j = 0; j < Constant.ClothesPartCount; j++)
                {
                    var origInfo = coordinate.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[j].layout;
                    coordinate.clothes.parts[j].id = StateManager.Instance.ClothesMappingDict[((int)origInfo.x, (int)origInfo.y)];
                }
            }
        }

        internal static void UpdateClothesState(Chara.ChaControl character, int coordinateType, int clothesType, bool isTopHalf, bool isBottomHalf)
        {
            Chara.ChaFileClothes.PartsInfo clothesStateInfo;
            if (character.ChaFile.Coordinate[coordinateType].clothes.parts.Count == Constant.ClothesPartCount)
            {
                //add a fake part for storing the clothes part
                character.ChaFile.Coordinate[coordinateType].clothes.parts = ExpandClothesStateArray(character.ChaFile.Coordinate[coordinateType].clothes.parts);
            }
            
            clothesStateInfo = character.ChaFile.Coordinate[coordinateType].clothes.parts[Constant.ExtraFieldPartNumber];
            clothesStateInfo.hideOpt[clothesType] = isTopHalf;
            clothesStateInfo.hideOpt[clothesType + Constant.ClothesPartCount] = isBottomHalf;

        }

        internal static (int, int) FindClothesMappingKeyByValue(int id)
        {
            foreach(var kvp in StateManager.Instance.ClothesMappingDict)
            {
                if (kvp.Value == id)
                    return kvp.Key;
            }
            return (-1, -1);
        }

        internal static int GetClothesSlotNumberByObjectName(string name)
        {
            switch (name)
            {
                case Constant.ClothesPartObjectName.Top:
                    return 0;
                case Constant.ClothesPartObjectName.Bottom:
                    return 1;
                case Constant.ClothesPartObjectName.InnerTop:
                    return 2;
                case Constant.ClothesPartObjectName.InnerBottom:
                    return 3;
                case Constant.ClothesPartObjectName.Gloves:
                    return 4;
                case Constant.ClothesPartObjectName.PantyHose:
                    return 5;
                case Constant.ClothesPartObjectName.Socks:
                    return 6;
                case Constant.ClothesPartObjectName.Shoes:
                    return 7;
                default:
                    return -1;

            }
        }

        internal static bool IsGameObjectNameClothesPart(string name)
        {
            if (name == Constant.ClothesPartObjectName.Top
                || name == Constant.ClothesPartObjectName.Bottom
                || name == Constant.ClothesPartObjectName.InnerTop
                || name == Constant.ClothesPartObjectName.InnerBottom
                || name == Constant.ClothesPartObjectName.PantyHose
                || name == Constant.ClothesPartObjectName.Gloves
                || name == Constant.ClothesPartObjectName.Socks
                || name == Constant.ClothesPartObjectName.Shoes
                )
            {
                return true;
            }
            return false;
        }

        internal static bool IsGameObjectNameClothesState(string name)
        {
            if (name == Constant.ClothesStateObjectName.TopFull
                || name == Constant.ClothesStateObjectName.TopHalf
                || name == Constant.ClothesStateObjectName.BottomFull
                || name == Constant.ClothesStateObjectName.BottomHalf
                )
            {
                return true;
            }
            return false;
        }

        internal static int GetOverrideClothesStateByClothesID(int overallClothesStatus, int clothesType)
        {
            if (overallClothesStatus == Constant.UserSelectedClothesStates.FullWear)
                return Constant.OverallClothesStateValue.Default[clothesType];
            if (overallClothesStatus == Constant.UserSelectedClothesStates.UnderwearOnly)
                return Constant.OverallClothesStateValue.UnderwearOnly[clothesType];
            if (overallClothesStatus == Constant.UserSelectedClothesStates.Nude)
                return Constant.OverallClothesStateValue.Nude[clothesType];
            else
                return Constant.OverallClothesStateValue.Default[clothesType];
        }

        internal static string GetClothesPartNameByCategoryNo(int cateNo)
        {
            switch (cateNo)
            {
                case (int)Chara.ChaListDefine.CategoryNo.fo_top:
                    return Constant.ClothesPartName.Top;
                case (int)Chara.ChaListDefine.CategoryNo.fo_bot:
                    return Constant.ClothesPartName.Bottom;
                case (int)Chara.ChaListDefine.CategoryNo.fo_inner_t:
                    return Constant.ClothesPartName.InnerTop;
                case (int)Chara.ChaListDefine.CategoryNo.fo_inner_b:
                    return Constant.ClothesPartName.InnerBottom;
                case (int)Chara.ChaListDefine.CategoryNo.fo_gloves:
                    return Constant.ClothesPartName.Gloves;
                case (int)Chara.ChaListDefine.CategoryNo.fo_panst:
                    return Constant.ClothesPartName.Pantyhose;
                case (int)Chara.ChaListDefine.CategoryNo.fo_socks:
                    return Constant.ClothesPartName.Socks;
                case (int)Chara.ChaListDefine.CategoryNo.fo_shoes:
                    return Constant.ClothesPartName.Shoes;
                default:
                    return "";
            }
        }

        internal static string GetClothesPartNameByClothesKind(int kind)
        {
            switch (kind)
            {
                case 0:
                    return Constant.ClothesPartName.Top;
                case 1:
                    return Constant.ClothesPartName.Bottom;
                case 2:
                    return Constant.ClothesPartName.InnerTop;
                case 3:
                    return Constant.ClothesPartName.InnerBottom;
                case 4:
                    return Constant.ClothesPartName.Gloves;
                case 5:
                    return Constant.ClothesPartName.Pantyhose;
                case 6:
                    return Constant.ClothesPartName.Socks;
                case 7:
                    return Constant.ClothesPartName.Shoes;
                default:
                    return "";
            }
        }

    }
}
