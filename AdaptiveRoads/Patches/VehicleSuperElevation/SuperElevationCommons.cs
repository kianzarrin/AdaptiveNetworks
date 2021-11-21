namespace AdaptiveRoads.Patches.VehicleSuperElevation {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using KianCommons;
    using KianCommons.Plugins;
    using System;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Patches.TranspilerUtils;

    public static class SuperElevationCommons {
        delegate void SimulationStepDelegate(
            ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData,
            ushort leaderID, ref Vehicle leaderData, int lodPhysics);

        public static MethodBase TargetMethod<T>() =>
            DeclaredMethod<SimulationStepDelegate>(typeof(T), "SimulationStep");

        public static MethodBase TargetTMPEMethod<T>() {
            string typeName = "TrafficManager.Custom.AI.Custom" + typeof(T);
            Type customType =
                PluginUtil.GetTrafficManager().GetMainAssembly()
                .GetType(typeName, throwOnError: false);

            if(customType != null)
                return DeclaredMethod<SimulationStepDelegate>(customType, "CustomSimulationStep");
            return null;
        }

        static PathUnit[] pathUnitBuffer => Singleton<PathManager>.instance.m_pathUnits.m_buffer;

        static string ToSTR(this ref PathUnit.Position pathPos) {
            var info = pathPos.m_segment.ToSegment().Info;
            return
                $"segment:{pathPos.m_segment} " +
                $"info:{info} " +
                $"nLanes={info.m_lanes.Length} " +
                $"laneIndex={pathPos.m_lane} ";
        }

        public static void Postfix(ref Vehicle vehicleData, ref Vehicle.Frame frameData) {
            if(!vehicleData.GetCurrentPathPos(out var pathPos))
                return;
            try {
                float se = GetCurrentSE(pathPos, vehicleData.m_lastPathOffset * (1f / 255f), ref vehicleData);

                var rot = Quaternion.Euler(0, 0f, se);
                frameData.m_rotation *= rot;
            } catch(Exception ex) {
                Log.Exception(ex, pathPos.ToSTR(), showInPanel: false);
            }
        }

        internal static bool GetCurrentPathPos(this ref Vehicle vehicleData, out PathUnit.Position pathPos) {
            byte pathIndex = vehicleData.m_pathPositionIndex;
            if(pathIndex == 255) pathIndex = 0;
            return pathUnitBuffer[vehicleData.m_path].GetPosition(pathIndex >> 1, out pathPos);
        }

        internal static NetInfo.Lane GetLaneInfo(this ref PathUnit.Position pathPos) =>
            pathPos.m_segment.ToSegment().Info.m_lanes[pathPos.m_lane];


        internal static float GetCurrentSE(PathUnit.Position pathPos, float offset, ref Vehicle vehicleData) {
            if(float.IsNaN(offset) || float.IsInfinity(offset)) return 0;
            // bezier is always from start to end node regardless of direction.
            ref var segExt = ref pathPos.m_segment.ToSegmentExt();
            float startSE = segExt.Start.DeltaAngle;
            float endSE = -segExt.End.DeltaAngle;
            float se = startSE * (1 - offset) + endSE * offset;

            var lane = pathPos.GetLaneInfo();
            if(lane is null) return 0;
            bool invert = pathPos.m_segment.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
            bool backward = lane.m_finalDirection == NetInfo.Direction.Backward;
            bool reversed = vehicleData.m_flags.IsFlagSet(Vehicle.Flags.Reversed);

            bool bidirectional = lane.m_finalDirection == NetInfo.Direction.Both;
            bool avoidForward = lane.m_finalDirection == NetInfo.Direction.AvoidForward;
            bool avoidBackward = lane.m_finalDirection == NetInfo.Direction.AvoidBackward;
            bool avoid = avoidForward | avoidBackward;

            if(invert) se = -se;
            if(backward) se = -se;
            if(reversed & !avoid) se = -se;

            //if (bidirectional) se = 0; // is this necessary?
            return se * Mathf.Rad2Deg;
        }

    }
}
