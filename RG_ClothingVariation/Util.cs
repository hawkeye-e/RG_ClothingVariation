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

        internal static GameObject InstantiateFromBundle(AssetBundle bundle, string assetName)
        {
            var asset = bundle.LoadAsset(assetName, Il2CppType.From(typeof(GameObject)));
            Log.LogInfo("InitializeUI pt a1 ");
            var obj = Object.Instantiate(asset);
            Log.LogInfo("InitializeUI pt a2 ");
            foreach (var rootGameObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.GetInstanceID() == obj.GetInstanceID())
                {
                    rootGameObject.name = assetName;
                    return rootGameObject;
                }
            }
            Log.LogInfo("InitializeUI pt a3 ");
            throw new FileLoadException("Could not instantiate asset " + assetName);
        }

        internal static Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo> ExpandClothesStateArray(Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo> partArray)
        {
            if (partArray.Count > 8)
            {
                //already expanded
                return partArray;
            }

            var clothesStateInfo = new Chara.ChaFileClothes.PartsInfo();
            clothesStateInfo.id = -999;
            clothesStateInfo.hideOpt = new Il2CppStructArray<bool>(8);
            for (int j = 0; j < clothesStateInfo.hideOpt.Length; j++)
                clothesStateInfo.hideOpt[j] = false;
            clothesStateInfo.colorInfo = new Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo.ColorInfo>(0);
            clothesStateInfo.breakRate = 0f;
            clothesStateInfo._id_k__BackingField = -999;
            clothesStateInfo._hideOpt_k__BackingField = clothesStateInfo.hideOpt;

            Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo> pInfo = new Il2CppReferenceArray<Chara.ChaFileClothes.PartsInfo>(9);
            for (int i = 0; i < partArray.Length; i++)
            {
                pInfo[i] = partArray[i];
            }
            pInfo[partArray.Length] = clothesStateInfo;
            return pInfo;
        }

        internal static void UpdateClothesState(Chara.ChaControl character, int coordinateType, int clothesType, bool isHalf)
        {
            Chara.ChaFileClothes.PartsInfo clothesStateInfo;
            if (character.ChaFile.Coordinate[coordinateType].clothes.parts.Count == 8)
            {
                Log.LogInfo("add fake part");
                //add a fake part for storing the clothes part
                character.ChaFile.Coordinate[coordinateType].clothes.parts = ExpandClothesStateArray(character.ChaFile.Coordinate[coordinateType].clothes.parts);
                Log.LogInfo("part count: " + character.ChaFile.Coordinate[coordinateType].clothes.parts.Length);
            }
            else
            {
                Log.LogInfo("get fake part");
                //fake part already exists, try to get the fake part item
                //clothesStateInfo = character.ChaFile.Coordinate[coordinateType].clothes.parts[8];
            }
            clothesStateInfo = character.ChaFile.Coordinate[coordinateType].clothes.parts[8];
            clothesStateInfo.hideOpt[clothesType] = isHalf;

            Log.LogInfo("Check coordinate var, part count: " + character.NowCoordinate.clothes.parts.Length);

        }
    }
}
