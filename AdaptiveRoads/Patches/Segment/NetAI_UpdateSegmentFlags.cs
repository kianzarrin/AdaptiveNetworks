using HarmonyLib;
using KianCommons;
using System.Reflection;

namespace AdvancedRoads.Patches.Segment {
    [HarmonyPatch]
    class NetAI_UpdateSegmentFlags {
        //public virtual void UpdateSegmentFlags(ushort segmentID, ref NetSegment data)
        static MethodInfo Target => AccessTools.DeclaredMethod(
                typeof(NetAI),
                nameof(NetAI.UpdateSegmentFlags),
                new[] { typeof(ushort), typeof(NetSegment).MakeByRefType() }) ??
            throw new System.Exception("could not find method");
        public static MethodBase TargetMethod() {
            var ret = Target;
            if (ret != null)
                Log.Debug("patching target method:" + ret);
            return ret;
        }
        static void Postfix(ushort segmentID) {
            Log.Debug("NetAI_UpdateSegmentFlags.PostFix() was called for segment:" + segmentID);
        } // end postfix
    }
}


