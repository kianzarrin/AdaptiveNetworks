namespace AdvancedRoads.Patches.RoadEditor {
    using AdvancedRoads.Manager;
    using HarmonyLib;
    using KianCommons;
    using PrefabIndeces;
    using System;
    using System.Reflection;
    using static KianCommons.Assertion;
    using static KianCommons.EnumerationExtensions;

    // AssetEditorRoadUtils
    //public static object AddArrayElement(object target, FieldInfo field, object newObject = null)
    //	public static void RemoveArrayElement(object element, object target, FieldInfo field)
    [HarmonyPatch(typeof(AssetEditorRoadUtils))]
    public static class AssetEditorRoadUtilsPatch {
        [HarmonyPostfix]
        [HarmonyPatch("AddArrayElement")]
        static void AddArrayElement(object target, FieldInfo field, ref object __result) {
            try {
                Log.Debug($"AddArrayElement({target}, {field}, {__result}\n" + Environment.StackTrace);
                NetInfoExt.ReExtendEditedPrefabIndeces();

                int found = 0;
                object array = field.GetValue(target);

                // brute force search all arrays to find teh one that is the given our target.
                if (target is NetInfo info) {
                    var InfoExt = info.GetExt();
                    ushort prefabIndex = info.GetIndex();

                    if ((array == info.m_nodes)) {
                        found++;
                        Assert(info.m_nodes.Length == InfoExt.NodeInfoExts.Length + 1, "new nodes == old nodes + 1");
                        __result = info.m_nodes.Last();
                        Assert(__result is NetInfoExtension.Node);
                        var item = new NetInfoExt.Node(info.m_nodes.Last());
                        AppendElement(ref InfoExt.NodeInfoExts, item);

                    }
                    if ((array == info.m_segments)) {
                        found++;
                        Assert(info.m_segments.Length == InfoExt.SegmentInfoExts.Length + 1, "new segments == old segments + 1");
                        __result = info.m_segments.Last();
                        var item = new NetInfoExt.Segment(info.m_segments.Last());
                        AppendElement(ref InfoExt.SegmentInfoExts, item);
                    }
                    if ((array == info.m_lanes)) {
                        found++;
                        Assert(info.m_lanes.Length == InfoExt.LaneInfoExts.Length + 1, "new lanes == old lanes + 1");
                        __result = info.m_lanes.Last();
                        var item = new NetInfoExt.Lane(info.m_lanes.Last());
                        AppendElement(ref InfoExt.LaneInfoExts, item);
                    }
                } else if (array is NetLaneProps.Prop[]) {
                    foreach (NetInfo info2 in NetInfoExt.EditNetInfos) {
                        var InfoExt = info2.GetExt();
                        for (int laneIndex = 0; laneIndex < info2.m_lanes.Length; ++laneIndex) {
                            var props = info2.m_lanes[laneIndex].m_laneProps.m_props;
                            var laneExt = InfoExt.LaneInfoExts[laneIndex];
                            if ((array == props)) {
                                found++;
                                Assert(props.Length == laneExt.PropInfoExts.Length + 1, "new lane props == old lane props + 1");
                                __result = props.Last();
                                var item = new NetInfoExt.LaneProp(props.Last());
                                AppendElement(ref laneExt.PropInfoExts, item);
                            }
                        }
                    }
                }
                Assert(found <= 1, $"{found} <= 1 | only 1 element is expected to be added at a time | target={target}");
                Log.Debug($"AddArrayElement -> return={__result}");
            }
            catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("RemoveArrayElement")]
        static void RemoveArrayElement(object element) {
            try {
                if (element is NetInfoExtension.Node node) {
                    DropElement(
                        ref NetInfoExt.Buffer[node.PrefabIndex].NodeInfoExts,
                        node.Index);
                } else if (element is NetInfoExtension.Segment segment) {
                    DropElement(
                        ref NetInfoExt.Buffer[segment.PrefabIndex].SegmentInfoExts,
                        segment.Index);
                } else if (element is NetInfoExtension.Lane lane) {
                    DropElement(
                        ref NetInfoExt.Buffer[lane.PrefabIndex].LaneInfoExts,
                        lane.Index);
                } else if (element is NetInfoExtension.Lane.Prop prop) {
                    DropElement(
                        ref NetInfoExt.Buffer[prop.PrefabIndex].LaneInfoExts[prop.LaneIndex].PropInfoExts,
                        prop.Index);
                } else {
                    throw new Exception($"Could not find the element to drop: {element}");
                }
                NetInfoExt.ReExtendEditedPrefabIndeces();
            }
            catch (Exception e) {
                Log.Exception(e);
                throw e;
            }
        }
    }
}
