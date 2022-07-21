namespace AdaptiveRoads.UI.Tool {
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using System.Linq;
    using UnityEngine;
    using AdaptiveRoads.Data.NetworkExtensions;

    internal class SegmentValueSelector : UIDropDownExt {
        static NetworkExtensionManager man_ => NetworkExtensionManager.Instance;

        ushort segmentID_;
        ushort []segmentIDs_;
        int index_;
        UIPanel wrapper_ => parent as UIPanel;

        static Data.UserValueNames ValueNames(ushort segmentID, int i) {
            return Names(segmentID).ValueNames[i];
        }
        static Data.UserDataNames Names(ushort segmentID) {
            return segmentID.ToSegment().Info.GetMetaData().UserDataNamesSet.Segment;
        }
        static void Allocate(ushort segmentID) {
            ref var segmentExt = ref segmentID.ToSegmentExt();
            segmentExt.UserData.Allocate(Names(segmentID));
        }

        public static SegmentValueSelector Add(UIPanel parent, ushort segmentID, ushort []segmentIDs, int index) {
            var panel = parent.AddUIComponent<UIPanel>();
            panel.autoLayout = panel.autoFitChildrenHorizontally = panel.autoFitChildrenVertically = true;
            panel.autoLayoutDirection = LayoutDirection.Vertical;
            panel.AddUIComponent<UILabel>().text = ValueNames(segmentID, index).Title + ":";
            var selector = panel.AddUIComponent<SegmentValueSelector>();
            selector.index_ = index;
            selector.segmentID_ = segmentID;
            selector.segmentIDs_ = segmentIDs ?? new ushort[0];
            return selector;
        }

        public override void Start() {
            started_ = false;
            base.Start();
            items = ValueNames(segmentID_, index_).Items;
            Allocate(segmentID_);
            

            selectedIndex = GetValue(segmentID_,index_);
            foreach(ushort segmentID in segmentIDs_) {
                Allocate(segmentID);
                if (GetValue(segmentID, index_) != selectedIndex) {
                    SetColor(Color.yellow);
                }
            }
            started_ = true;
        }
        bool started_ = false;

        protected override void OnSelectedIndexChanged() {
            base.OnSelectedIndexChanged();
            if (started_) {
                SetColor(Color.white);
                SimulationManager.instance.AddAction(delegate () {
                    SetValue(segmentID_, index_, selectedIndex);
                    foreach (var segmentID in segmentIDs_) {
                        SetValue(segmentID, index_, selectedIndex);
                    }
                });
            }
        }

        static ref int GetValue(ushort segmentID, int index) {
            ref var segmentExt = ref segmentID.ToSegmentExt();
            return ref segmentExt.UserData.UserValues[index];
        }

        public static void SetValue(ushort segmentID, int index, int value) {
            ref int segmentValue = ref GetValue(segmentID, index);
            
            if (segmentValue != value) {
                segmentValue = value;
                man_.UpdateSegment(segmentID);
            }
        }

        public void SetColor(Color color) {
            this.color = color;
        }
    }
}
