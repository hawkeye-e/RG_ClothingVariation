using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;

namespace RGClothingVariation
{
    internal class Debug
    {

        private static ManualLogSource Log = RGClothingVariationPlugin.Log;

        public static void PrintCoordinateInfo(Chara.ChaControl character)
        {
            Log.LogInfo("===========PrintCoordinateInfo===========");

            Log.LogInfo("Character: " + character.FileParam.fullname);
            for (int i = 0; i < character.ChaFile.Coordinate.Count; i++)
            {
                Log.LogInfo("Coordinate: " + i + ", part count: " + character.ChaFile.Coordinate[i].clothes.parts.Count);
                for (int j = 0; j < Constant.ClothesPartCount; j++)
                {
                    Log.LogInfo("Coordinate: " + i + ", part : " + j + ", part id: " + character.ChaFile.Coordinate[i].clothes.parts[j].id + ", clothesState: " + character.FileStatus.clothesState[j]);
                    
                }
                PrintExtraField(character.ChaFile.Coordinate[i]);
            }
            Log.LogInfo("Now coordinate part count: " + character.NowCoordinate.clothes.parts.Count);
            for (int j = 0; j < Constant.ClothesPartCount; j++)
            {
                Log.LogInfo("Now coordinate, part : " + j + ", part id: " + character.NowCoordinate.clothes.parts[j].id + ", clothesState: " + character.FileStatus.clothesState[j]);
            }
            PrintExtraField(character.NowCoordinate);
            Log.LogInfo("=========================================");
        }


        public static void PrintExtraField(Chara.ChaFileCoordinate coord)
        {
            if (coord.clothes.parts.Count > Constant.ClothesPartCount)
            {
                Log.LogInfo("**********Extra Field Info**********");
                for (int i = 0; i < Constant.ClothesPartCount; i++)
                {
                    Log.LogInfo("i: " + i
                        + ", Clothes State: " + (coord.clothes.parts[Constant.ExtraFieldPartNumber].hideOpt[i] ? "Half Dress" : "Full Dress")
                        + ", Original Clothes Type: " + coord.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[i].layout.x
                        + ", Original Clothes ID: " + coord.clothes.parts[Constant.ExtraFieldPartNumber].colorInfo[i].layout.y
                        );
                }


            }
        }

        public static void PrintDetail(object a)
        {
            foreach (var prop in a.GetType().GetProperties())
            {
                try
                {

                    object value = prop.GetValue(a, null);
                    if (value != null)
                        Log.Log(LogLevel.Info, prop.Name + "=" + value);
                    else
                        Log.Log(LogLevel.Info, prop.Name + " is null!!");
                }
                catch { }
            }
            Log.LogInfo("================");
        }

        public static void PrintCustomTable(int category)
        {
            var a = Manager.Character.Instance.CustomTableData._instance[category];
            foreach (var b in a)
            {

                Log.LogInfo("b.Key: " + b.Key);
                foreach (var c in b.Value)
                {
                    Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c.Category: " + c.Category);

                    foreach (var d in c._dic)
                    {
                        Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c.Category: " + c.Category + ", d.Key: " + d.Key);

                        foreach (var e in d.Value)
                        {
                            Log.LogInfo("b.Key: " + b.Key + ", c.ID: " + c.ID + ", c.Category: " + c.Category + ", d.Key: " + d.Key + ", e: " + e);


                        }

                    }

                }
            }
        }

        internal static void PrintTransformTree(Transform t, string currentPath)
        {
            if (t != null)
            {
                Log.LogInfo("Path: " + currentPath);
                Log.LogInfo("Name: " + t.name);
                Log.LogInfo("Active: " + t.gameObject.active);
                //PrintDetail(t.gameObject);
                //PrintDetail(t);
                GetComponentTypes(t);

                var mono = t.GetComponent<MonoBehaviour>();
                if (mono != null)
                {
                    Log.LogInfo("GetScriptClassName: " + mono.GetScriptClassName());
                }

                Log.LogInfo("Position: " + t.position);
                Log.LogInfo("LocalPosition: " + t.localPosition);
                var r = t.GetComponent<RectTransform>();
                if (r != null)
                {
                    Log.LogInfo("Width: " + r.rect.width + ", height: " + r.rect.height);
                    Log.LogInfo("bottom: " + r.rect.bottom + ", top: " + r.rect.top);
                }
                Log.LogInfo("Child Count: " + t.childCount);

                Log.LogInfo("");
                for (int i = 0; i < t.GetChildCount(); i++)
                {
                    Log.LogInfo("Visiting the child of [" + t.name + "]");
                    PrintTransformTree(t.GetChild(i), currentPath + ".[" + t.name + "]");
                }
                Log.LogInfo("");
            }
        }

        internal static void PrintClothesState(Chara.ChaControl character)
        {

            foreach (var a in character.DictStateType)
            {
                Log.LogInfo("a.Key: " + a.Key);
                foreach (var b in a.Value)
                {
                    Log.LogInfo("a.Key: " + a.Key + ", b.Key: " + b.Key + ", b.Value: " + b.Value);
                }
            }
        }

        internal static void PrintCharacterVisibleStatus()
        {
            foreach (var character in StateManager.Instance.HSceneInstance._chaFemales)
            {
                if (character != null)
                    Log.LogInfo("Name: " + character.FileParam.fullname + ", ID: " + character.GetInstanceID() + ", visible:" + character.IsVisibleInCamera + ", active: " + character.isActiveAndEnabled);

            }
            foreach (var character in StateManager.Instance.HSceneInstance._chaMales)
            {
                if (character != null)
                    //Debug.PrintDetail(character);
                    Log.LogInfo("Name: " + character.FileParam.fullname + ", visible:" + character.IsVisibleInCamera + ", active: " + character.isActiveAndEnabled);
            }
        }

        internal static void PrintDictHSceneClothesStates(Chara.ChaControl character)
        {
            if (StateManager.Instance.DictHSceneClothesStates.ContainsKey(character.GetInstanceID()))
            {
                Log.LogInfo("Name: " + character.FileParam.fullname);
                var a = StateManager.Instance.DictHSceneClothesStates[character.GetInstanceID()];

                for (int i = 0; i < Constant.ClothesPartCount; i++)
                {
                    if (a.ContainsKey(i) && a.ContainsKey(i + Constant.ClothesPartCount))
                        Log.LogInfo("SlotNumber: " + i + ", Top State: " + a[i] + ", Bottom State: " + a[i + Constant.ClothesPartCount]);
                    else if (a.ContainsKey(i))
                        Log.LogInfo("SlotNumber: " + i + ", Top State: " + a[i] + ", Bottom State: Empty");
                    else if (a.ContainsKey(i + Constant.ClothesPartCount))
                        Log.LogInfo("SlotNumber: " + i + ", Top State: Empty , Bottom State: " + a[i + Constant.ClothesPartCount]);
                    else
                        Log.LogInfo("SlotNumber: " + i + ", Top State: Empty , Bottom State: Empty");
                }
            }
        }

        internal static void PrintHSceneClothesStateButtonStates()
        {
            if (StateManager.Instance.HSceneClothButtonGroup != null)
            {
                Log.LogInfo("_clothObjSets.Count: " + StateManager.Instance.HSceneClothButtonGroup._clothObjSets.Count);
                for (int i = 0; i < StateManager.Instance.HSceneClothButtonGroup._clothObjSets.Count; i++)
                {
                    Log.LogInfo("_clothObjSets.Count: " + StateManager.Instance.HSceneClothButtonGroup._clothObjSets[i].Obj.buttons.Count);
                    for (int j = 0; j < StateManager.Instance.HSceneClothButtonGroup._clothObjSets[i].Obj.buttons.Count; j++)
                    {
                        var b = StateManager.Instance.HSceneClothButtonGroup._clothObjSets[i].Obj.buttons[j];
                        Log.LogInfo("Button, i: " + i + " j: " + j + ", name: " + b.name + ", active: " + b.isActiveAndEnabled);
                    }
                }
            }
        }

        private static void GetComponentTypes(Transform t)
        {
            var c1 = t.GetComponent<Renderer>(); if (c1 != null) Log.LogInfo("has Renderer");
            var c2 = t.GetComponent<MeshFilter>(); if (c2 != null) Log.LogInfo("has MeshFilter");
            var c3 = t.GetComponent<LODGroup>(); if (c3 != null) Log.LogInfo("has LODGroup");
            var c4 = t.GetComponent<Behaviour>(); if (c4 != null) Log.LogInfo("has Behaviour");

            var c5 = t.GetComponent<Transform>(); if (c5 != null) Log.LogInfo("has Transform");
            var c6 = t.GetComponent<CanvasRenderer>(); if (c6 != null) Log.LogInfo("has CanvasRenderer");
            var c7 = t.GetComponent<Component>(); if (c7 != null) Log.LogInfo("has Component");
            var c8 = t.GetComponent<RectTransform>(); if (c8 != null) Log.LogInfo("has RectTransform");

            var c9 = t.GetComponent<BillboardRenderer>(); if (c9 != null) Log.LogInfo("has BillboardRenderer");
            var c10 = t.GetComponent<LineRenderer>(); if (c10 != null) Log.LogInfo("has LineRenderer");
            var c11 = t.GetComponent<SkinnedMeshRenderer>(); if (c11 != null) Log.LogInfo("has SkinnedMeshRenderer");
            var c12 = t.GetComponent<MeshRenderer>(); if (c12 != null) Log.LogInfo("has MeshRenderer");
            var c13 = t.GetComponent<SpriteRenderer>(); if (c13 != null) Log.LogInfo("has SpriteRenderer");
            var c14 = t.GetComponent<Animator>(); if (c14 != null) Log.LogInfo("has Animator");
            var c15 = t.GetComponent<MonoBehaviour>(); if (c15 != null) Log.LogInfo("has MonoBehaviour");

            var c17 = t.GetComponent<VerticalLayoutGroup>(); if (c17 != null) Log.LogInfo("has VerticalLayoutGroup");
            var c18 = t.GetComponent<HorizontalLayoutGroup>(); if (c18 != null) Log.LogInfo("has HorizontalLayoutGroup");
            var c19 = t.GetComponent<LayoutGroup>(); if (c19 != null) Log.LogInfo("has LayoutGroup");
            var c20 = t.GetComponent<GridLayoutGroup>(); if (c20 != null) Log.LogInfo("has GridLayoutGroup");
            var c21 = t.GetComponent<ContentSizeFitter>(); if (c21 != null) Log.LogInfo("has ContentSizeFitter");
            var c22 = t.GetComponent<Canvas>(); if (c22 != null) Log.LogInfo("has Canvas");
            var c23 = t.GetComponent<ContentSizeFitter>(); if (c23 != null) Log.LogInfo("has ContentSizeFitter");
            var c24 = t.GetComponent<Collider>(); if (c24 != null) Log.LogInfo("has Collider");

            var c25 = t.GetComponent<CharacterController>(); if (c25 != null) Log.LogInfo("has CharacterController");
            var c26 = t.GetComponent<MeshCollider>(); if (c26 != null) Log.LogInfo("has MeshCollider");
            var c27 = t.GetComponent<CapsuleCollider>(); if (c27 != null) Log.LogInfo("has CapsuleCollider");
            var c28 = t.GetComponent<BoxCollider>(); if (c28 != null) Log.LogInfo("has BoxCollider");
            var c29 = t.GetComponent<SphereCollider>(); if (c29 != null) Log.LogInfo("has SphereCollider");
        }
    }
}
