using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.IL2CPP;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace RGClothingVariation
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class RGClothingVariationPlugin : BasePlugin
    {
        public const string PluginName = "RGClothingVariation";
        public const string GUID = "hawk.RG.ClothingVariation";
        public const string Version = "0.1";

        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;

            RGClothingVariation.Config.Init(this);

            if (RGClothingVariation.Config.Enabled)
            {
                Harmony.CreateAndPatchAll(typeof(PatchInitialize.Hook), GUID);
                Harmony.CreateAndPatchAll(typeof(CharacterCustom.Hook), GUID);
                Harmony.CreateAndPatchAll(typeof(ActionSceneScreen.Hook), GUID);
                Harmony.CreateAndPatchAll(typeof(HSceneScreen.Hook), GUID);
            }

            StateManager.Instance = new StateManager();
        }


    }
}
