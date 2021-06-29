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

        private UITable propertiesTable;

        private NetInfo netInfo;
        private RoadEditorPanel parentPanel;
        private static Dictionary<NetInfo, QuayRoadsPanel> instances = new();

        private ProfileSection[] profile {
            get => netInfo.GetMetaData().quayRoadsProfile;
            set {
                netInfo.GetMetaData().quayRoadsProfile = value;
                parentPanel.OnObjectModified();
            }
        }
        public override void Start() {
            base.Start();
            Caption = "Quay Road Settings for " + netInfo.name;
            autoSize = true;

            parentPanel.EventPanelClosed += (_) => Close();

            var buttonsPanel = AddUIComponent<UIPanel>();
            buttonsPanel.autoSize = true;
            buttonsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            buttonsPanel.autoLayout = true;
            buttonsPanel.autoFitChildrenHorizontally = true;
            buttonsPanel.autoFitChildrenVertically = true;

            var flipButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            flipButton.text = "flip";
            flipButton.eventClicked += (_,_) => {
                profile = profile.Inverse();
                Refresh();
            };

            var presetDropDown = buttonsPanel.AddUIComponent<UIDropDownExt>();
            presetDropDown.items = new string[] { "(choose preset)" }.Concat(Profiles.presets.Keys.ToArray()).ToArray() ;
            presetDropDown.selectedIndex = 0;
            presetDropDown.eventSelectedIndexChanged += (_, selectedIndex) => {
                if (selectedIndex != 0) {
                    profile = Profiles.presets[presetDropDown.selectedValue];
                    presetDropDown.selectedIndex = 0;
                    Refresh();
                }
            };
            presetDropDown.listOffset = new UnityEngine.Vector2(0f, 20f);

            var addSectionButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            addSectionButton.text = "add section";
            addSectionButton.eventClicked += (_, _) => {
                profile = profile.Expand(profile.Length + 1, (_) => ProfileSection.Default());
                Refresh();
            };

            var removeSectionButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            removeSectionButton.text = "remove section";
            removeSectionButton.eventClicked += (_, _) => {
                profile = profile.Shrink(profile.Length - 1, (_, _) => { });
                Refresh();
            };

            var deleteButton = buttonsPanel.AddUIComponent<UIButtonExt>();
            deleteButton.text = "delete";
            deleteButton.eventClicked += (_, _) => {
                profile = null;
                Close();
            };

            if(profile is null) {
                profile = new ProfileSection[] { };
            }

            
            Refresh();
        }
        public void Refresh() {
            if (propertiesTable is not null) {
                RemoveUIComponent(propertiesTable);
                DestroyImmediate(propertiesTable.gameObject);
            }

            propertiesTable = AddUIComponent<UITable>();
            makeFirstColumn();
            for (int sectionIndex = 0; sectionIndex < profile.Length; sectionIndex++) {
                makeEditableColumn(sectionIndex);
            }
        }
        public override void Close() {
            instances.Remove(netInfo);
            base.Close();
        }

        public static void CloseIfOpen(NetInfo netInfo) {
            if (instances.ContainsKey(netInfo)) {
                instances[netInfo].Close();
            }
        }
        public static QuayRoadsPanel GetOrOpen(NetInfo netInfo, RoadEditorPanel parentPanel) {
            if (instances.ContainsKey(netInfo)) {
                return instances[netInfo];
            } else {
                var newPanel = Create();
                newPanel.netInfo = netInfo;
                newPanel.parentPanel = parentPanel;
                instances[netInfo] = newPanel;
                return newPanel;
            }
        }
        public static void CloseAll() {
            foreach(var instance in instances) {
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
            public IConvertible? flag;

            public Property(FieldInfo fieldInfo, IConvertible? flag = null) {
                this.fieldInfo = fieldInfo;
                this.flag = flag;
            }
        }

        // TODO: precompute this. When?
        private List<PropertyGroup> makeGroups() {
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
        private void makeFirstColumn() {
            var groups = makeGroups();
            int row = 1;
            propertiesTable.Expand(1 + groups.Count + groups.Sum(group => group.Properties.Count), 1 + profile.Length);
            foreach (var group in groups) {
                var groupLabel = propertiesTable.GetCell(row, 0).AddUIComponent<UILabel>();
                row++;
                groupLabel.text = group.GroupName;

                foreach (var property in group.Properties) {
                    if (property.flag is not null) {
                        Log.Debug(property.flag.ToString());
                        Log.Debug(property.flag.ToUInt64().ToString());
                        propertiesTable.Expand(row + 1, 1);
                        var propertyLabel = propertiesTable.GetCell(row, 0).AddUIComponent<UILabel>();
                        row++;
                        propertyLabel.text = property.flag.ToString();
                    } else {
                        propertiesTable.Expand(row + 1, 1);
                        var propertyLabel = propertiesTable.GetCell(row, 0).AddUIComponent<UILabel>();
                        row++;
                        propertyLabel.text = property.fieldInfo.GetAttribute<CustomizablePropertyAttribute>().name;

                    }
                }
            }
        }
        private void makeEditableColumn(int sectionIndex) {
            var groups = makeGroups();
            int row = 0;
            propertiesTable.Expand(1, sectionIndex + 1);
            var header = propertiesTable.GetCell(0, sectionIndex + 1).AddUIComponent<UILabel>();
            header.text = sectionIndex.ToString();
            row++;
            foreach (var group in groups) {
                row++;

                foreach (var property in group.Properties) {
                    var field = new QuayRoadsPanelField(netInfo, sectionIndex, property.fieldInfo, property.flag, propertiesTable.GetCell(row, sectionIndex + 1), parentPanel);
                    row++;
                }
            }

        }

    }
}
