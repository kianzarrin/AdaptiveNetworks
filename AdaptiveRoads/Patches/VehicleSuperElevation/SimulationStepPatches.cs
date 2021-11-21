namespace AdaptiveRoads.Patches.VehicleSuperElevation {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection;
    using static SuperElevationCommons;


    [HarmonyPatch]
    static class TrainAI_SimulationStepPatch {
        internal static IEnumerable<MethodBase> TargetMethods() {
            yield return TargetMethod<TrainAI>();
            var tmpeTarget = TargetTMPEMethod<TrainAI>();
            if(tmpeTarget != null)
                yield return tmpeTarget; //old TMPE
        }

        internal static void Postfix(ref Vehicle vehicleData, ref Vehicle.Frame frameData) =>
            SuperElevationCommons.Postfix(ref vehicleData, ref frameData);
    }

}

