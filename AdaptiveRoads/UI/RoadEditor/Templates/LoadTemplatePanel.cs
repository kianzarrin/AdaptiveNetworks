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
    public class LoadTemplatePanel : PanelBase {
        public SummaryLabel SummaryBox;
        public SavesListBoxT SavesListBox;
        public UIButton LoadButton;
        public Checkbox ToggleDir;
        public Checkbox ToggleSide;
        public TextFieldInt Displacement;

        public delegate void OnPropsLoadedHandler(NetLaneProps.Prop[] props);
        public event OnPropsLoadedHandler OnPropsLoaded;

        public static LoadTemplatePanel Display(OnPropsLoadedHandler handler) {
            Log.Debug($"LoadTemplatePanel.Display() called");
            var ret = UIView.GetAView().AddUIComponent<LoadTemplatePanel>();
            ret.OnPropsLoaded = handler;
            return ret;
        }

        public override void Awake() {
            base.Awake();
            AddDrag("Load Prop Template");
            {
                UIPanel panel = AddLeftPanel();
                {
                    SavesListBox = panel.AddUIComponent<SavesListBoxT>();
                    SavesListBox.width = panel.width;
                    SavesListBox.height = 628;
                    SavesListBox.AddScrollBar();
                    SavesListBox.eventSelectedIndexChanged += (_, val) =>
                        OnSelectedSaveChanged(val);
                    SavesListBox.eventDoubleClick += (_, __) => OnLoad();
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
                    ToggleDir = panel.AddUIComponent<Checkbox>();
                    ToggleDir.Label = "Toggle Forward/Backward";
                    ToggleSide = panel.AddUIComponent<Checkbox>();
                    ToggleSide.Label = "Toggle RHT/LHT";
                }
                {
                    //Displacement = panel.AddUIComponent<TextFieldInt>();
                    //Displacement.width = panel.width;

                    UIPanel panel2 = panel.AddUIComponent<UIPanel>();
                    panel2.autoLayout = true;
                    panel2.autoLayoutDirection = LayoutDirection.Horizontal;
                    panel2.autoLayoutPadding = new RectOffset(0, 5, 0, 0);
                    var lbl = panel2.AddUIComponent<UILabel>();
                    lbl.text = "Displacement:";
                    Displacement = panel2.AddUIComponent<TextFieldInt>();
                    Displacement.width = panel.width - Displacement.relativePosition.x;
                    Displacement.tooltip = "put a posetive number to move props away from the junction.";
                    lbl.height = Displacement.height;
                    lbl.verticalAlignment = UIVerticalAlignment.Middle;
                    panel2.FitChildren();
                }

            }

            FitChildrenVertically(10);
            {
                var cancel = AddUIComponent<Button>();
                cancel.text = "Cancel";
                var pos = size - cancel.size - new Vector2(20, 10);
                cancel.relativePosition = pos;
                cancel.eventClick += (_, __) => Destroy(gameObject);
                LoadButton = AddUIComponent<Button>();
                LoadButton.text = "Load";
                pos.x += -LoadButton.size.x - 20;
                LoadButton.relativePosition = pos;
                LoadButton.eventClick += (_, __) => OnLoad();
            }
        }


        bool started_ = false;
        public override void Start() {
            Log.Debug("LoadTemplatePanel.Start() called");
            base.Start();
            started_ = true;
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public void OnLoad() {
            var template = SavesListBox.SelectedTemplate;
            var props = template.GetProps();
            foreach(var prop in props) {
                if (ToggleDir.isChecked)
                    prop.ToggleRHT_LHT();
                if (ToggleSide.isChecked)
                    prop.ToggleForwardBackward();
                if (Displacement.Number != 0) {
                    prop.Displace(Displacement.Number);
                }
            }
            OnPropsLoaded(props);
            Destroy(gameObject);
        }

        public void OnSelectedSaveChanged(int newIndex) {
            Log.Debug($"OnSelectedSaveChanged({newIndex})\n" + Environment.StackTrace);
            try {
                if (started_) {
                    LoadButton.isEnabled = newIndex >= 0;
                    SummaryBox.text = SavesListBox.SelectedTemplate?.Summary ?? "";
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
