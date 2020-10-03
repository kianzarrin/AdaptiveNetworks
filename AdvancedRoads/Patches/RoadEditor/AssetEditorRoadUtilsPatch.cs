namespace AdvancedRoads.Patches.RoadEditor {
    using AdvancedRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using PrefabIndeces;
    using System.Reflection;
    using static KianCommons.Assertion;

    // AssetEditorRoadUtils
    //public static object AddArrayElement(object target, FieldInfo field, object newObject = null)
    //	public static void RemoveArrayElement(object element, object target, FieldInfo field)
    [HarmonyPatch(typeof(AssetEditorRoadUtils))]
    public static class AssetEditorRoadUtilsPatch {
        [HarmonyPostfix]
        [HarmonyPatch("AddArrayElement")]
        static void AddArrayElement_Postfix(object target, FieldInfo field, object newObject) {
            int found = 0;

            foreach (NetInfo info in NetInfoExt.EditNetInfos) {
                var InfoExt = info.GetExt();

                if (target == info.m_nodes) {
                    found++;
                    Assert(info.m_nodes.Length == InfoExt.NodeInfoExts.Length + 1, "new nodes == old nodes + 1");
                    var item = new NetInfoExt.Node(info.m_nodes.Last());
                    InfoExt.NodeInfoExts.AppendElement(item);
                }
                if (target == info.m_segments) {
                    found++;
                    Assert(info.m_segments.Length == InfoExt.SegmentInfoExts.Length + 1, "new segments == old segments + 1");
                    var item = new NetInfoExt.Segment(info.m_segments.Last());
                    InfoExt.SegmentInfoExts.AppendElement(item);
                }
                if (target == info.m_lanes) {
                    found++;
                    Assert(info.m_lanes.Length == InfoExt.LaneInfoExts.Length + 1, "new lanes == old lanes + 1");
                    var item = new NetInfoExt.Lane(info.m_lanes.Last());
                    InfoExt.LaneInfoExts.AppendElement(item);
                }
                for (int laneIndex = 0; laneIndex < info.m_lanes.Length; ++laneIndex) {
                    var props = info.m_lanes[laneIndex].m_laneProps.m_props;
                    var propsExt = InfoExt.LaneInfoExts[laneIndex].PropInfoExts;
                    if (target == props) {
                        found++;
                        Assert(props.Length == propsExt.Length + 1, "new lane props == old lane props + 1");
                        var item = new NetInfoExt.LaneProp(props.Last());
                        propsExt.AppendElement(item);
                    }
                }
            }
            Assert(found == 1, $"only 1 element is expected to be added at a time. got {found}");
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveArrayElement")]
        static void RemoveArrayElement_Prefix(object element, object target, FieldInfo field) {
            if (element is NetInfoExtension.Node node) {
                var array = NetInfoExt.Buffer[node.PrefabIndex].NodeInfoExts;
                int i = node.Index;
                array.DropElement(i);
            } else if (element is NetInfoExtension.Segment segment) {
                var array = NetInfoExt.Buffer[segment.PrefabIndex].SegmentInfoExts;
                int i = segment.Index;
                array.DropElement(i);
            } else if (element is NetInfoExtension.Lane lane) {
                var array = NetInfoExt.Buffer[lane.PrefabIndex].LaneInfoExts;
                int i = lane.Index;
                array.DropElement(i);
            } else if (element is NetInfoExtension.Lane.Prop prop) {
                var array = NetInfoExt.Buffer[prop.PrefabIndex].LaneInfoExts[prop.LaneIndex].PropInfoExts;
                int i = prop.Index;
                array.DropElement(i);
            }
        }
    }
}
