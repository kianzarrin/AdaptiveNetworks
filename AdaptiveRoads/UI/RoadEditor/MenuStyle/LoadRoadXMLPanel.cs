using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using AdaptiveRoads.DTO;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class LoadRoadXMLPanel : PersitancyPanelBase {
        public SummaryLabel SummaryBox;
        public SavesListBoxT<RoadAssetInfo> SavesListBox;
        public UIButton LoadButton;

        public delegate void OnLoadedHandler(NetInfo info);
        public event OnLoadedHandler OnLoaded;

        public static LoadRoadXMLPanel Display(OnLoadedHandler handler) {
            Log.Debug($"LoadTemplatePanel.Display() called");
            var ret = UIView.GetAView().AddUIComponent<LoadRoadXMLPanel>();
            ret.OnLoaded = handler;
            return ret;
        }

        public override void Awake() {
            base.Awake();
            AddDrag("Load Road XML");
            {
                UIPanel panel = AddLeftPanel();
                {
                    SavesListBox = panel.AddUIComponent<SavesListBoxT<RoadAssetInfo>>();
                    SavesListBox.width = panel.width;
                    SavesListBox.height = 470;
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
            var info = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
            template.WriteToGame(info);
            OnLoaded(info);
            Destroy(gameObject);
        }

        public void OnSelectedSaveChanged(int newIndex) {
            Log.Debug($"OnSelectedSaveChanged({newIndex})\n" + Environment.StackTrace);
            try {
                if (started_) {
                    LoadButton.isEnabled = newIndex >= 0;
                    var selectedItem = SavesListBox.SelectedTemplate;
                    if (selectedItem == null) {
                        SummaryBox.text = "";
                    } else {
                        SummaryBox.text = selectedItem.Description;
                    }
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
