namespace AdaptiveRoads.Patches.NetworkDetective {
    using HarmonyLib;
    using KianCommons;
    using System.Reflection;
    using AdaptiveRoads.Manager;
    using System;

    [HarmonyPatch]
    [InGamePatch]
    static class GetDetails {
        static MethodBase TargetMethod() {
            var asm = PluginUtil.GetNetworkDetective().GetMainAssembly();
            var t =
                asm.GetType("NetworkDetective.UI.ControlPanel.InterAvtiveButton", throwOnError: false) ??
                asm.GetType("NetworkDetective.UI.ControlPanel.InterActiveButton", throwOnError: true); // just in case I fixed the typo
            return t.GetMethod("GetDetails");
        }

        static bool Prepare() {
            return PluginUtil.GetNetworkDetective().IsActive();
        }
        static NetworkExtensionManager man => NetworkExtensionManager.Instance;
        // private void LaneArrowManager.OnLaneChange(uint laneId) {
        static void Postfix(ref string __result,
            InstanceID ____instanceID) {
            {
                if (string.IsNullOrEmpty(__result)) return;

                string flags = ____instanceID.Type switch {
                    InstanceType.NetLane =>
                        man.LaneBuffer[____instanceID.NetLane].m_flags.ToSTR(),
                    InstanceType.NetSegment =>
#pragma warning disable
                        man.SegmentBuffer[____instanceID.NetSegment].m_flags
                        + "\nStart: " + man.GetSegmentEnd(____instanceID.NetSegment, true).m_flags
                        + "\nEnd: " + man.GetSegmentEnd(____instanceID.NetSegment, false).m_flags,
#pragma warning restore
                    InstanceType.NetNode =>
                        man.NodeBuffer[____instanceID.NetNode].m_flags.ToSTR(),
                    _ => "",
                };
                if (string.IsNullOrEmpty(flags)) return;

                var res = __result.SplitLines();
                res[0] += ", " + flags;
                __result = res.JoinLines();
            }
        }
    }
}
