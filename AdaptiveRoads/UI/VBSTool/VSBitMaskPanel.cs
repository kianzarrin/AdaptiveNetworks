namespace AdaptiveRoads.UI.VBSTool {
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using AdaptiveRoads.Util;
    using AdaptiveRoads.UI.RoadEditor.Bitmask;
    using AdaptiveRoads.Manager;

    public static class Extension {
        public static NetSegment.Flags SetMaskedFlags(this NetSegment.Flags flags, NetSegment.Flags value, NetSegment.Flags mask) =>
            (flags & ~mask) | (value & mask);
    }

    public class VSBitMaskPanel : BitMaskPanelBase {
        public ushort SegmentID;
        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static VSBitMaskPanel Add(
            UIComponent container,
            ushort segmentId,
            string label,
            string hint) {
            try {
                Log.Called(container, segmentId, label, hint);
                var subPanel = UIView.GetAView().AddUIComponent(typeof(VSBitMaskPanel)) as VSBitMaskPanel;
                subPanel.SegmentID = segmentId;
                subPanel.Label.text = label + ":";
                subPanel.Hint = hint;
                subPanel.Initialize();

                container.AttachUIComponent(subPanel.gameObject);

                return subPanel;
            } catch (Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        protected override void Initialize() {
            try {
                //Disable();
                Populate(DropDown, flags: GetValue(), NetSegmentFlags.StopAll);
                UpdateText();
                Enable();
            } catch (Exception ex) {
                ex.Log();
            }
        }

        public static void Populate(UICheckboxDropDown dropdown, NetSegmentFlags flags, NetSegmentFlags mask) {
            try {
                foreach (var flag in new []{
                    NetSegmentFlags.BusStopLeft,
                    NetSegmentFlags.BusStopRight,
                    NetSegmentFlags.TramStopLeft,
                    NetSegmentFlags.TramStopRight,
                }) {
                    dropdown.AddItem(
                        item: flag.ToString(),
                        isChecked: flags.IsFlagSet(flag),
                        userData: flag);
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }

        protected override void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown) {
            try {
                SetValue(GetCheckedFlags());
                UpdateText();
                NetworkExtensionManager.Instance.UpdateSegment(SegmentID);
            } catch (Exception ex) {
                ex.Log();
            }
        }

        // apply checked flags from UI to prefab
        protected void SetValue(NetSegmentFlags value) {
            NetSegment.Flags flags = (NetSegment.Flags)(int)value;
            NetSegment.Flags mask = NetSegment.Flags.StopAll;
            if ((SegmentID.ToSegment().m_flags & mask) != flags) {
                SegmentID.ToSegment().m_flags = SegmentID.ToSegment().m_flags.SetMaskedFlags(value:flags , mask: mask);
            }
        }

        protected NetSegmentFlags GetValue() =>
            (NetSegmentFlags)(int)SegmentID.ToSegment().m_flags & NetSegmentFlags.StopAll;

        // get checked flags in UI
        private NetSegmentFlags GetCheckedFlags() {
            NetSegmentFlags ret = 0;
            for (int i = 0; i < DropDown.items.Length; i++) {
                if (DropDown.GetChecked(i)) {
                    ret |= (NetSegmentFlags)DropDown.GetItemUserData(i);
                }
            }
            return ret;
        }

        private void UpdateText() {
            ApplyText(DropDown, GetValue().ToString());
        }
    }
}
