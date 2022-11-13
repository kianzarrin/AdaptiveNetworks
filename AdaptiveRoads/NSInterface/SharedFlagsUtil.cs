namespace AdaptiveRoads.NSInterface {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using Epic.OnlineServices.Presence;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    internal static class SharedFlagsUtil {
        public static CustomFlags GatherAllFlags(this NetInfo baseInfo) =>
                baseInfo.AllElevations()
                .Select(info => info?.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None)
                .Or();

        public static CustomFlags GatherSharedCustomFlags(this NetInfo baseInfo) {
            CustomFlags shared = default;
            foreach (Enum flag in baseInfo.GatherAllFlags()) {
                if (flag is NetLaneExt.Flags laneFlag) {
                    for (int laneIndex = 0; laneIndex < baseInfo.m_lanes.Length; ++laneIndex) {
                        string name = baseInfo.GetSharedName(laneFlag, laneIndex);
                        if (name != null) {
                            shared |= laneFlag;
                            break;
                        }
                    }
                } else {
                    string name = baseInfo.GetSharedName(flag);
                    if (name != null) {
                        shared |= flag;
                    }
                }
            }

            return shared;
        }

        public static ARCustomFlags GatherSharedARCustomFlags(this NetInfo baseInfo) {
            CustomFlags all = baseInfo.AllElevations()
                .Select(info => info?.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None)
                .Or();

            ARCustomFlags shared = new ARCustomFlags(baseInfo.m_lanes.Length);
            foreach (Enum flag in all) {
                if (flag is NetLaneExt.Flags laneFlag) {
                    for (int laneIndex = 0; laneIndex < baseInfo.m_lanes.Length; ++laneIndex) {
                            string name = baseInfo.GetSharedName(laneFlag, laneIndex);
                        if (name != null) {
                            shared.Lanes[laneIndex] |= laneFlag;
                            break;
                        }
                    }
                } else {
                    string name = baseInfo.GetSharedName(flag);
                    if (name != null) {
                        shared.AddFlag(flag);
                    }
                }
            }

            return shared;
        }

        public static ARCustomFlags GatherARCustomFlags(this NetInfo info) {
            CustomFlags all = info?.GetMetaData()?.UsedCustomFlags ?? CustomFlags.None;

            ARCustomFlags ret = new ARCustomFlags(info.m_lanes.Length);
            foreach (Enum flag in all) {
                if (flag is NetLaneExt.Flags laneFlag) {
                    for (int laneIndex = 0; laneIndex < info.m_lanes.Length; ++laneIndex) {
                        string name = info.GetMetaData().CustomLaneFlagNames[laneIndex]?.GetorDefault(laneFlag);
                        if (name != null) {
                            ret.Lanes[laneIndex] |= laneFlag;
                            break;
                        }
                    }
                } else {
                    string name = CustomFlagAttribute.GetName(flag, info);
                    if (name != null) {
                        ret.AddFlag(flag);
                    }
                }
            }

            return ret;
        }

        public static string GetSharedName(this NetInfo baseInfo, Enum flag) {
            string name = null;
            foreach (NetInfo info in baseInfo.AllElevations()) {
                if (info?.GetCustomFlagName(flag) is string name2) {
                    if(name == null) {
                        name = name2;
                    } else if (!SameName(name, name2)) {
                        return null; // inconsistent
                    }
                }
            }
            return name;
        }

        public static string GetSharedName(this NetInfo baseInfo, NetLaneExt.Flags flag, int laneIndex) {
            string name = null;
            foreach (NetInfo info in baseInfo.AllElevations()) {
                if (info?.GetCustomFlagName(flag, laneIndex) is string name2) {
                    if (name == null) {
                        name = name2;
                    } else if (!SameName(name, name2)) {
                        return null; // inconsistent
                    }
                }
            }
            return name;
        }

        static bool SameName(string a, string b) => a.Remove(" ") == b.Remove(" ");
        
        public static string GetMergedName(this NetInfo baseInfo, Enum flag) {
            var names = new HashSet<string>();
            foreach (NetInfo info in baseInfo.AllElevations()) {
                if (info?.GetCustomFlagName(flag) is string name2) {
                    names.Add(name2);
                }
            }
            return names.Join(" | ");
        }

        public static string GetMergedName(this NetInfo baseInfo, NetLaneExt.Flags flag, int laneIndex) {
            var names = new HashSet<string>();
            foreach (NetInfo info in baseInfo.AllElevations()) {
                if (info?.GetCustomFlagName(flag, laneIndex) is string name2) {
                    names.Add(name2);
                }
            }
            return names.Join(" | ");
        }

        public static string GetCustomFlagName(this NetInfo netInfo, Enum flag) {
            string name = CustomFlagAttribute.GetName(flag, netInfo);
            if (!name.IsNullOrWhiteSpace())
                return name;
            else
                return null;
        }

        public static string GetCustomFlagName(this NetInfo info, NetLaneExt.Flags flag, int laneIndex) {
            string name = info.
                GetMetaData()?.
                CustomLaneFlagNames?.
                GetOrDefault(laneIndex)?.
                GetorDefault(flag);
            if (!name.IsNullOrWhiteSpace())
                return name;
            else return null;
        }

        static T GetOrDefault<T>(this T[] ar, int index) {
            if (ar.Length > index && index >=0)
                return ar[index];
            else 
                return default;
        }

        static void TrySet<T>(this T[] ar, int index, T value) {
            Assertion.GTEq(index, 0, "index:" + index);
            if (ar.Length > index && index >= 0)
                ar[index] = value;
        }
    }
}
