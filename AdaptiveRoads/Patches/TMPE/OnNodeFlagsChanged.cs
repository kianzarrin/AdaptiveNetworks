using HarmonyLib;
using KianCommons;
using TrafficManager.Manager.Impl;
using System;
using System.Collections.Generic;
using System.Reflection;
using AdaptiveRoads.Manager;
using System.Diagnostics;
using static KianCommons.ReflectionHelpers;

namespace AdaptiveRoads.Patches.TMPE {
    [InGamePatch]
    [HarmonyPatch]
    static class OnNodeFlagsChanged {
        static IEnumerable<MethodBase> TargetMethods() {
            /************* speed limit */
            // public bool SetTrafficLight(ushort nodeId, bool flag, ref NetNode node, out ToggleTrafficLightError reason)
            foreach(var m in AccessTools.GetDeclaredMethods(typeof(TrafficLightManager))) {
                if (m.Name == nameof(TrafficLightManager.SetTrafficLight) &&
                    m.GetParameters().Length == 4 
                    )
                    yield return m;
            }
        }

        static void Postfix(ushort nodeId) {
            Log.Debug($"{ThisMethod} was called for " +
                $"segment:{nodeId} " +
                $"caller:{new StackFrame(1)}");
            NetworkExtensionManager.Instance.UpdateNode(nodeId);
        }
    }
}