using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using static KianCommons.ReflectionHelpers;

namespace AdaptiveRoads.Patches.QuayRoads {
    [HarmonyPatch]
    [PreloadPatch]
    class TerrainUpdatedPatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(NetSegment), "TerrainUpdated");
            yield return GetMethod(typeof(NetNode), "TerrainUpdated");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach(var instruction in instructions) {
                // remove the bounds on right and left
                if (instruction.Calls(typeof(Vector3).GetMethod("Lerp"))){
                    yield return CodeInstruction.Call(typeof(Vector3), "LerpUnclamped");
                    continue;
                }

                yield return instruction;
            }
        }
    }
}
