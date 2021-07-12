using ColossalFramework;
using ColossalFramework.UI;
using KianCommons.UI.Table;
using KianCommons.UI;
using static KianCommons.ReflectionHelpers;
using static KianCommons.EnumBitMaskExtensions;
using static KianCommons.EnumerationExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveRoads.Data.QuayRoads;
using System.Reflection;
using KianCommons;
using AdaptiveRoads.Manager;
using AdaptiveRoads.Util;
using KianCommons.UI.Helpers;

namespace AdaptiveRoads.UI.QuayRoads {
    class QuayRoadsPanel : UIDraggableWindow<QuayRoadsPanel> {
        public override SavedFloat SavedX => ModSettings.QuayRoadsPanelX;

        public override SavedFloat SavedY => ModSettings.QuayRoadsPanelY;

        private UITable propertiesTable_;

        private NetInfo netInfo_;
        private RoadEditorPanel parentPanel_;
        private static readonly Dictionary<NetInfo, QuayRoadsPanel> instances_ = new();

        private ProfileSection[] Profile {
            get => netInfo_.GetMetaData().QuayRoadsProfile;
            set {
                netInfo_.GetMetaData().QuayRoadsProfile = value;
                parentPanel_.OnObjectModified();
            }
        }
        public override void Start() {
            base.Start();
            Caption = "Quay Road Settings for " + netInfo_.name;
            autoSize = true;

            parentPanel_.EventPanelClosed += (_) => Close();

            var buttonsPanel = AddUIComponent<UIPanel>();
            buttonsPanel.autoSize = true;
            buttonsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            buttonsPanel.autoLayout = true;
            buttonsPanel.autoFitChildrenHorizontally = true;
            buttonsPanel.autoFitChildrenVertically = true;

            var flipButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            flipButton.text = "flip";
            flipButton.eventClicked += (_,_) => {
                Profile = Profile.Inverse();
                Refresh();
            };

            var presetDropDown = buttonsPanel.AddUIComponent<UIDropDownExt>();
            presetDropDown.items = new string[] { "(choose preset)" }.Concat(Profiles.presets.Keys.ToArray()).ToArray() ;
            presetDropDown.selectedIndex = 0;
            presetDropDown.eventSelectedIndexChanged += (_, selectedIndex) => {
                if (selectedIndex != 0) {
                    Profile = Profiles.presets[presetDropDown.selectedValue];
                    presetDropDown.selectedIndex = 0;
                    Refresh();
                }
            };
            presetDropDown.listOffset = new UnityEngine.Vector2(0f, 20f);

            var addSectionButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            addSectionButton.text = "add section";
            addSectionButton.eventClicked += (_, _) => {
                Profile = Profile.Expand(Profile.Length + 1, (_) => ProfileSection.Default());
                Refresh();
            };

            var removeSectionButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            removeSectionButton.text = "remove section";
            removeSectionButton.eventClicked += (_, _) => {
                Profile = Profile.Shrink(Profile.Length - 1, (_, _) => { });
                Refresh();
            };

            var deleteButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            deleteButton.text = "delete";
            deleteButton.eventClicked += (_, _) => {
                Profile = null;
                Close();
            };

            if(Profile is null) {
                Profile = new ProfileSection[] { };
            }

            
            Refresh();
        }
        public void Refresh() {
            if (propertiesTable_ is not null) {
                RemoveUIComponent(propertiesTable_);
                DestroyImmediate(propertiesTable_.gameObject);
            }

            propertiesTable_ = AddUIComponent<UITable>();
            MakeFirstColumn();
            for (int sectionIndex = 0; sectionIndex < Profile.Length; sectionIndex++) {
                MakeEditableColumn(sectionIndex);
            }
        }
        public override void Close() {
            instances_.Remove(netInfo_);
            base.Close();
        }

        public static void CloseIfOpen(NetInfo netInfo) {
            if (instances_.ContainsKey(netInfo)) {
                instances_[netInfo].Close();
            }
        }
        public static QuayRoadsPanel GetOrOpen(NetInfo netInfo, RoadEditorPanel parentPanel) {
            if (instances_.ContainsKey(netInfo)) {
                return instances_[netInfo];
            } else {
                var newPanel = Create();
                newPanel.netInfo_ = netInfo;
                newPanel.parentPanel_ = parentPanel;
                instances_[netInfo] = newPanel;
                return newPanel;
            }
        }
        public static void CloseAll() {
            foreach(var instance in instances_) {
                instance.Value.Close();
            }
        }

        private struct PropertyGroup {
            public string GroupName;

            public PropertyGroup(string groupName) : this() {
                GroupName = groupName;
                Properties = new();
            }

            public List<Property> Properties;
        }
        private struct Property {
            public FieldInfo fieldInfo;
            public IConvertible flag; // treated as nullable, this is okay, as flag = 0 doen't make sense

            public Property(FieldInfo fieldInfo, IConvertible flag = null) {
                this.fieldInfo = fieldInfo;
                this.flag = flag;
            }
        }

        // TODO: precompute this. When?
        private List<PropertyGroup> MakeGroups() {
            List<PropertyGroup> groups = new();
            foreach (var property in
                typeof(ProfileSection)
                .GetFieldsWithAttribute<CustomizablePropertyAttribute>()
                .OrderByDescending(property => property.GetAttribute<CustomizablePropertyAttribute>().priority)
                ) {
                var attribute = property.GetAttribute<CustomizablePropertyAttribute>();
                var groupIndex = groups.FindIndex(group => group.GroupName == attribute.group);
                if (groupIndex == -1) {
                    groups.Add(new(attribute.group));
                    groupIndex = groups.Count - 1;
                }
                Log.Debug(attribute.group.ToString());

                if (property.FieldType.HasAttribute<FlagsAttribute>()) {
                    foreach (var flag in GetPow2Values(property.FieldType)) {
                        groups[groupIndex].Properties.Add(new(property, flag));
                    }
                } else {
                    groups[groupIndex].Properties.Add(new(property));
                }
                Log.Debug(attribute.group.ToString());
            }
            return groups;
        }
        private void MakeFirstColumn() {
            var groups = MakeGroups();
            int row = 1;
            propertiesTable_.Expand(1 + groups.Count + groups.Sum(group => group.Properties.Count), 1 + Profile.Length);
            foreach (var group in groups) {
                var groupLabel = propertiesTable_.GetCell(row, 0).AddUIComponent<UILabel>();
                row++;
                groupLabel.text = group.GroupName;

                foreach (var property in group.Properties) {
                    if (property.flag is not null) {
                        Log.Debug(property.flag.ToString());
                        Log.Debug(property.flag.ToUInt64().ToString());
                        propertiesTable_.Expand(row + 1, 1);
                        var propertyLabel = propertiesTable_.GetCell(row, 0).AddUIComponent<UILabel>();
                        row++;
                        propertyLabel.text = property.flag.ToString();
                    } else {
                        propertiesTable_.Expand(row + 1, 1);
                        var propertyLabel = propertiesTable_.GetCell(row, 0).AddUIComponent<UILabel>();
                        row++;
                        propertyLabel.text = property.fieldInfo.GetAttribute<CustomizablePropertyAttribute>().name;

                    }
                }
            }
        }
        private void MakeEditableColumn(int sectionIndex) {
            var groups = MakeGroups();
            int row = 0;
            propertiesTable_.Expand(1, sectionIndex + 1);
            var header = propertiesTable_.GetCell(0, sectionIndex + 1).AddUIComponent<UILabel>();
            header.text = sectionIndex.ToString();
            row++;
            foreach (var group in groups) {
                row++;

                foreach (var property in group.Properties) {
                    var field = new QuayRoadsPanelField(netInfo_, sectionIndex, property.fieldInfo, property.flag, propertiesTable_.GetCell(row, sectionIndex + 1), parentPanel_);
                    row++;
                }
            }

        }

    }
}
