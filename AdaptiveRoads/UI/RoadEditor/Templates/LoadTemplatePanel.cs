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
    public class LoadTemplatePanel : PanelBase {
        public SummaryLabel SummaryBox;
        public SavesListBoxT SavesListBox;
        public UIButton LoadButton;

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

            }
            {
                UIPanel panel = AddRightPanel();
                {
                    SummaryBox = panel.AddUIComponent<SummaryLabel>();
                    SummaryBox.width = panel.width;
                    SummaryBox.height = 400;
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
