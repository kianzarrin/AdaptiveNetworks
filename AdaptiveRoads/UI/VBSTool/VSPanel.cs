namespace AdaptiveRoads.UI.VBSTool {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class VSPanel : UIWindow {
        static string FileName => ModSettings.FILE_NAME;

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        public static readonly SavedFloat SavedX = new SavedFloat(
            "VSPanelX", FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "VSPanelY", FileName, 150, true);

        public override Vector2 Position {
            get => new Vector2(SavedX.value, SavedY.value);
            set {
                SavedX.value = value.x;
                SavedY.value = value.y;
            }
        }

        ushort segmentID_;

        public static VSPanel Open(ushort segmentID) {
            var panel = Create<VSPanel>();
            panel.Init(segmentID);
            return panel;
        }

        public void Close() => Destroy(Wrapper.gameObject);

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void Awake() {
            try {
                base.Awake();
                Log.Called();
                Title = "Virtual Stops";
            } catch (Exception ex) { ex.Log(); }
        }

        public void Init(ushort segmentID) {
            segmentID_ = segmentID;
            var flags = (NetSegmentFlags)segmentID_.ToSegment().m_flags & NetSegmentFlags.StopAll; ;
            foreach (var flag in new[]{
                    NetSegmentFlags.BusStopLeft,
                    NetSegmentFlags.BusStopRight,
                    NetSegmentFlags.TramStopLeft,
                    NetSegmentFlags.TramStopRight,
                }) {
                var cb = Container.AddUIComponent<AutoSizeCheckbox>();
                cb.Label = flag.ToString();
                cb.isChecked = flags.IsFlagSet(flag);
                cb.objectUserData = (NetSegment.Flags)flag;
                cb.eventCheckChanged += Cb_eventCheckChanged;
            }
        }

        private void Cb_eventCheckChanged(UIComponent component, bool value) {
            NetSegment.Flags flag = (NetSegment.Flags)component.objectUserData;
            segmentID_.ToSegment().m_flags = segmentID_.ToSegment().m_flags.SetFlags(flag, value);
            NetworkExtensionManager.Instance.UpdateSegment(segmentID_);
        }
    }
}