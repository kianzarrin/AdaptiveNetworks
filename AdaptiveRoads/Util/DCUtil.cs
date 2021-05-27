namespace AdaptiveRoads.Util {
    using System;
    using System.Runtime.CompilerServices;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KianCommons;
    using System.Diagnostics;
    using KianCommons;
    using HarmonyLib;

    // empty array must be null
    internal static class DCUtil {
        [Conditional("Debug")]
        static void AssertNotEmpty(int[] ar, string name) => Assertion.AssertDebug(ar != null && ar.Length == 0, $"{name} must be null if empty");

        internal static bool ConnectGroupsMatch(int []source, NetInfo.ConnectGroup cgSource, int []target, NetInfo.ConnectGroup cgTarget) {
            AssertNotEmpty(source,"source");
            AssertNotEmpty(target, "target");
            if (source == null || cgSource == 0) return true;
            if (target == null || cgTarget == 0) return false;
            foreach(var s in source) {
                foreach(var t in target) {
                    if (t == s)
                        return true;
                }
            }
            return false;
        }

        internal static bool ConnectGroupsMatchStrict(int[] group1, int[] group2) {
            AssertNotEmpty(group1, "group1");
            AssertNotEmpty(group2, "group2");
            if (group1 == null) return false;
            if (group2 == null) return false;
            foreach (var g1 in group1) {
                foreach (var g2 in group2) {
                    if (g1 == g2)
                        return true;
                }
            }
            return false;
        }

        internal static string[] AddToConnectGroup(string[] connectGroup, string item){
            var newAr = new string[] { item };
            if (connectGroup == null)
                return newAr;
            else
                return connectGroup.Concat(newAr).ToArray();
        }
        internal static string[] RemoveFromConnectGroup(string[] connectGroup, string item) {
            var ret = connectGroup?.Where(item0 => item0 != item);
            if (ret.IsNullorEmpty())
                return null;
            return ret.ToArray();
        }


    }
}
