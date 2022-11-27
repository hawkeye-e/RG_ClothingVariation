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
            for(int i=0; i < character.ChaFile.Coordinate.Count; i++)
            {
                Log.LogInfo("Coordinate: " + i + ", part count: " + character.ChaFile.Coordinate[i].clothes.parts.Count);
                for (int j=0; j< 8; j++)
                {
                    Log.LogInfo("Coordinate: " + i + ", part : " + j + ", part id: " + character.ChaFile.Coordinate[i].clothes.parts[j].id);
                }
                
            }
            Log.LogInfo("Now coordinate part count: " + character.NowCoordinate.clothes.parts.Count);
            for (int j = 0; j < 8; j++)
            {
                Log.LogInfo("Now coordinate, part : " + j + ", part id: " + character.NowCoordinate.clothes.parts[j].id);
            }

            Log.LogInfo("=========================================");
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
    }
}
