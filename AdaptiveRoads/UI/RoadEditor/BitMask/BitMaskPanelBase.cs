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
    using System.Collections.Generic;

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
            try {
                base.Awake();
                size = new Vector2(370, 54);
                atlas = TextureUtil.Ingame;
                color = new Color32(87, 97, 100, 255);

                Label = AddUIComponent<UILabel>();
                Label.relativePosition = new Vector2(0, 6);

                DropDown = AddUIComponent<UICheckboxDropDown>();
                EditorMultiSelectDropDown.Init(DropDown);
                DropDown.relativePosition = new Vector2(width - DropDown.width, 28);
                DropDown.eventAfterDropdownClose += OnAfterDropdownClose;
                isInteractive = true;
            } catch (Exception ex) {
                ex.Log();
            }
        }

        public override void Start() {
            base.Start();
            FitTo(parent, LayoutDirection.Horizontal);
        }

        protected abstract void OnAfterDropdownClose(UICheckboxDropDown checkboxdropdown);

        public void OnPropertyChanged() {
            try {
                LogCalled();
                EventPropertyChanged?.Invoke();
            } catch (Exception ex) {
                ex.Log();
            }
        }

        protected abstract void Initialize();

        public void Refresh() {
            try {
                DropDown.Clear();
                Initialize();
            } catch (Exception ex) { ex.Log(); }
        }

        public static void Populate(UICheckboxDropDown dropdown, long flags, Type enumType) {
            try {
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
            } catch (Exception ex) {
                ex.Log();
            }
        }

        /// <summary>
        /// convirts enum value to its underlying type. (useful for enum.format)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="underlyingType"></param>
        /// <returns></returns>
        public static IConvertible Convert2RawInteger(IConvertible value, TypeCode underlyingType) {
            return underlyingType switch {
                TypeCode.Int32 => (int)value,
                TypeCode.Int64 => (long)value,
                _ => value,
            };
        }


        // private UIFontRenderer ObtainTextRenderer()
        static MethodInfo mObtainTextRenderer = GetMethod(typeof(UIButton), "ObtainTextRenderer");
        protected static UIFontRenderer ObtainTextRenderer(UIButton button) =>
            mObtainTextRenderer.Invoke(button, null) as UIFontRenderer;

        public static void ApplyText(UICheckboxDropDown dd, string text) {
            try {
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
            } catch (Exception ex) {
                ex.Log();
            }
        }

        [FPSBoosterSkipOptimizations]
        public override void Update() {
            try {
                base.Update();
                if (IsHovered())
                    backgroundSprite = "GenericPanelWhite";
                else
                    backgroundSprite = "";
            } catch (Exception ex) {
                ex.Log();
            }
            if (Input.GetMouseButtonDown(1))
                DropDown.ClosePopup(); // close all popups on left click
        }

        public bool IsHovered() {
            try {
                if (containsMouse)
                    return true;
                if (DropDown.GetHoverIndex() >= 0)
                    return true;
            } catch (Exception ex) {
                ex.Log();
            }
            return false;
        }

        public string GetHint() {
            try {
                int i = DropDown.GetHoverIndex();
                if (i >= 0) {
                    var userData = DropDown.GetItemUserData(i);
                    List<string> hints = new List<string>();
                    if (userData is Enum flag)
                        hints = flag.GetEnumMemberInfo().GetHints();
                    else if(userData != null)
                        hints.Add(userData.ToString());
                    hints.Add("right-click => close drop down");
                    return hints.JoinLines();
                } else if (containsMouse) {
                    return Hint;
                } else {
                    return null;
                }
            } catch (Exception ex) {
                ex.Log();
                return null;
            }
        }
    }
}
