namespace AdaptiveRoads.Util {
    using ColossalFramework;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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

                Rename(ground, " G_Data");
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
    }
}
