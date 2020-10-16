using HarmonyLib;
using KianCommons;
using System;
using System.Reflection;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.Patches{
    public static class HideCrosswalksPatch {
        internal static bool patched = false;
        public static MethodBase TargetMethod() {
            return PluginUtil.GetHideCrossings().GetMainAssembly()
                .GetType("HideCrosswalks.Utils.RoadUtils", throwOnError:true)
                .GetMethod("CalculateCanHideCrossingsRaw") ?? throw new Exception("CalculateCanHideCrossingsRaw() not found");
        }
        public static bool Prefix(NetInfo info, ref bool __result) {
            //Log.Debug("IsNormalSymetricalTwoWay.Prefix() was called for info:" + info);
            if (info.IsAdaptive()) {
                __result = false;
                return false;
            }
            return true;
        }
    }

}
