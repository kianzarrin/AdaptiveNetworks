namespace AdaptiveRoads.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KianCommons;
    using AdaptiveRoads.Manager;
    public static class RoadFamilyUtil {
        // pass around netInfos to make sure we limit ourselves to filtered netinfos.
        public static List<NetInfo>[] BuildFamilies(IEnumerable<NetInfo> netInfos = null) {
            HashSet<NetInfo> scanned = new();
            List<List<NetInfo>> groups = new();
            netInfos ??= NetUtil.IterateLoadedNetInfos();
            foreach (NetInfo info in netInfos) {
                if (scanned.Contains(info)) continue;
                var family = info.GetRelatives(netInfos).ToList();
                SortByElevation(family);
                groups.Add(family);
                scanned.AddRange(family);
            }
            return groups.ToArray();
        }

        public static void SortByElevation(List<NetInfo> family) {
            Dictionary<NetInfo, int> levels = new(family.Count);
            foreach (var info in family) {
                if (info == null) continue;

                NetInfo elevated = AssetEditorRoadUtils.TryGetElevated(info);
                if (elevated != null && !levels.ContainsKey(elevated)) {
                    levels[elevated] = 1;
                }

                NetInfo bridge = AssetEditorRoadUtils.TryGetBridge(info);
                if (bridge != null && !levels.ContainsKey(bridge)) {
                    levels[bridge] = 2;
                }

                NetInfo slope = AssetEditorRoadUtils.TryGetSlope(info);
                if (slope != null && !levels.ContainsKey(slope)) {
                    levels[slope] = 3;
                }

                NetInfo tunnel = AssetEditorRoadUtils.TryGetTunnel(info);
                if (tunnel != null && !levels.ContainsKey(tunnel)) {
                    levels[tunnel] = 4;
                }
            }

            // add basic elevations:
            foreach (var info in family) {
                if (!levels.ContainsKey(info)) {
                    levels.Add(info, 0);
                }
            }

            family.Sort((a, b) => levels[a].CompareTo(levels[b]));
        }


        /// <summary>
        /// returns a list of all elevations that are directly or indirectly linked to <c>currentInfo</c>.
        /// sometimes one elevation belongs to multiple basic elevations.
        /// the return family will contain all of them.
        /// </summary>
        public static HashSet<NetInfo> GetRelatives(this NetInfo netinfo, IEnumerable<NetInfo> netInfos) {
            try {
                if (netinfo == null) return null;

                HashSet<NetInfo> family = new HashSet<NetInfo>(netinfo.GetDirectlyRelated(netInfos));

                // get elevations that are indirectly linked to currentInfo.
                foreach (var netinfo2 in family.ToArray()) {
                    family.AddRange(netinfo2.GetDirectlyRelated(netInfos));
                }

                return family;
            } catch (Exception ex) {
                ex.Log();
                return new HashSet<NetInfo>( new[] { netinfo });
            }
        }

        /// <summary>
        /// returns a list containing <c>currentInfo</c>, base elevation, and all its other elevations.
        /// </summary>
        private static IEnumerable<NetInfo> GetDirectlyRelated(this NetInfo netinfo, IEnumerable<NetInfo> netInfos) {
            foreach (var netinfo2 in netInfos) {
                var elevations = netinfo2.AllElevations();
                if (elevations.Contains(netinfo)) {
                    foreach (var elevation in elevations) {
                        yield return elevation;
                    }
                }
            }
        }
        public static NetInfo GetBasicElevation(NetInfo info) {
            if (info == null) return null;
            foreach (NetInfo info2 in NetUtil.IterateLoadedNetInfos()) {
                if (info2 == AssetEditorRoadUtils.TryGetElevated(info))
                    return info2;
                if (info2 == AssetEditorRoadUtils.TryGetBridge(info))
                    return info2;
                if (info2 == AssetEditorRoadUtils.TryGetSlope(info))
                    return info2;
                if (info2 == AssetEditorRoadUtils.TryGetTunnel(info))
                    return info2;
            }
            return null;
        }
    }
}
