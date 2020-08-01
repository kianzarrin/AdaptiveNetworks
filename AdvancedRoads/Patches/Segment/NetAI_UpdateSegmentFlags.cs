using HarmonyLib;
using KianCommons;
using System.Reflection;

namespace AdvancedRoads.Patches.Segment {
    class NetAI_UpdateSegmentFlags {
        //public virtual void UpdateSegmentFlags(ushort segmentID, ref NetSegment data)
        public static MethodBase TargetMethod() =>
            AccessTools.DeclaredMethod(
                typeof(NetAI),
                nameof(NetAI.UpdateSegmentFlags),
                new[] { typeof(ushort), typeof(NetSegment).MakeByRefType() }) ??
            throw new System.Exception("could not find method");

        static void Postfix(ushort segmentID, ref NetSegment data) {
            //Log.Debug("NetAI_UpdateSegmentFlags.PostFix() was called");
            if (!NetUtil.IsNodeValid(segmentID)) return;
            ref NetSegmentExt netSegmentExt = ref NetworkExtensionManager.Instance.SegmentBuffer[segmentID];

            netSegmentExt.Start.UpdateFlags();
            netSegmentExt.Start.UpdateDirections();

            netSegmentExt.End.UpdateFlags();
            netSegmentExt.End.UpdateDirections();
        } // end postfix
    }
}