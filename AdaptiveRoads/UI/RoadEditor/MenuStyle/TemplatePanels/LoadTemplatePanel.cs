using AdaptiveRoads.Util;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using UnityEngine;
using AdaptiveRoads.DTO;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public abstract class LoadTemplatePanel<SavelistBoxT, T> : PersitancyPanelBase
        where T : class, ISerialziableDTO
        where SavelistBoxT : SaveListBoxBase<T> {
        public SavelistBoxT SavesListBox;
        public SummaryLabel SummaryBox;
        public UIButton LoadButton;

        protected abstract string Title { get; }
        protected abstract void AddCustomUIComponents(UIPanel panel);
        public abstract void Load(T template);

        public override void Awake() {
            base.Awake();
            AddDrag(Title);
            {
                UIPanel panel = AddLeftPanel();
                {
                    SavesListBox = panel.AddUIComponent<SavelistBoxT>();
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
                AddCustomUIComponents(panel);
            }

            FitChildrenVertically(10);

            {
                var BottomPanel = AddBottomPanel(this);
                LoadButton = BottomPanel.AddUIComponent<MenuButton>();
                LoadButton.text = "Load";
                LoadButton.eventClick += (_, __) => OnLoad();
                //pos.x += -LoadButton.size.x - 20;
                //LoadButton.relativePosition = pos;

                var cancel = BottomPanel.AddUIComponent<MenuButton>();
                cancel.text = "Cancel";
                cancel.eventClick += (_, __) => Destroy(gameObject);
                //var pos = size - cancel.size - new Vector2(20, 10);
                //cancel.relativePosition = pos;
            }
        }


        bool started_ = false;
        public override void Start() {
            Log.Called();
            base.Start();
            started_ = true;
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public void OnLoad() {
            var template = SavesListBox.SelectedTemplate;
            Load(template);
            Destroy(gameObject);
        }


        public void OnSelectedSaveChanged(int newIndex) {
            Log.Debug($"OnSelectedSaveChanged({newIndex})\n" + Environment.StackTrace);
            try {
                if (started_) {
                    LoadButton.isEnabled = newIndex >= 0;
                    SummaryBox.text = SavesListBox.SelectedTemplate?.Summary ?? "";
                }
            } catch (Exception ex) {
                Log.Exception(ex, $"newIndex={newIndex} " +
                    $"SelectedIndex={SavesListBox.selectedIndex} " +
                    $"SelectedTemplate={SavesListBox.SelectedTemplate.ToSTR()} " +
                    $"Saves[0]={SavesListBox.Saves[0].ToSTR()}");
            }
        }

    }
}
