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
    public class SaveTemplatePanel : TemplatePanelBase {
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

        public override void Awake() {
            base.Awake();
            AddDrag("Save Prop Template");
            {
                UIPanel panel = AddLeftPanel();
                {
                    SavesListBox = panel.AddUIComponent<SavesListBoxT>();
                    SavesListBox.width = panel.width;
                    SavesListBox.height = 628;
                    SavesListBox.AddScrollBar();
                    SavesListBox.eventSelectedIndexChanged += (_, val) =>
                        OnSelectedSaveChanged(val);
                }
                {
                    NameField = panel.AddUIComponent<MenuTextField>();
                    NameField.text = "New Template";
                    NameField.width = panel.width;
                    NameField.eventTextChanged += (_, __) => OnNameChanged();
                }
            }
            {
                UIPanel panel = AddRightPanel();
                {
                    SummaryBox = panel.AddUIComponent<SummaryLabel>();
                    SummaryBox.width = panel.width;
                    SummaryBox.height = 400;
                }
                {
                    DescriptionField = panel.AddUIComponent<MenuTextField>();
                    DescriptionField.multiline = true;
                    DescriptionField.text = "Description";
                    DescriptionField.width = panel.width;
                    DescriptionField.height = 162;
                }
            }

            FitChildrenVertically(10);
            {
                var cancel = AddUIComponent<MenuButton>();
                cancel.text = "Cancel";
                var pos = size - cancel.size - new Vector2(20, 10);
                cancel.relativePosition = pos;
                cancel.eventClick += (_, __) => Destroy(gameObject);
                SaveButton = AddUIComponent<MenuButton>();
                SaveButton.text = "Save";
                pos.x += -SaveButton.size.x - 20;
                SaveButton.relativePosition = pos;
                SaveButton.eventClick += (_, __) => OnSave();
            }
        }


        bool started_ = false;
        public override void Start() {
            Log.Debug("SaveTemplatePanel.Start() called");
            base.Start();
            if (Props == null) {
                Destroy(gameObject);
                return;
            }
            started_ = true;
            OnNameChanged();
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public void OnSave() {
            if (string.IsNullOrEmpty(NameField.text)) return;
            eventsOff_ = true;
            var template = PropTemplate.Create(
                NameField.text,
                Props.ToArray(),
                DescriptionField.text);
            template.Save();
            SavesListBox.Populate();
            eventsOff_ = false;
            OnNameChanged();
        }

        public string RemoveInvalidChars(string s) =>
            s.Trim(Path.GetInvalidFileNameChars());

        bool eventsOff_ = false;
        public void OnNameChanged() {
            try {
                Log.Debug($"OnNameChanged called. " +
                    $"eventsOff_={eventsOff_} " +
                    $"NameField.text={NameField.text}\n"
                    + Environment.StackTrace);
                if (!eventsOff_ && started_) {
                eventsOff_ = true;
                NameField.text = RemoveInvalidChars(NameField.text);
                SaveButton.isEnabled = !string.IsNullOrEmpty(NameField.text);

                SavesListBox.Select(NameField.text);
                if (SavesListBox.selectedIndex < 0) {
                    SummaryBox.text = Props.Summary();
                    SaveButton.text = "Save";
                } else {
                    SummaryBox.text = SavesListBox.SelectedTemplate.Summary;
                    SaveButton.text = "Overwrite";
                }
                eventsOff_ = false;
                Invalidate();
            }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }
        public void OnSelectedSaveChanged(int newIndex) {
            Log.Debug($"OnSelectedSaveChanged({newIndex})\n" + Environment.StackTrace);
            try {
                if (!eventsOff_ && newIndex >= 0 && started_) {
                    DescriptionField.text = SavesListBox.SelectedTemplate.Description;
                    NameField.text = SavesListBox.SelectedTemplate.Name;
                    Invalidate();
                }
            }catch(Exception ex) {
                Log.Exception(ex, $"newIndex={newIndex} " +
                    $"SelectedIndex={SavesListBox.selectedIndex} " +
                    $"SelectedTemplate={SavesListBox.SelectedTemplate.ToSTR()} " +
                    $"Saves[0]={SavesListBox.Saves[0].ToSTR()}");
            }
        }

    }
}
