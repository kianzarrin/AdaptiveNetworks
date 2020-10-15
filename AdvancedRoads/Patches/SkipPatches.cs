using HarmonyLib;
using KianCommons;
using System;
using System.Reflection;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.Patches{
    public static class HideCrosswalksPatch {
        internal static bool patched = false;
        public static MethodBase TargetMethod() {
            return PluginUtil.GetHideCrossings().MainAssembly()
                .GetType("HideCrosswalks.Utils.RoadUtils")
                .GetMethod("CalculateCanHideCrossingsRaw");
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
