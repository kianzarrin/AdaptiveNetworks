namespace AdaptiveRoads.UI.VBSTool {
    using AdaptiveRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;

    public class AutoFitPanel : UIPanel {
        const int PADDING = 3;
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            autoLayoutPadding = new RectOffset(3, PADDING, PADDING, PADDING);
            padding = new RectOffset(PADDING, PADDING, PADDING, PADDING);
            eventFitChildren += OnAutoFit;
        }
            
        private void OnAutoFit() {
            size += new Vector2(PADDING, PADDING);
        }
    }

    public class VSPanel : AutoFitPanel {
        static string FileName => ModSettings.FILE_NAME;

        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        public static readonly SavedFloat SavedX = new SavedFloat(
            "VSPanelX", FileName, 0, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "VSPanelY", FileName, 150, true);


        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;
        private UIPanel Wrapper => parent as UIPanel;

        ushort segmentID_;


        public static VSPanel Create() {
            UIPanel wrapper = UIView.GetAView().AddUIComponent<UIPanel>();
            return wrapper.AddUIComponent<VSPanel>();
        }

        public static VSPanel Open(ushort segmentID) {
            var panel = Create();
            panel.segmentID_ = segmentID;
            return panel;
        }

        public void Close() => DestroyImmediate(Wrapper.gameObject);

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void Awake() {
            try {
                base.Awake();
                LogCalled();
                name = "VSPanel";
                backgroundSprite = "MenuPanel2";
                atlas = TextureUtil.Ingame;
            } catch (Exception ex) { ex.Log(); }

        }

        public override void Start() {
            try {
                base.Start();
                LogCalled();

                absolutePosition = new Vector3(SavedX, SavedY);

                {
                    dragHandle_ = AddUIComponent<UIDragHandle>();
                    dragHandle_.height = 20;
                    dragHandle_.relativePosition = Vector3.zero;
                    dragHandle_.target = this;
                    dragHandle_.width = width;
                    dragHandle_.height = 32;
                    lblCaption_ = dragHandle_.AddUIComponent<UILabel>();
                    lblCaption_.text = "Virtual Stops";
                    lblCaption_.name = "VS_caption";
                    lblCaption_.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
                }

                var panel = AddUIComponent<AutoFitPanel>();
                {
                    var flags = GetValue();
                    foreach (var flag in new[]{
                        NetSegmentFlags.BusStopLeft,
                        NetSegmentFlags.BusStopRight,
                        NetSegmentFlags.TramStopLeft,
                        NetSegmentFlags.TramStopRight,
                    }) {
                        var cb = panel.AddUIComponent<UICheckBoxExt>();
                        cb.Label = flag.ToString();
                        cb.isChecked = flags.IsFlagSet(flag);
                        cb.objectUserData = (NetSegment.Flags)flag;
                        cb.eventCheckChanged += Cb_eventCheckChanged;
                    }
                }


                isVisible = true;



            } catch (Exception ex) { ex.Log(); }
        }

        private void Cb_eventCheckChanged(UIComponent component, bool value) {
            NetSegment.Flags flag = (NetSegment.Flags)component.objectUserData;
            segmentID_.ToSegment().m_flags = segmentID_.ToSegment().m_flags.SetFlags(flag, value);
            NetworkExtensionManager.Instance.UpdateSegment(segmentID_);
        }

        protected override void OnPositionChanged() {
            Assertion.AssertStack();
            base.OnPositionChanged();
            Log.DebugWait("OnPositionChanged called", id: "OnPositionChanged called".GetHashCode(), seconds: 0.2f, copyToGameLog: false);

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.DebugWait("absolutePosition: " + absolutePosition, id: "absolutePosition: ".GetHashCode(), seconds: 0.2f, copyToGameLog: false);
        }

        protected NetSegmentFlags GetValue() =>
            (NetSegmentFlags)segmentID_.ToSegment().m_flags & NetSegmentFlags.StopAll;
    }
}