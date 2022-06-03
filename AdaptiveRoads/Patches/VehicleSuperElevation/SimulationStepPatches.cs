namespace AdaptiveRoads.Patches.VehicleSuperElevation {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection;
    using static SuperElevationCommons;


    [HarmonyPatch]
    [InGamePatch]
    static class TrainAI_SimulationStepPatch {
        internal static IEnumerable<MethodBase> TargetMethods() {
            yield return TargetMethod<TrainAI>();
        }

        internal static void Postfix(ref Vehicle vehicleData, ref Vehicle.Frame frameData) =>
            SuperElevationCommons.Postfix(ref vehicleData, ref frameData);
    }

}

