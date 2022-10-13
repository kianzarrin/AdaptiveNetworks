namespace AdaptiveRoads.Patches.Lane {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using KianCommons.Patches;
    using KianCommons;
    using ColossalFramework.Math;
    using System;
    using AdaptiveRoads.Manager;

    internal static class SeedIndexCommons {
        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            MethodBase NewRandomizer = AccessTools.Constructor(
                typeof(Randomizer),
                new[] { typeof(int) } )
                ?? throw new NullReferenceException("NewRandomizer");
            MethodInfo mGetSpeed = typeof(SeedIndexCommons).GetMethod(nameof(GetSeed), throwOnError: true);

            int iLdProp = codes.Search(_c => _c.IsLdLoc(typeof(NetLaneProps.Prop), method));
            for (int occurance = 1; occurance<=2; occurance++) {
                int iNewRandomizer = codes.Search(_c => _c.Calls(NewRandomizer), count: occurance);
                codes.InsertInstructions(iNewRandomizer,
                    new[] {
                    // seed0 already on the stack
                    TranspilerUtils.GetLDArg(method, "laneID"),
                    codes[iLdProp].Clone(),
                    new CodeInstruction(OpCodes.Call, mGetSpeed),
                    });
            }
        }

        public static int GetSeed(int seed0, uint laneId, NetLaneProps.Prop prop) {
            if(prop.GetMetaData() is var metadata && metadata.SeedIndex != 0) {
                return unchecked((int)laneId + (metadata.SeedIndex -1));
            } else {
                return seed0;
            }
        }
    }
}
