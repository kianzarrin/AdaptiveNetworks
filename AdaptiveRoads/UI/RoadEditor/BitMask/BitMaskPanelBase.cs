namespace AdaptiveRoads.UI.RoadEditor.Bitmask {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using System.Reflection;
    using System.Linq;
    using KianCommons.UI.Helpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Plugins;

    internal struct FlagDataT {
        public delegate void SetHandlerD(IConvertible flag);
        public delegate IConvertible GetHandlerD();
        internal readonly Type EnumType;
        public readonly SetHandlerD SetValue;
        public readonly GetHandlerD GetValue;
        public readonly TypeCode UnderlyingType;
        public FlagDataT(SetHandlerD setValue, GetHandlerD getValue, Type enumType) {
            SetValue = setValue;
            GetValue = getValue;
            EnumType = enumType;
            UnderlyingType = getValue().GetTypeCode();
            Assertion.Assert(
                UnderlyingType == TypeCode.Int64 || UnderlyingType == TypeCode.Int32,
                $"bad enum type:{enumType}, Underlying Type:{UnderlyingType}");
            Assertion.Assert(enumType.IsEnum, "isEnum");
            Assertion.Equal(
                Type.GetTypeCode(Enum.GetUnderlyingType(enumType)),
                UnderlyingType,
                "underlaying types mismatch");
        }

        public long GetValueLong() {
            var flag = GetValue();
            switch (UnderlyingType) {
                case TypeCode.Int32: return (long)(uint)(int)flag;
                case TypeCode.Int64: return (long)flag;
                default: throw new Exception("unreachable code");
            }
        }

        public void SetValueLong(long flag) {
            switch (UnderlyingType) {
                case TypeCode.Int32:
                    SetValue((int)flag);
                    break;
                case TypeCode.Int64:
                    SetValue(flag);
                    break;
                default: throw new Exception("unreachable code");
            }
        }
    }

    public abstract class BitMaskPanelBase : UIPanel, IDataUI {
        public UILabel Label;
        public UICheckboxDropDown DropDown;

        public string Hint;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
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
            DropDown.eventAfterDropdownClose += OnAfterDropdownClose;
        }

        protected abstract void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown);

        public void OnPropertyChanged() {
            LogCalled();
            EventPropertyChanged?.Invoke();
        }

        protected abstract void Initialize();

        public void Refresh() => Initialize();

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


        // private UIFontRenderer ObtainTextRenderer()
        static MethodInfo mObtainTextRenderer = GetMethod(typeof(UIButton), "ObtainTextRenderer");
        protected static UIFontRenderer ObtainTextRenderer(UIButton button) =>
            mObtainTextRenderer.Invoke(button, null) as UIFontRenderer;

        public static void ApplyText(UICheckboxDropDown dd, string text) {
            UIButton uibutton = (UIButton)dd.triggerButton;
            var padding = uibutton.textPadding;
            padding.left = 5;
            padding.right = 21;

            uibutton.text = text; // must set text to mearure text once and only once.

            using (UIFontRenderer uifontRenderer = ObtainTextRenderer(uibutton)) {
                float p2uRatio = uibutton.GetUIView().PixelsToUnits();
                var widths = uifontRenderer.GetCharacterWidths(text);
                float x = widths.Sum() / p2uRatio;
                //Log.Debug($"{uifontRenderer}.GetCharacterWidths(\"{text}\")->{widths.ToSTR()}");
                //if (x > uibutton.width - 42) 
                //    uibutton.textHorizontalAlignment = UIHorizontalAlignment.Left;
                //else
                //    uibutton.textHorizontalAlignment = UIHorizontalAlignment.Center;

                if (x > uibutton.width - uibutton.textPadding.horizontal) {
                    for (int n = 4; n < text.Length; ++n) {
                        float x2 = widths.Take(n).Sum() / p2uRatio + 15; // 15 = width of ...
                        if (x2 > uibutton.width - 21) {
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
            if (containsMouse)
                return true;
            if (DropDown.GetHoverIndex() >= 0)
                return true;
            return false;
        }

        public string GetHint() {
            int i = DropDown.GetHoverIndex();
            if (i >= 0) {
                Enum flag = DropDown.GetItemUserData(i) as Enum;
                var hints = flag.GetEnumMemberInfo().GetHints();
                hints.Add("right-click => close drop down");
                return hints.JoinLines();
            } else if (DropDown.containsMouse || Label.containsMouse) {
                return Hint;
            } else {
                return null;
            }
        }
    }
}
