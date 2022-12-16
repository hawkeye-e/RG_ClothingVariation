using BepInEx.Logging;

namespace RGClothingVariation.PatchInitialize
{
    internal class Patches
    {
        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        internal static void InitializeCustomClothesMapping()
        {
            if (!StateManager.Instance.isClothesMappingProcessed)
            {
                StateManager.Instance.isClothesMappingProcessed = true;

                var dataTop = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_top];
                var dataBottom = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_bot];
                var dataInnerTop = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_inner_t];
                var dataInnerBottom = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_inner_b];
                var dataGloves = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_gloves];
                var dataPantyhose = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_panst];
                var dataSocks = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_socks];
                var dataShoes = Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_shoes];

                var dataFullList = new Illusion.Collections.Generic.Optimized.Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>>();

                AddCustomTableData(dataFullList, dataTop, 0);
                AddCustomTableData(dataFullList, dataBottom, 1);
                AddCustomTableData(dataFullList, dataInnerTop, 2);
                AddCustomTableData(dataFullList, dataInnerBottom, 3);
                AddCustomTableData(dataFullList, dataGloves, 4);
                AddCustomTableData(dataFullList, dataPantyhose, 5);
                AddCustomTableData(dataFullList, dataSocks, 6);
                AddCustomTableData(dataFullList, dataShoes, 7);

                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_top] = dataFullList;
                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_bot] = dataFullList;
                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_inner_t] = dataFullList;
                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_inner_b] = dataFullList;
                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_gloves] = dataFullList;
                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_panst] = dataFullList;
                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_socks] = dataFullList;
                Manager.Character.Instance.CustomTableData._instance[(int)Chara.ChaListDefine.CategoryNo.fo_shoes] = dataFullList;
            }
        }

        internal static void AddCustomTableData(
            Illusion.Collections.Generic.Optimized.Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> fullList,
            Illusion.Collections.Generic.Optimized.Int32KeyDictionary<Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>> toBeCopiedList,
            int clothesType
            )
        {
            foreach (var level1 in toBeCopiedList)
            {
                int newKey = StateManager.Instance.ClothesMappingDict.Count;

                Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo> listLevel1 = new Il2CppSystem.Collections.Generic.List<Chara.CustomTableData.CustomInfo>();
                foreach (var level2 in level1.Value)
                {
                    Chara.CustomTableData.CustomInfo newCustomInfo = new Chara.CustomTableData.CustomInfo();
                    newCustomInfo._dic = new Illusion.Collections.Generic.Optimized.Int32KeyDictionary<RG.TableData<Chara.CustomTableData.CustomInfo>.Info.RevisedList<string>>();

                    foreach (var level3 in level2._dic)
                    {
                        var newRevisedList = new RG.TableData<Chara.CustomTableData.CustomInfo>.Info.RevisedList<string>();

                        foreach (var level4 in level3.Value)
                        {
                            if (level3.Key == (int)Chara.ChaListDefine.KeyType.ID)
                                newRevisedList.Add(newKey.ToString());
                            else if (level3.Key == (int)Chara.ChaListDefine.KeyType.ListIndex)
                                newRevisedList.Add(newKey.ToString());
                            else if (level3.Key == (int)Chara.ChaListDefine.KeyType.Coordinate)     //flag to control enable/disable the Bottom/Inner Bottom choice
                                newRevisedList.Add("0");
                            else
                                newRevisedList.Add(level4);
                        }

                        newCustomInfo._dic.Add(level3.Key, newRevisedList);
                    }

                    listLevel1.Add(newCustomInfo);
                }


                //add key info to the mapping table
                StateManager.Instance.ClothesMappingDict.Add((clothesType, level1.Key), newKey);
                fullList.Add(newKey, listLevel1);
                //add also to the cache table
                if (clothesType == 0)
                    StateManager.Instance.ClothesTableCache.Top.Add(newKey, listLevel1);
                else if (clothesType == 1)
                    StateManager.Instance.ClothesTableCache.Bottom.Add(newKey, listLevel1);
                else if (clothesType == 2)
                    StateManager.Instance.ClothesTableCache.InnerTop.Add(newKey, listLevel1);
                else if (clothesType == 3)
                    StateManager.Instance.ClothesTableCache.InnerBottom.Add(newKey, listLevel1);
                else if (clothesType == 4)
                    StateManager.Instance.ClothesTableCache.Gloves.Add(newKey, listLevel1);
                else if (clothesType == 5)
                    StateManager.Instance.ClothesTableCache.Pantyhose.Add(newKey, listLevel1);
                else if (clothesType == 6)
                    StateManager.Instance.ClothesTableCache.Socks.Add(newKey, listLevel1);
                else if (clothesType == 7)
                    StateManager.Instance.ClothesTableCache.Shoes.Add(newKey, listLevel1);
            }
        }
    }
}
