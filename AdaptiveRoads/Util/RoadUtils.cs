namespace AdaptiveRoads.Util {
    using KianCommons;
    using System;
    using System.Linq;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;
    using ColossalFramework;

    internal static class RoadUtils {
        public static void RenameEditNet(string name) {
            if (name.IsNullOrWhiteSpace())
                throw new Exception("name is empty");
            var ground = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
            if (ground == null)
                throw new Exception("m_editPrefabInfo is not netInfo");

            NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(ground);
            NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(ground);
            NetInfo slope = AssetEditorRoadUtils.TryGetSlope(ground);
            NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

            ground.name = GetUniqueNetInfoName(name + " G");
            if (elevated) elevated.name = GetUniqueNetInfoName(name + " E");
            if (bridge) bridge.name = GetUniqueNetInfoName(name + " B");
            if (slope) slope.name = GetUniqueNetInfoName(name + " S");
            if (tunnel) tunnel.name = GetUniqueNetInfoName(name + " T");

            ground?.NameAIBuildings();
            elevated?.NameAIBuildings();
            bridge?.NameAIBuildings();
            slope?.NameAIBuildings();
            tunnel?.NameAIBuildings();
        }

        public static string GetUniqueNetInfoName(string name) {
            name = PackageHelper.StripName(name);
            string name2 = name;
            for (int i = 0; PrefabCollection<NetInfo>.LoadedExists(name2); i++) {
                name2 = $"instance{i}." + name;
                if (i > 1000) throw new Exception("Infinite loop");
            }
            return name2;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0101:Array allocation for params parameter", Justification = "<Pending>")]
        public static void NameAIBuildings(this NetInfo info) {
            var ai = info.m_netAI;
            string name = PackageHelper.StripName(info.name);
            foreach (FieldInfo field in ai.GetType().GetFields()) {
                if (field.GetType() != typeof(BuildingInfo))
                    continue;
                string postfix = field.Name.Remove("m_", "Info");
                BuildingInfo buildingInfo = field.GetValue(ai) as BuildingInfo;
                buildingInfo.name = name + "_" + postfix;
            }
        }
    }
}
