namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using AdaptiveRoads.Util;
    using System.Reflection;
    using System.Linq;
    using KianCommons.UI.Helpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.ReflectionHelpers;
    using HarmonyLib;
    using KianCommons.Plugins;
    using System.Collections.Generic;

    public class MultiBitMaskPanel : UIPanel, IDataUI {


        internal FlagDataT[] FlagDatas;
        public UILabel Label;
        public UICheckboxDropDown DropDown;
        
        public string Hint;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static MultiBitMaskPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            params FlagDataT [] flagDatas) {
            Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label})");
            var subPanel = UIView.GetAView().AddUIComponent(typeof(MultiBitMaskPanel)) as MultiBitMaskPanel;
            subPanel.FlagDatas = flagDatas;
            subPanel.Initialize();
            subPanel.Label.text = label + ":";
            subPanel.Hint = hint;

            container.AttachUIComponent(subPanel.gameObject);
            roadEditorPanel.FitToContainer(subPanel);
            subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;

            return subPanel;
        }

        public override void Awake() {
            base.Awake();
            size = new Vector2(370, 54);
            atlas = TextureUtil.Ingame;
            //backgroundSprite = "GenericPanelWhite";
            //color = Color.white;

            Label = AddUIComponent<UILabel>();
            Label.relativePosition = new Vector2(0, 6);

            DropDown = AddUIComponent<UICheckboxDropDown>();
            EditorMultiSelectDropDown.Init(DropDown);
            DropDown.relativePosition = new Vector2(width - DropDown.width, 28);
            DropDown.eventAfterDropdownClose += DropdownClose;
        }

        private void Initialize() {
            //Disable();
            Populate(DropDown, FlagDatas);
            UpdateText();
            Enable();
        }

        public void Refresh() => Initialize();

        internal static void Populate(UICheckboxDropDown dropdown, FlagDataT [] flagDatas) {
            foreach (FlagDataT flagData in flagDatas) {
                Populate(dropdown, flagData.GetValueLong(), flagData.EnumType);
            }
        }

        public static void Populate(UICheckboxDropDown dropdown, long flags, Type enumType) {
            var values = EnumBitMaskExtensions.GetPow2Values(enumType);
            foreach (IConvertible flag in values) {
                bool hasFlag = (flags & flag.ToInt64()) != 0;

                var itemInfo = enumType.GetEnumMemberInfo(flag);
                bool hide = itemInfo.HasAttribute<HideAttribute>();
                hide &= ModSettings.HideIrrelavant;
                hide &= !hasFlag;
                if (hide)
                    continue; // hide

                dropdown.AddItem(
                    item: Enum.GetName(enumType, flag),
                    isChecked: hasFlag,
                    userData: flag);
            }
        }

        private void DropdownClose(UICheckboxDropDown checkboxdropdown) {
            SetValue(GetCheckedFlags());
            UpdateText();
        }

        // apply checked flags from UI to prefab
        protected void SetValue(long []enumFlags) {
            for (int i = 0; i < FlagDatas.Length; ++i) {
                long originalValue = FlagDatas[i].GetValueLong();
                if (originalValue == enumFlags[i]) {
                    FlagDatas[i].SetValueLong(enumFlags[i]);
                    EventPropertyChanged?.Invoke();
                }
            }
        }

        // get checked flags in UI
        private long[] GetCheckedFlags() {
            long[] ret = new long[FlagDatas.Length];
            for(int i = 0; i < DropDown.items.Length; i++) {
                if(DropDown.GetChecked(i)) {
                    IConvertible flag = DropDown.GetItemUserData(i) as IConvertible;
                    int j = FlagDatas.FindIndex(item => item.EnumType == flag.GetType());
                    Assertion.GTEq(j, 0, "j");
                    ret[j] |= flag.ToInt64();
                }
            }
            return ret;
        }

        private string ToText(IConvertible[] enumFlags) {
            string ret = "";
            for (int i = 0; i < FlagDatas.Length; ++i) {
                if (enumFlags[i].ToInt64() == 0) continue;
                if (ret != "") ret += ", ";
                ret += Enum.Format(enumType: FlagDatas[i].EnumType, value: enumFlags[i], "G");
            }
            if (ret == "") ret = "None";
            return ret;
        }

        private void UpdateText() {
            var enumFlags = FlagDatas.Select(item => item.GetValue()).ToArray();
            string text = ToText(enumFlags);
            ApplyText(DropDown, text);
        }

        // private UIFontRenderer ObtainTextRenderer()
        static MethodInfo mObtainTextRenderer = GetMethod(typeof(UIButton), "ObtainTextRenderer");
        static UIFontRenderer ObtainTextRenderer(UIButton button) =>
            mObtainTextRenderer.Invoke(button, null) as UIFontRenderer;

        public static void ApplyText(UICheckboxDropDown dd, string text) {
            UIButton uibutton = (UIButton)dd.triggerButton;
            var padding = uibutton.textPadding;
            padding.left = 5;
            padding.right = 21;

            uibutton.text = text; // must set text to mearure text once and only once.

            using(UIFontRenderer uifontRenderer = ObtainTextRenderer(uibutton)) {
                float p2uRatio = uibutton.GetUIView().PixelsToUnits();
                var widths = uifontRenderer.GetCharacterWidths(text);
                float x = widths.Sum() / p2uRatio;
                //Log.Debug($"{uifontRenderer}.GetCharacterWidths(\"{text}\")->{widths.ToSTR()}");
                //if (x > uibutton.width - 42) 
                //    uibutton.textHorizontalAlignment = UIHorizontalAlignment.Left;
                //else
                //    uibutton.textHorizontalAlignment = UIHorizontalAlignment.Center;

                if(x > uibutton.width - uibutton.textPadding.horizontal) {
                    for(int n = 4; n < text.Length; ++n) {
                        float x2 = widths.Take(n).Sum() / p2uRatio + 15; // 15 = width of ...
                        if(x2 > uibutton.width - 21) {
                            text = text.Substring(0, n - 1) + "...";
                            break;
                        }
                    }

                }
            }
            uibutton.text = text;
        }

        [FPSBoosterSkipOptimizations]
        public override void Update() {
            isVisible = isVisible;
            base.Update();
            if (IsHovered())
                color = Color.blue;
            else
                color = Color.white;
        }

        public bool IsHovered() {
            if(containsMouse)
                return true;
            if(DropDown.GetHoverIndex() >= 0)
                return true;
            return false;
        }

        public string GetHint() {
            int i = DropDown.GetHoverIndex();
            if (i >= 0) {
                Enum flag = DropDown.GetItemUserData(i) as Enum;
                return flag.GetEnumMemberInfo().GetHints().JoinLines();
            } else if (DropDown.containsMouse || Label.containsMouse) {
                return Hint;
            } else {
                return null;
            }
        }
    }
}
