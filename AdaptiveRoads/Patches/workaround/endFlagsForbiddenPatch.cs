/*
// NetLaneProps.Prop
public NetNode.FlagsLong endFlagsForbidden
{
    get => NetNode.GetFlags(m_startFlagsRequired, m_endFlagsForbidden2); // m_startFlagsRequired is wrong
    set => NetNode.GetFlagParts(value, out m_startFlagsRequired, out m_endFlagsForbidden2); // m_startFlagsRequired is wrong.
}
 */

namespace AdaptiveRoads.Patches.workaround {
    using HarmonyLib;
    [HarmonyPatch(typeof(NetLaneProps.Prop), "get_endFlagsForbidden")]
    internal static class endFlagsForbiddenGetPatch {
        static bool Prefix(NetLaneProps.Prop __instance, ref NetNode.FlagsLong __result) {
            __result = NetNode.GetFlags(__instance.m_endFlagsForbidden, __instance.m_endFlagsForbidden2);
            return false; // replace original code
        }
    }

    [HarmonyPatch(typeof(NetLaneProps.Prop), "set_endFlagsForbidden")]
    internal static class endFlagsForbiddenSetPatch {
        static bool Prefix(NetLaneProps.Prop __instance, NetNode.FlagsLong value) {
            NetNode.GetFlagParts(value, out __instance.m_endFlagsForbidden, out __instance.m_endFlagsForbidden2);
            return false; // replace original code
        }

    }

}
