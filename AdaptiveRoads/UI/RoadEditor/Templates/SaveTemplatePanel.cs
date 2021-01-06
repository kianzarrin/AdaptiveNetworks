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

namespace AdaptiveRoads.UI.RoadEditor.Templates {
    public class SaveTemplatePanel : UIPanel {
        public UITextField NameField;
        public UITextField DescriptionField;
        public SummaryLabel Summary;
        public SavesListBoxT SavesListBox;
        public UIButton SaveButton;

        public List<NetLaneProps.Prop> Props;

        public static SaveTemplatePanel Display(IEnumerable<NetLaneProps.Prop> props) {
            Log.Debug($"SaveTemplatePanel.Display() called");
            var ret = UIView.GetAView().AddUIComponent<SaveTemplatePanel>();
            ret.Props = props.ToList();
            return ret;
        }

        public override void Awake() {
            base.Awake();
            atlas = TextureUtil.Ingame;
            backgroundSprite = "MenuPanel";
            size = new Vector2(880, 720);
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
                panel.width = 425;
                panel.relativePosition = new Vector2(10, 46);
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
                FitChildrenVertically(10);
            }
            {
                UIPanel panel = AddUIComponent<UIPanel>();
                panel.autoLayout = true;
                panel.autoLayoutDirection = LayoutDirection.Vertical;
                panel.width = 425;
                panel.relativePosition = new Vector2(425+20, 46);
                panel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
                {
                    Summary = panel.AddUIComponent<SummaryLabel>();
                    Summary.width = panel.width;
                    Summary.height = 400;
                    Summary.text = "Summary";
                }
                {
                    DescriptionField = panel.AddUIComponent<TextField>();
                    DescriptionField.multiline = true;
                    DescriptionField.text = "Description";
                    DescriptionField.width = panel.width;
                    DescriptionField.height = 162;
                }
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
        }

        bool started_ = false;
        public override void Start() {
            Log.Debug("MiniPanel.Start() called");
            base.Start();
            Invalidate(); // TODO is this necessary?
            relativePosition = new Vector2(505, 154);
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
            if (!changingText_) {
                changingText_ = true;
                NameField.text = RemoveInvalidChars(NameField.text);
                SaveButton.isEnabled = !string.IsNullOrEmpty(NameField.text);

                int index = SavesListBox.IndexOf(NameField.text);
                SavesListBox.selectedIndex = index;
                if (index < 0) {
                    Summary.text = Props.Summary();
                    SaveButton.text = "Save";
                } else {
                    Summary.text = SavesListBox.SelectedTemplate.Summary;
                    SaveButton.text = "Overwrite";
                }
                changingText_ = false;
            }
        }
        public void OnSelectedIndexChanged(int newIndex) {
            if (!changingText_ && newIndex >= 0) {
                if (newIndex >= 0) {
                    NameField.text = SavesListBox.SelectedTemplate.Name;
                    DescriptionField.text = SavesListBox.SelectedTemplate.Description;
                }
            }
        }

    }
}
