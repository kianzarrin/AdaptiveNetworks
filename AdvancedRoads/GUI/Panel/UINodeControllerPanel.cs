namespace AdvancedRoads.GUI {
    using UnityEngine;
    using ColossalFramework.UI;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Formatters;
    using System.IO;
    using System.Linq;
    using Util;
    using ColossalFramework;

    public class UIAdvancedRoadsPanel : UIAutoSizePanel, IDataControllerUI {
        public static readonly SavedInputKey ActivationShortcut = new SavedInputKey(
            "ActivationShortcut",
            Settings.FileName,
            SavedInputKey.Encode(KeyCode.N, true, false, false),
            true);

        public static readonly SavedFloat SavedX = new SavedFloat(
            "PanelX", Settings.FileName, 87, true);
        public static readonly SavedFloat SavedY = new SavedFloat(
            "PanelY", Settings.FileName, 58, true);


        #region Instanciation
        public static UIAdvancedRoadsPanel Instance { get; private set; }

        static BinaryFormatter GetBinaryFormatter =>
            new BinaryFormatter { AssemblyFormat = FormatterAssemblyStyle.Simple };

        public static UIAdvancedRoadsPanel Create() {
            var uiView = UIView.GetAView();
            UIAdvancedRoadsPanel panel = uiView.AddUIComponent(typeof(UIAdvancedRoadsPanel)) as UIAdvancedRoadsPanel;
            return panel;
        }

        public static void Release() {
            var uiView = UIView.GetAView();
            var panel = (UIAdvancedRoadsPanel)uiView.FindUIComponent<UIAdvancedRoadsPanel>("UIAdvancedRoadsPanel");
            Destroy(panel);
        }
        #endregion Instanciation

        public ushort NodeID { get; private set; }

        public NodeData NodeData {
            get {
                if (NodeID == 0) return null;
                return NodeManager.Instance.GetOrCreate(NodeID);
            }
        }

        public List<IDataControllerUI> Controls;

        public override void Awake() {
            base.Awake();
            Instance = this;
            Controls = new List<IDataControllerUI>();
        }

        public override void Start() {
            base.Start();
            Log.Debug("UIAdvancedRoadsPanel started");

            width = 250;
            name = "UIAdvancedRoadsPanel";
            backgroundSprite = "MenuPanel2";
            absolutePosition = new Vector3(SavedX, SavedY);

            isVisible = false;

            {
                var dragHandle_ = AddUIComponent<UIDragHandle>();
                dragHandle_.width = width;
                dragHandle_.height = 42;
                dragHandle_.relativePosition = Vector3.zero;
                dragHandle_.target = parent;

                var lblCaption = dragHandle_.AddUIComponent<UILabel>();
                lblCaption.text = "Node controller";
                lblCaption.relativePosition = new Vector3(70, 14, 0);

                var sprite = dragHandle_.AddUIComponent<UISprite>();
                sprite.size = new Vector2(40, 40);
                sprite.relativePosition = new Vector3(5, 3, 0);
                sprite.atlas = TextureUtil.GetAtlas(AdvancedRoadsButton.AtlasName);
                sprite.spriteName = AdvancedRoadsButton.AdvancedRoadsIconActive;
            }

            {
                var panel = AddPanel();
                var label = panel.AddUIComponent<UILabel>();
                label.text = "Choose node type";

                var dropdown_ = panel.AddUIComponent<UINodeTypeDropDown>();
                Controls.Add(dropdown_);
            }

            {
                var panel = AddPanel();

                var label = panel.AddUIComponent<UILabel>();
                label.text = "Corner smoothness";
                label.tooltip = "Adjusts Corner offset for smooth junction transition.";

                var slider_ = panel.AddUIComponent<UIOffsetSlider>();
                Controls.Add(slider_);
            }

            AddPanel().name = "Space";

            {
                var panel = AddPanel();
                var checkBox = panel.AddUIComponent<UIHideMarkingsCheckbox>();
                Controls.Add(checkBox);
            }
            {
                var panel = AddPanel();
                var checkBox = panel.AddUIComponent<UIFlatJunctionsCheckbox>();
                Controls.Add(checkBox);
            }

            {
                var panel = AddPanel();
                var button = panel.AddUIComponent<UIResetButton>();
                Controls.Add(button);
            }
        }

        UIAutoSizePanel AddPanel() {
            int pad_horizontal = 10;
            int pad_vertical = 5;
            UIAutoSizePanel panel = AddUIComponent<UIAutoSizePanel>();
            panel.width = width - pad_horizontal * 2;
            panel.autoLayoutPadding =
                new RectOffset(pad_horizontal, pad_horizontal, pad_vertical, pad_vertical);
            return panel;
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

        public void ShowNode(ushort nodeID) {
            NodeManager.Instance.RefreshData(NodeID);
            NodeID = nodeID;
            Show();
            Refresh();
        }

        public void Close() {
            NodeManager.Instance.RefreshData(NodeID);
            NodeID = 0;
            Hide();
        }

        public void Apply() {
            foreach(IDataControllerUI control in Controls ?? Enumerable.Empty<IDataControllerUI>())
                control.Apply();
        }

        public void Refresh() {
            foreach (IDataControllerUI control in Controls ?? Enumerable.Empty<IDataControllerUI>())
                control.Refresh();
            //Update();
            RefreshSizeRecursive();
        }
    }
}
