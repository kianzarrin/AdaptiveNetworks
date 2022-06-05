namespace AdaptiveRoads.Util {
    using ColossalFramework;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Helpers;

    internal static class RoadUtils {
        public static List<string> GatherEditNames() {
            var ground = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
            if (ground == null)
                throw new Exception("m_editPrefabInfo is not netInfo");
            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

            var ret = GatherNetInfoNames(ground);
            ret.AddRange(GatherNetInfoNames(elevated));
            ret.AddRange(GatherNetInfoNames(bridge));
            ret.AddRange(GatherNetInfoNames(slope));
            ret.AddRange(GatherNetInfoNames(tunnel));

            return ret;
        }

        public static List<string> GatherNetInfoNames(NetInfo info) {
            var ret = new List<string>();
            if (!info) return ret;
            ret.Add(info.name);
            var ai = info.m_netAI;
            foreach (FieldInfo field in ai.GetType().GetFields()) {
                if (field.GetValue(ai) is BuildingInfo buildingInfo) {
                    ret.Add(buildingInfo.name);
                }
            }
            return ret;
        }

        public static List<string> RenameEditNet(string name, bool reportOnly) {
            try {
                if (name.IsNullOrWhiteSpace())
                    throw new Exception("name is empty");
                var ground = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
                if (ground == null)
                    throw new Exception("m_editPrefabInfo is not netInfo");

                NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
                NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
                NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
                NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

                var ret = new List<string>();
                void Rename(NetInfo _info, string _postfix) {
                    if (!_info) return;
                    ret.Add(GetUniqueNetInfoName(name + _postfix, true));
                    if (!reportOnly) _info.name = ret.Last();
                    ret.AddRange(_info.NameAIBuildings(ret.Last(), reportOnly));
                }

                Rename(ground, "_Data");
                Rename(elevated, " E");
                Rename(bridge, " B");
                Rename(slope, " S");
                Rename(tunnel, " T");

                return ret;
            } catch (Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        /// <summary>
        /// generates unique name by adding prefix.
        /// </summary>
        /// <param name="excludeCurrent">
        /// set to true when renaming
        /// set to false when cloning.
        /// </param>
        public static string GetUniqueNetInfoName(string name, bool excludeCurrent = false) {
            string strippedName = PackageHelper.StripName(name);
            if (excludeCurrent && strippedName == name)
                return name;
            string finalName = strippedName;
            for (int i = 0; PrefabCollection<NetInfo>.LoadedExists(finalName); i++) {
                finalName = $"instance{i}." + strippedName;
                if (i > 1000) throw new Exception("Infinite loop");
            }
            return finalName;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0101:Array allocation for params parameter", Justification = "<Pending>")]
        public static List<string> NameAIBuildings(this NetInfo info, string infoName, bool reportOnly) {
            var ret = new List<string>();
            var ai = info.m_netAI;
            string name = PackageHelper.StripName(infoName);
            foreach (FieldInfo field in ai.GetType().GetFields()) {
                if (field.GetValue(ai) is BuildingInfo buildingInfo) {
                    string postfix = field.Name.Remove("m_", "Info");
                    ret.Add(name + "_" + postfix);
                    if (!reportOnly) {
                        buildingInfo.name = ret.Last();
                        Log.Debug($"set {info}.netAI.{field.Name}={buildingInfo.name}");
                    }
                }
            }
            return ret;
        }

        public static void SetDirection(bool lht) {
            SimulationManager.instance.AddAction(() => SetDirectionImpl(lht));
        }
        public static void SetDirectionImpl(bool lht) {
            Log.Called();
            if(lht == NetUtil.LHT) return; // no need for change.
            SimulationManager.instance.m_metaData.m_invertTraffic =
                lht ? SimulationMetaData.MetaBool.True: SimulationMetaData.MetaBool.False;
            
            for(ushort i=0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                var info = PrefabCollection<NetInfo>.GetLoaded(i);
                if(!info) continue;
                foreach(var lane in info.m_lanes) {
                    const NetInfo.LaneType flags = NetInfo.LaneType.Vehicle | NetInfo.LaneType.Parking | NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.TransportVehicle;
                    if(lht && lane.m_laneType.IsFlagSet(flags)) {
                        lane.m_finalDirection = NetInfo.InvertDirection(lane.m_direction);
                    } else {
                        lane.m_finalDirection = lane.m_direction;
                    }
                }
                Swap(ref info.m_hasForwardVehicleLanes, ref info.m_hasBackwardVehicleLanes);
                Swap(ref info.m_forwardVehicleLaneCount, ref info.m_backwardVehicleLaneCount);
            }

            SimulationManager.instance.m_ThreadingWrapper.QueueMainThread(RoadEditorUtils.RefreshRoadEditor);

            for(ushort segmentID = 1; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                if(NetUtil.IsSegmentValid(segmentID)) {
                    NetManager.instance.UpdateSegment(segmentID);
                }
            }
        }

        public static void SetTiling(this Material material, float tiling) {
            if (material) {
                material.mainTextureScale = new Vector2(1, tiling);
                // not sure if checksum changes if I change texture scale.to make sure checksum changes I also change the name.
                material.name = "NetworkTiling " + tiling.ToString("R");
            }
        }
    }
}
