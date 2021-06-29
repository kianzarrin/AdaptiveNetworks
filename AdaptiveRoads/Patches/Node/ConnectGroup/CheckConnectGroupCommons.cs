namespace AdaptiveRoads.Patches.Node.ConnectGroup {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using ColossalFramework;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System;
    using KianCommons.Patches;
    using AdaptiveRoads.Util;
    using static KianCommons.ReflectionHelpers;

    /// <summary> insert after the clause:'node.m_ConnectGroup == None' </summary>
    internal static class CheckNodeConnectGroupNone {
        // returns !(ConnectGroup == None && MetaData.ConnectGroups == null)
        // the next instruction is brfalse which aumatically takes a not of the above phrase so at the end it will be
        // (ConnectGroup == None && MetaData.ConnectGroups == null) ||
        public static bool CheckConnectGroup(NetInfo.ConnectGroup cg, NetInfo.Node node) {
#if DEBUG
            DirectConnectUtil.AssertNotEmpty(node.GetMetaData()?.ConnectGroupsHash, "connect groups");
#endif
            return !(cg == 0 && node.GetMetaData()?.ConnectGroupsHash == null);
        }

        static FieldInfo fNodeConnectGroup => typeof(NetInfo.Node).GetField(nameof(NetInfo.Node.m_connectGroup));
        static FieldInfo fNetConnectGroup => typeof(NetInfo).GetField(nameof(NetInfo.m_connectGroup));
        static MethodInfo mCheckConnectGroup => typeof(CheckNodeConnectGroupNone).GetMethod(nameof(CheckConnectGroup), throwOnError: true);

        // returns the index after the clause
        public static int GetNext(int startIndex, List<CodeInstruction> codes) {
            for (int i = startIndex; i < codes.Count - 1; ++i) {
                bool b = codes[i].LoadsField(fNodeConnectGroup); // find 'Connectgroup == None'
                b = b && codes[i + 1].Branches(out _);
                if (b)
                    return i + 1; // after the clause
            }
            return -1;
        }

        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            //return;
            try {
                int count = 0;
                for (int index = GetNext(0, codes); index >= 0 && index < codes.Count; index = GetNext(index, codes)) {
                    var iLoadNode = codes.Search(c => c.IsLdLoc(typeof(NetInfo.Node), method), startIndex: index, count: -1);
                    codes.InsertInstructions(index, new[] {
                        codes[iLoadNode].Clone(),
                        new CodeInstruction(OpCodes.Call, mCheckConnectGroup),
                    });
                    count++;
                }
                Log.Info(ThisMethod + $" successfully patched {count} places in " + method);
            } catch (Exception ex) {
                ex.Log("failed to patch " + method);
            }
        }
    }

    /// <summary> insert after the clause:'node.m_connectGroup & info.m_connectGroup'</summary>
    internal static class CheckNodeConnectGroup {
        public static bool CheckConnectGroup(bool flagsMatch, NetInfo.Node node, NetInfo info) {
            if (flagsMatch)
                return true;
            return DirectConnectUtil.ConnectGroupsMatch(
                node.GetMetaData()?.ConnectGroupsHash,
                info.GetMetaData()?.ConnectGroupsHash);
        }

        static FieldInfo fNodeConnectGroup => typeof(NetInfo.Node).GetField(nameof(NetInfo.Node.m_connectGroup));
        static FieldInfo fNetConnectGroup => typeof(NetInfo).GetField(nameof(NetInfo.m_connectGroup));
        static MethodInfo mCheckConnectGroup => typeof(CheckNodeConnectGroup).GetMethod(nameof(CheckConnectGroup), throwOnError: true);

        public static int GetNext(int startIndex, List<CodeInstruction> codes) {
            for (int i = startIndex; i < codes.Count - 1; ++i) {
                //find the clause
                bool b = codes[i].LoadsField(fNodeConnectGroup);
                b = b && codes[i + 2].LoadsField(fNetConnectGroup);
                b = b && codes[i + 3].opcode == OpCodes.And;
                if (b)
                    return i + 4; // after the clause
            }
            return -1;//re
        }

        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            TranspilerUtils.PeekBefore = 20;
            TranspilerUtils.PeekAfter = 20;
            try {
                int count = 0;
                for (int index = GetNext(0, codes); index >= 0 && index < codes.Count; index = GetNext(index, codes)) {
                    var iLoadNode = codes.Search(c => c.IsLdLoc(typeof(NetInfo.Node), method), startIndex: index, count: -1);
                    var iLoadNetInfo = codes.Search(c => c.IsLdLoc(typeof(NetInfo), method), startIndex: index, count: -1);
                    codes.InsertInstructions(index, new[] {
                        codes[iLoadNode].Clone(),
                        codes[iLoadNetInfo].Clone(),
                        new CodeInstruction(OpCodes.Call, mCheckConnectGroup),
                    });
                    count++;
                }
                Log.Info(ThisMethod + $" successfully patched {count} places in " + method);
            } catch (Exception ex) {
                ex.Log("failed to patch " + method);
            }
        }
    }

    /// <summary>insert after the clause:'info.m_nodeConnectGroups == None'</summary>
    internal static class CheckNetConnectGroupNone {
        // returns ConnectGroup != None || MetaData.ConnectGroups != null
        // the next instruction is brfalse which aumatically takes a not of the above phrase so at the end it will be
        // (ConnectGroup == None && MetaData.ConnectGroups == null) ||
        public static bool CheckConnectGroup(NetInfo.ConnectGroup cg, NetInfo info) {
#if DEBUG
            DirectConnectUtil.AssertNotEmpty(info.GetMetaData()?.NodeConnectGroupsHash, "connect groups");
#endif
            return !(cg == 0 && info.GetMetaData()?.NodeConnectGroupsHash == null);
        }


        static FieldInfo fNetNodeConnectGroups => typeof(NetInfo).GetField(nameof(NetInfo.m_nodeConnectGroups));
        static FieldInfo fNetConnectGroup => typeof(NetInfo).GetField(nameof(NetInfo.m_connectGroup));
        static MethodInfo mCheckConnectGroup => typeof(CheckNetConnectGroupNone).GetMethod(nameof(CheckConnectGroup), throwOnError: true);

        public static int GetNext(int startIndex, List<CodeInstruction> codes) {
            // find the clause
            for (int i = startIndex; i < codes.Count - 1; ++i) {
                bool b = codes[i].LoadsField(fNetNodeConnectGroups); // find ' Connectgroup == None'
                b = b && codes[i + 1].Branches(out _);
                if (b)
                    return i + 1; // after the clause
            }
            return -1;
        }

        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            //return;
            try {
                int count = 0;
                for (int index = GetNext(0, codes); index >= 0 && index < codes.Count; index = GetNext(index, codes)) {
                    var iLoadNetInfo1 = codes.Search(c => c.IsLdLoc(typeof(NetInfo), method), startIndex: index, count: -1);
                        codes.InsertInstructions(index, new[] {
                        codes[iLoadNetInfo1].Clone(),
                        new CodeInstruction(OpCodes.Call, mCheckConnectGroup),
                    });
                    count++;
                }
                Log.Info(ThisMethod + $" successfully patched {count} places in " + method);
            } catch (Exception ex) {
                ex.Log("failed to patch " + method);
            }
        }
    }

    /// <summary> insert after the clause:'info.m_nodeConnectGroups & info.m_connectGroup' </summary>
    internal static class CheckNetConnectGroup {
        public static bool CheckConnectGroup(bool flagsMatch, NetInfo sourceInfo, NetInfo targetInfo) {
            if (flagsMatch)
                return true;
            return DirectConnectUtil.ConnectGroupsMatch(
                sourceInfo.GetMetaData()?.NodeConnectGroupsHash,
                targetInfo.GetMetaData()?.ConnectGroupsHash);
        }

        static FieldInfo fNetNodeConnectGroups => typeof(NetInfo).GetField(nameof(NetInfo.m_nodeConnectGroups));
        static FieldInfo fNetConnectGroup => typeof(NetInfo).GetField(nameof(NetInfo.m_connectGroup));
        static MethodInfo mCheckConnectGroup => typeof(CheckNetConnectGroup).GetMethod(nameof(CheckConnectGroup), throwOnError: true);

        public static int GetNext(int startIndex, List<CodeInstruction> codes) {
            for (int i = startIndex; i < codes.Count - 1; ++i) {
                //find the clause
                bool b = codes[i].LoadsField(fNetNodeConnectGroups);
                b = b && codes[i + 2].LoadsField(fNetConnectGroup);
                b = b && codes[i + 3].opcode == OpCodes.And;
                if (b)
                    return i + 4; // after the clause
            }

            return -1;
        }

        public static void Patch(List<CodeInstruction> codes, MethodBase method) {
            //return;
            try {
                int count = 0;
                for (int index = GetNext(0, codes); index >= 0 && index < codes.Count; index = GetNext(index, codes)) {
                    var iLoadNetInfo1 = codes.Search(c => c.IsLdLoc(typeof(NetInfo), method), startIndex: index, count: -2);
                    var iLoadNetInfo2 = codes.Search(c => c.IsLdLoc(typeof(NetInfo), method), startIndex: index, count: -1);
                    codes.InsertInstructions(index, new[] {
                        codes[iLoadNetInfo1].Clone(),
                        codes[iLoadNetInfo2].Clone(),
                        new CodeInstruction(OpCodes.Call, mCheckConnectGroup),
                    });
                    count++;
                 }
                Log.Info(ThisMethod + $" successfully patched {count} places in " + method);
            } catch (Exception ex) {
                ex.Log("failed to patch " + method);
            }
        }
    }
}
