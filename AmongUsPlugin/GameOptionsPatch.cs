using System.IO;
using System.Linq;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Threading;
using UnhollowerBaseLib;
using UnityEngine;
using Math = System.Math;
using Object = UnityEngine.Object;

namespace PluginExperiments
{
    class GameOptionsPatch
    {
        
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        public static class GameOptionsMenuPatch
        {
            public static void Postfix(GameOptionsMenu __instance)
            {
                foreach (var child in __instance.Children)
                {
                    if (child.name == "CrewmateVision" || child.name == "ImpostorVision")
                    {
                        var option = child.GetComponent<NumberOption>();
                        option.Increment = 0.1f;
                        option.ValidRange.min = 0f;
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
        public static class NumberOptionIncrease
        {
            public static void Postfix(NumberOption __instance)
            {
                __instance.Value = (float)Math.Round(__instance.Value, 2);
            }
        }
        
        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
        public static class NumberOptionDecrease
        {
            public static void Postfix(NumberOption __instance)
            {
                __instance.Value = (float)Math.Round(__instance.Value, 2);
            }
        }
    }
}