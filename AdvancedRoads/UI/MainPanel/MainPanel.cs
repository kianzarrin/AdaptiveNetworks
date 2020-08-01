namespace AdvancedRoads.UI.MainPanel {
    using AdvancedRoads.Manager;
    using ColossalFramework;
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using KianCommons.UI.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;



    // TODO node lanes !
    // TODO lane as title. ?
    // TODO why segment flags are always 0.
    public class MainPanel : UIAutoSizePanel {
        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", ModSettings.FILE_NAME, 87, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", ModSettings.FILE_NAME, 58, true);

        #region Instanciation
        public static MainPanel Instance { get; private set; }

        public static void Release() {
            Destroy(Instance);
        }

        #endregion Instanciation

        public enum SubPrefabTypeT {
            None = 0,
            Nodes,
            Segments,
            Lanes,
            Props
        }
        public SubPrefabTypeT SubPrefabType;
        public int NodeIndex, SegmentIndex, LaneIndex, PropIndex;

        UILabel Caption;
        UIPanel Container, SubContainer;

        public static MainPanel Create() {
            var uiView = UIView.GetAView();
            MainPanel panel = uiView.AddUIComponent(typeof(MainPanel)) as MainPanel;
            return panel;
        }

        public override void Awake() {
            base.Awake();
            Instance = this;
        }

        public override void Start() {
            base.Start();
            Log.Debug("ControlPanel started");

            width = 500;
            name = "ControlPanel";
            backgroundSprite = "MenuPanel2";
            absolutePosition = new Vector3(SavedX, SavedY);


            {
                var dragHandle_ = AddUIComponent<UIDragHandle>();
                dragHandle_.width = width;
                dragHandle_.height = 42;
                dragHandle_.relativePosition = Vector3.zero;
                dragHandle_.target = parent;

                Caption = dragHandle_.AddUIComponent<UILabel>();
                Caption.text = "Network Detective";
                Caption.relativePosition = new Vector3(100, 14, 0);

                //var sprite = dragHandle_.AddUIComponent<UISprite>();
                //sprite.size = new Vector2(40, 40);
                //sprite.relativePosition = new Vector3(5, 2.5f, 0);
                //sprite.atlas = TextureUtil.GetAtlas(PedestrianBridgeButton.ATLAS_NAME);
                //sprite.spriteName = PedestrianBridgeButton.PedestrianBridgeIconPressed;

                //var closeBtn = dragHandle_.AddUIComponent<CloseButton>();
                //closeBtn.relativePosition = new Vector2(width - 40 , 3f);

                var backButton = dragHandle_.AddUIComponent<BackButton>();
                backButton.relativePosition = new Vector2(width - 40, 3f);
                backButton.isVisible = false;
            }

            AddSpacePanel(this, 10);

            Container = AddUIComponent<UIAutoSizePanel>();
            MakeMainPanel();

            isVisible = true;
        }

        static UIPanel AddSpacePanel(UIPanel panel, int space) {
            UIPanel newPanel = panel.AddUIComponent<UIPanel>();
            newPanel.width = panel.width;
            newPanel.height = space;
            return newPanel;
        }

        UIPanel ReplaceOldPanel() {
            SubContainer?.Hide();
            Destroy(SubContainer);
            SubContainer = Container.AddUIComponent<UIAutoSizePanel>();
            SubContainer.autoLayoutPadding = new RectOffset(5, 5, 5, 5);
            return SubContainer;
        }

        void MakeMainPanel() {
            UIPanel panel = ReplaceOldPanel();

            var btnNodes = panel.AddUIComponent<UIButtonExt>();
            btnNodes.name = "Nodes";
            btnNodes.text = "Nodes";
            btnNodes.eventClicked += (_, __) => MakeNodesPanel();

            var btnSegments = panel.AddUIComponent<UIButtonExt>();
            btnSegments.name = "Segments";
            btnSegments.text = "Segments";
            btnSegments.eventClicked += (_, __) => MakeSegmentsPanel();

            var btnLaneProps = panel.AddUIComponent<UIButtonExt>();
            btnLaneProps.name = "LaneProps";
            btnLaneProps.text = "LaneProps";
            btnLaneProps.eventClicked += (_, __) => MakeLanesPanel();

            BackButton.Instace.Hide();
            //CloseButton.Instace.Show();
        }

        public void Back() { }
        public void Close() { }

        void MakeNodesPanel() {
            UIPanel panel = ReplaceOldPanel();
            var nodes = NetInfoExt.EditInfo.NodeInfoExts;

            for (int i = 0; i < nodes.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "Nodes_" + i;
                button.text = $"Nodes[{i}]";
                //button.eventClicked += (_, __) => MakeNodeFlagPanel(i);
            }

            BackButton.Instace.Show();
            //CloseButton.Instace.Hide();
        }

        void MakeSegmentsPanel() {
            UIPanel panel = ReplaceOldPanel();
            var segments = NetInfoExt.EditInfo.SegmentInfoExts;

            for (int i = 0; i < segments.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "Segments_" + i;
                button.text = $"Segments[{i}]";
                //button.eventClicked += (_, __) => MakeSegmentFlagPanel(i);
            }

            BackButton.Instace.Show();
            //CloseButton.Instace.Hide();
        }

        void MakeLanesPanel() {
            UIPanel panel = ReplaceOldPanel();
            var lanes = NetInfoExt.EditInfo.LaneInfoExts;

            for (int i = 0; i < lanes.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "Lanes_" + i;
                button.text = $"Lanes[{i}]";
                button.eventClicked += (_, __) => MakeLanePropsPanel(i);
            }

            BackButton.Instace.Show();
            //CloseButton.Instace.Hide();
        }

        public static NetLaneProps.Prop GetStockProp(int laneIndex, int propIndex) {
            NetInfo netInfo = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
            var prop = netInfo.m_lanes[laneIndex].m_laneProps.m_props[propIndex];
            return prop;
        }

        void MakeLanePropsPanel(int laneIndex) {
            UIPanel panel = ReplaceOldPanel();
            var props = NetInfoExt.EditInfo.LaneInfoExts[laneIndex].PropInfoExts;

            for (int i = 0; i < props.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "LaneProps_" + i;
                var prop = GetStockProp(laneIndex, i);
                button.text = $"LaneProps[{i}]:" + prop.m_prop.name;
                button.eventClicked += (_, __) => MakeLanePropFlagsPanel(laneIndex, i);
            }

            BackButton.Instace.Show();
            //CloseButton.Instace.Hide();
        }

        void MakeLanePropFlagsPanel(int laneIndex, int propIndex) {
            UIPanel topPanel = ReplaceOldPanel();
            var prop = GetStockProp(laneIndex, propIndex);
            Caption.text = $"Lane[{laneIndex}].Props[{propIndex}]:" + prop.m_prop.name;
            var propExt = NetInfoExt.EditInfo.LaneInfoExts[laneIndex].PropInfoExts[propIndex];

            // required forbidden
            /*
            public LaneInfoFlags LaneFlags;
            public SegmentInfoFlags SegmentFlags;
            public SegmentEndInfoFlags SegmentStartFlags, SegmentEndFlags;
            public NodeInfoFlags StartNodeFlags, EndNodeFlags;
            */

            {//Lane
                {
                    AddLabel(topPanel, "Lane Flags Required:");
                    var dropdown = UICheckboxDropDownExt.Add(topPanel);

                    // Apply changes
                    dropdown.eventCheckedChanged += (_, index) => {
                        NetLaneExt.Flags flag = (NetLaneExt.Flags)dropdown.GetItemUserData(index);
                        propExt.LaneFlags.Required.SetFlags(flag, dropdown.GetChecked(index));
                        dropdown.Text = propExt.LaneFlags.Required.ToString();
                        RefreshNetworks();
                    };

                    // Populate
                    var values = GetPow2Values<NetLaneExt.Flags>();
                    foreach (NetLaneExt.Flags flag in values) {
                        dropdown.AddItem(
                            item: flag.ToString(),
                            isChecked: propExt.LaneFlags.Required.IsFlagSet(flag),
                            userData: flag);
                    }

                    BackButton.Instace.Show();
                }
                {
                    AddLabel(topPanel, "Lane Flags Forbidden:");
                    var dropdown = UICheckboxDropDownExt.Add(topPanel);

                    // Apply changes
                    dropdown.eventCheckedChanged += (_, index) => {
                        NetLaneExt.Flags flag = (NetLaneExt.Flags)dropdown.GetItemUserData(index);
                        propExt.LaneFlags.Forbidden.SetFlags(flag, dropdown.GetChecked(index));
                        dropdown.Text = propExt.LaneFlags.Forbidden.ToString();
                        RefreshNetworks();
                    };

                    // Populate
                    var values = GetPow2Values<NetLaneExt.Flags>();
                    foreach (NetLaneExt.Flags flag in values) {
                        dropdown.AddItem(
                            item: flag.ToString(),
                            isChecked: propExt.LaneFlags.Forbidden.IsFlagSet(flag),
                            userData: flag);
                    }

                    BackButton.Instace.Show();
                }
            } // Lane
            {//Segment
                {
                    AddLabel(topPanel, "Segment Flags Required:");
                    var dropdown = UICheckboxDropDownExt.Add(topPanel);

                    // Apply changes
                    dropdown.eventCheckedChanged += (_, index) => {
                        NetSegmentExt.Flags flag = (NetSegmentExt.Flags)dropdown.GetItemUserData(index);
                        propExt.SegmentFlags.Required.SetFlags(flag, dropdown.GetChecked(index));
                        dropdown.Text = propExt.SegmentFlags.Required.ToString();
                        RefreshNetworks();
                    };

                    // Populate
                    var values = GetPow2Values<NetSegmentExt.Flags>();
                    foreach (NetSegmentExt.Flags flag in values) {
                        dropdown.AddItem(
                            item: flag.ToString(),
                            isChecked: propExt.SegmentFlags.Required.IsFlagSet(flag),
                            userData: flag);
                    }

                    BackButton.Instace.Show();
                }
                {
                    AddLabel(topPanel, "Segment Flags Forbidden:");
                    var dropdown = UICheckboxDropDownExt.Add(topPanel);

                    // Apply changes
                    dropdown.eventCheckedChanged += (_, index) => {
                        NetSegmentExt.Flags flag = (NetSegmentExt.Flags)dropdown.GetItemUserData(index);
                        propExt.SegmentFlags.Forbidden.SetFlags(flag, dropdown.GetChecked(index));
                        dropdown.Text = propExt.SegmentFlags.Forbidden.ToString();
                        RefreshNetworks();
                    };

                    // Populate
                    var values = GetPow2Values<NetSegmentExt.Flags>();
                    foreach (NetSegmentExt.Flags flag in values) {
                        dropdown.AddItem(
                            item: flag.ToString(),
                            isChecked: propExt.SegmentFlags.Forbidden.IsFlagSet(flag),
                            userData: flag);
                    }

                    BackButton.Instace.Show();
                }
            } // Segment
            {//SegmentEnds
                { // Start/tail
                    {
                        AddLabel(topPanel, "Segment Start Flags Required:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetSegmentEnd.Flags flag = (NetSegmentEnd.Flags)dropdown.GetItemUserData(index);
                            propExt.SegmentStartFlags.Required.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.SegmentStartFlags.Required.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetSegmentEnd.Flags>();
                        foreach (NetSegmentEnd.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.SegmentStartFlags.Required.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    }
                    {
                        AddLabel(topPanel, "SegmentEnd Flags Forbidden:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetSegmentEnd.Flags flag = (NetSegmentEnd.Flags)dropdown.GetItemUserData(index);
                            propExt.SegmentStartFlags.Forbidden.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.SegmentStartFlags.Forbidden.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetSegmentEnd.Flags>();
                        foreach (NetSegmentEnd.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.SegmentStartFlags.Forbidden.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    }
                }
                { // End/head
                    {
                        AddLabel(topPanel, "End Segment Flags Required:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetSegmentEnd.Flags flag = (NetSegmentEnd.Flags)dropdown.GetItemUserData(index);
                            propExt.SegmentEndFlags.Required.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.SegmentEndFlags.Required.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetSegmentEnd.Flags>();
                        foreach (NetSegmentEnd.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.SegmentEndFlags.Required.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    }
                    {
                        AddLabel(topPanel, "SegmentEnd Flags Forbidden:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetSegmentEnd.Flags flag = (NetSegmentEnd.Flags)dropdown.GetItemUserData(index);
                            propExt.SegmentEndFlags.Forbidden.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.SegmentEndFlags.Forbidden.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetSegmentEnd.Flags>();
                        foreach (NetSegmentEnd.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.SegmentEndFlags.Forbidden.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    } // Forbidden
                } // End 
            } // SegmentEnds
            { // Nodes
                { // Start/tail
                    {
                        AddLabel(topPanel, "Start Node Flags Required:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetNodeExt.Flags flag = (NetNodeExt.Flags)dropdown.GetItemUserData(index);
                            propExt.StartNodeFlags.Required.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.StartNodeFlags.Required.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetNodeExt.Flags>();
                        foreach (NetNodeExt.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.StartNodeFlags.Required.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    }
                    {
                        AddLabel(topPanel, "Node Flags Forbidden:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetNodeExt.Flags flag = (NetNodeExt.Flags)dropdown.GetItemUserData(index);
                            propExt.StartNodeFlags.Forbidden.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.StartNodeFlags.Forbidden.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetNodeExt.Flags>();
                        foreach (NetNodeExt.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.StartNodeFlags.Forbidden.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    }
                }
                { // End/head
                    {
                        AddLabel(topPanel, "End Node Flags Required:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetNodeExt.Flags flag = (NetNodeExt.Flags)dropdown.GetItemUserData(index);
                            propExt.EndNodeFlags.Required.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.EndNodeFlags.Required.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetNodeExt.Flags>();
                        foreach (NetNodeExt.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.EndNodeFlags.Required.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    }
                    {
                        AddLabel(topPanel, "Node Flags Forbidden:");
                        var dropdown = UICheckboxDropDownExt.Add(topPanel);

                        // Apply changes
                        dropdown.eventCheckedChanged += (_, index) => {
                            NetNodeExt.Flags flag = (NetNodeExt.Flags)dropdown.GetItemUserData(index);
                            propExt.EndNodeFlags.Forbidden.SetFlags(flag, dropdown.GetChecked(index));
                            dropdown.Text = propExt.EndNodeFlags.Forbidden.ToString();
                            RefreshNetworks();
                        };

                        // Populate
                        var values = GetPow2Values<NetNodeExt.Flags>();
                        foreach (NetNodeExt.Flags flag in values) {
                            dropdown.AddItem(
                                item: flag.ToString(),
                                isChecked: propExt.EndNodeFlags.Forbidden.IsFlagSet(flag),
                                userData: flag);
                        }

                        BackButton.Instace.Show();
                    } // Forbidden
                } // End 
            } // Nodes
        }

        public static void AddLabel(UIPanel panel, string text) => panel.AddUIComponent<UILabel>().text = text;

        public static void RefreshNetworks() {
            //throw new NotImplementedException();
            // TODO update and invoke afterdeserialize.
            // TODO sync values of stock UI panel.
        }

        public static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;
        IEnumerable<ulong> GetPow2Values<T>() {
            Array values = Enum.GetValues(typeof(T));
            foreach (ulong value in values) {
                if(IsPow2(value))
                    yield return value;
            }
        }

        protected override void OnPositionChanged() {
            base.OnPositionChanged();
            Log.Debug("OnPositionChanged called");

            Vector2 resolution = GetUIView().GetScreenResolution();

            absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            SavedX.value = absolutePosition.x;
            SavedY.value = absolutePosition.y;
            Log.Debug("absolutePosition: " + absolutePosition);
        }
    }
}

