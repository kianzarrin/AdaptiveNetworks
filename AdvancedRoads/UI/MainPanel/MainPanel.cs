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

        public int NodeIndex, SegmentIndex, LaneIndex, PropIndex;

        UILabel Caption;
        UIPanel Container, SubContainer;
        UIDragHandle Drag;

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

            name = "ControlPanel";
            backgroundSprite = "MenuPanel2";
            absolutePosition = new Vector3(SavedX, SavedY);
            atlas = TextureUtil.GetAtlas("Ingame");

            {
                Drag = AddUIComponent<UIDragHandle>();
                Drag.width = 500; // sets minimum width
                Drag.height = 42;
                Drag.relativePosition = Vector3.zero;
                Drag.target = parent;

                Caption = Drag.AddUIComponent<UILabel>();
                Caption.eventTextChanged += (_, __) => {
                    float x = (Drag.width - Caption.width) * 0.5f;
                    Caption.relativePosition = new Vector3(x, 14, 0);
                };
                
                //var closeBtn = dragHandle_.AddUIComponent<CloseButton>();
                //closeBtn.relativePosition = new Vector2(width - 40 , 3f);

                var backButton = Drag.AddUIComponent<BackButton>();
                backButton.relativePosition = new Vector2(Drag.width - 40, 3f);
            }

            AddSpacePanel(this, 10);

            Container = AddUIComponent<UIAutoSizePanel>();
            MakeMainPanel();

            isVisible = true;
            RefreshSize();
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
            LaneIndex = NodeIndex = SegmentIndex = PropIndex = -1;
            BackButton.Instace.Hide();
            //CloseButton.Instace.Show();
            Caption.text = "Advanced Roads";
    
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

            RefreshSize();
        }

        public void Back() {
            if (PropIndex >= 0)
                MakeLanePropsPanel(LaneIndex);
            else if (LaneIndex >= 0)
                MakeLanesPanel();
            else if (NodeIndex >= 0)
                MakeNodesPanel();
            else if (SegmentIndex >= 0)
                MakeSegmentsPanel();
            else
                MakeMainPanel();
        }
        public void Close() { Hide(); }

        void MakeNodesPanel() {
            LaneIndex = NodeIndex = SegmentIndex = PropIndex = -1;
            BackButton.Instace.Show();
            Caption.text = "Nodes";

            UIPanel panel = ReplaceOldPanel();
            var nodes = NetInfoExt.EditNetInfoExt.NodeInfoExts;

            for (int i = 0; i < nodes.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "Nodes_" + i;
                button.text = $"Nodes[{i}]";
                button.objectUserData = i;
                //button.eventClicked += (component, _) =>
                //    MakeNodeFlagPanel((int)component.objectUserData);
            }

            //CloseButton.Instace.Hide();
            RefreshSize();
        }

        void MakeSegmentsPanel() {
            LaneIndex = NodeIndex = SegmentIndex = PropIndex = -1;
            BackButton.Instace.Show();
            Caption.text = "Segments";
            UIPanel panel = ReplaceOldPanel();

            var segments = NetInfoExt.EditNetInfoExt.SegmentInfoExts;
            for (int i = 0; i < segments.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "Segments_" + i;
                button.text = $"Segments[{i}]";
                button.objectUserData = i;
                //button.eventClicked += (component, _) =>
                //    MakeSegmentFlagPanel((int)component.objectUserData);
            }

            BackButton.Instace.Show();
            //CloseButton.Instace.Hide();
            RefreshSize();
        }

        void MakeLanesPanel() {
            LaneIndex = NodeIndex = SegmentIndex = PropIndex = -1;
            BackButton.Instace.Show();
            Caption.text = "Lanes";
            UIPanel panel = ReplaceOldPanel();

            var lanes = NetInfoExt.EditNetInfoExt.LaneInfoExts;
            for (int i = 0; i < lanes.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "Lanes_" + i;
                button.text = $"Lanes[{i}]";
                button.objectUserData = i;
                void handler(UIComponent component, UIMouseEventParameter eventParam) {
                    MakeLanePropsPanel((int)component.objectUserData);
                }
                button.eventClicked += handler;
            }

            BackButton.Instace.Show();
            //CloseButton.Instace.Hide();
            RefreshSize();
        }

        public static NetLaneProps.Prop GetStockProp(int laneIndex, int propIndex) {
            NetInfo netInfo = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
            var lane = netInfo.m_lanes[laneIndex];
            var prop = lane.m_laneProps.m_props[propIndex];
            return prop;
        }

        void MakeLanePropsPanel(int laneIndex) {
            LaneIndex = laneIndex;
            NodeIndex = SegmentIndex = PropIndex = -1;
            BackButton.Instace.Show();
            Caption.text = $"Lane[{laneIndex}] Props";
            UIPanel panel = ReplaceOldPanel();

            var props = NetInfoExt.EditNetInfoExt.LaneInfoExts[laneIndex].PropInfoExts;
            for (int i = 0; i < props.Length; ++i) {
                var button = panel.AddUIComponent<UIButtonExt>();
                button.name = "LaneProps_" + i;
                var prop = GetStockProp(laneIndex, i);
                button.text = $"LaneProps[{i}]:" + prop.m_prop.name;
                button.objectUserData = i;
                button.eventClicked += (component, _) =>
                    MakeLanePropFlagsPanel(laneIndex, (int)component.objectUserData);
            }

            BackButton.Instace.Show();
            //CloseButton.Instace.Hide();
            RefreshSize();
        }

        void MakeLanePropFlagsPanel(int laneIndex, int propIndex) {
            Log.Debug($"MainPanel.MakeLanePropFlagsPanel({laneIndex},{propIndex})");
            NodeIndex = SegmentIndex = 0;
            LaneIndex = laneIndex;
            PropIndex = propIndex;
            BackButton.Instace.Show();

            UIPanel topPanel = ReplaceOldPanel();
            var prop = GetStockProp(laneIndex, propIndex);
            Caption.text = $"Flags for Lane[{laneIndex}].Props[{propIndex}]:" + prop.m_prop.name;
            var propExt = NetInfoExt.EditNetInfoExt.LaneInfoExts[laneIndex].PropInfoExts[propIndex];

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
                    var values = GetPow2ValuesU32<NetLaneExt.Flags>();
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
                    var values = GetPow2ValuesU32<NetLaneExt.Flags>();
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
                    var values = GetPow2ValuesU32<NetSegmentExt.Flags>();
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
                    var values = GetPow2ValuesU32<NetSegmentExt.Flags>();
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
                        var values = GetPow2ValuesU32<NetSegmentEnd.Flags>();
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
                        var values = GetPow2ValuesU32<NetSegmentEnd.Flags>();
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
                        var values = GetPow2ValuesU32<NetSegmentEnd.Flags>();
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
                        var values = GetPow2ValuesU32<NetSegmentEnd.Flags>();
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
                        var values = GetPow2ValuesU32<NetNodeExt.Flags>();
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
                        var values = GetPow2ValuesU32<NetNodeExt.Flags>();
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
                        var values = GetPow2ValuesU32<NetNodeExt.Flags>();
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
                        var values = GetPow2ValuesU32<NetNodeExt.Flags>();
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

            RefreshSize();
        }

        public static void AddLabel(UIPanel panel, string text) => panel.AddUIComponent<UILabel>().text = text;

        public static void RefreshNetworks() {
            //throw new NotImplementedException();
            // TODO update and invoke afterdeserialize.
            // TODO sync values of stock UI panel.
        }

        public static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;
        IEnumerable<uint> GetPow2ValuesU32<T>() {
            Array values = Enum.GetValues(typeof(T));
            foreach (object val in values) {
                if (IsPow2((uint)val))
                    yield return (uint)val;
            }
        }


        public void RefreshSize() {
            RefreshSizeRecursive();
            BackButton.Instace.relativePosition = new Vector2(Drag.width - 40, 3f);
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

