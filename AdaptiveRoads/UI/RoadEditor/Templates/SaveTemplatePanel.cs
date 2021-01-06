using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using System.Reflection;
using HarmonyLib;
using KianCommons.UI.Helpers;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Util;
using System.IO;
using System.Drawing.Imaging;

namespace AdaptiveRoads.UI.RoadEditor.Templates {


    public class SaveTemplatePanel : UIPanel {
        public UITextField NameField;
        public UITextField DescriptionField;
        public SummaryLabel SummaryBox;
        public SavesListBoxT SavesListBox;
        public UIButton SaveButton;

        public List<NetLaneProps.Prop> Props;

        public static SaveTemplatePanel Display(IEnumerable<NetLaneProps.Prop> props) {
            Log.Debug($"SaveTemplatePanel.Display() called");
            var ret = UIView.GetAView().AddUIComponent<SaveTemplatePanel>();
            ret.Props = props.ToList();
            return ret;
        }

        const int WIDTH_LEFT = 425;
        const int WIDTH_RIGHT = 475;
        const int PAD = 10;

        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            backgroundSprite = "MenuPanel";
            width = WIDTH_LEFT + WIDTH_RIGHT + PAD * 3;
            color = new Color32(49, 52, 58, 255);

            {
                var drag = AddUIComponent<UIDragHandle>();
                drag.width = width;
                drag.height = 40;
                drag.target = this;
                drag.relativePosition = Vector2.zero;
                var label = drag.AddUIComponent<UILabel>();
                label.textScale = 1.5f;
                label.text = "Save Prop Template";
                label.autoHeight = label.autoSize = true;
                label.relativePosition = new Vector2(0, 6);
                label.anchor = UIAnchorStyle.CenterHorizontal;
            }
            {
                UIPanel panel = AddUIComponent<UIPanel>();
                panel.autoLayout = true;
                panel.autoLayoutDirection = LayoutDirection.Vertical;
                panel.width = WIDTH_LEFT;
                panel.relativePosition = new Vector2(PAD, 46);
                panel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
                {
                    SavesListBox = panel.AddUIComponent<SavesListBoxT>();
                    SavesListBox.width = panel.width;
                    SavesListBox.height = 628;
                    SavesListBox.AddScrollBar();
                    SavesListBox.eventSelectedIndexChanged += (_, val) =>
                        OnSelectedIndexChanged(val);
                }
                {
                    NameField = panel.AddUIComponent<TextField>();
                    NameField.text = "New Template";
                    NameField.width = panel.width;
                    NameField.eventTextCancelled += (_, __) => OnTextChanged();
                }
                panel.FitChildrenVertically();
            }
            {
                UIPanel panel = AddUIComponent<UIPanel>();
                panel.autoLayout = true;
                panel.autoLayoutDirection = LayoutDirection.Vertical;
                panel.width = WIDTH_RIGHT;
                panel.relativePosition = new Vector2(WIDTH_LEFT + PAD * 2, 46);
                panel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
                {
                    SummaryBox = panel.AddUIComponent<SummaryLabel>();
                    SummaryBox.width = panel.width;
                    SummaryBox.height = 400;
                }
                {
                    DescriptionField = panel.AddUIComponent<TextField>();
                    DescriptionField.multiline = true;
                    DescriptionField.text = "Description";
                    DescriptionField.width = panel.width;
                    DescriptionField.height = 162;
                }
                panel.FitChildrenVertically();
            }
            FitChildrenVertically(10);

            {
                var cancel = AddUIComponent<Button>();
                cancel.text = "Cancel";
                var pos = size - cancel.size - new Vector2(20, 10);
                cancel.relativePosition = pos;
                cancel.eventClick += (_, __) => Destroy(gameObject);
                SaveButton = AddUIComponent<Button>();
                SaveButton.text = "Save";
                pos.x += -SaveButton.size.x - 20;
                SaveButton.relativePosition = pos;
                SaveButton.eventClick += (_, __) => OnSave();
            }
        }


        bool started_ = false;
        public override void Start() {
            Log.Debug("MiniPanel.Start() called");
            base.Start();
            if (Props == null) {
                Destroy(gameObject);
                return;
            }
            SummaryBox.text = Props.Summary();
            Invalidate(); // TODO is this necessary?
            pivot = UIPivotPoint.MiddleCenter;
            anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
            //relativePosition = new Vector2(505, 154);
            started_ = true;
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public void OnSave() {
            if (string.IsNullOrEmpty(NameField.text)) return;
            var template = PropTemplate.Create(
                NameField.text,
                Props.ToArray(),
                DescriptionField.text);
            template.Save();
            Destroy(gameObject);
        }

        public string RemoveInvalidChars(string s) =>
            s.Trim(Path.GetInvalidFileNameChars());

        bool changingText_ = false;
        public void OnTextChanged() {
            if (!changingText_ && started_) {
                changingText_ = true;
                NameField.text = RemoveInvalidChars(NameField.text);
                SaveButton.isEnabled = !string.IsNullOrEmpty(NameField.text);

                int index = SavesListBox.IndexOf(NameField.text);
                SavesListBox.selectedIndex = index;
                if (index < 0) {
                    SummaryBox.text = Props.Summary();
                    SaveButton.text = "Save";
                } else {
                    SummaryBox.text = SavesListBox.SelectedTemplate.Summary;
                    SaveButton.text = "Overwrite";
                }
                changingText_ = false;
                Invalidate();
            }
        }
        public void OnSelectedIndexChanged(int newIndex) {
            if (!changingText_ && newIndex >= 0 && started_) {
                if (newIndex >= 0) {
                    NameField.text = SavesListBox.SelectedTemplate.Name;
                    DescriptionField.text = SavesListBox.SelectedTemplate.Description;
                }
                Invalidate();
            }
        }

    }
}
