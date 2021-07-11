using AdaptiveRoads.Manager;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using static KianCommons.EnumBitMaskExtensions;
using static KianCommons.EnumerationExtensions;
using static KianCommons.ReflectionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AdaptiveRoads.Data.QuayRoads;
using UnityEngine;
using static UnityEngine.Object;
using System.Globalization;
using ColossalFramework;
using AdaptiveRoads.Util;

namespace AdaptiveRoads.UI.QuayRoads {
    class QuayRoadsPanelField {
        private readonly NetInfo netInfo_;
        private readonly int sectionIndex_;
        private readonly FieldInfo fieldInfo_;
        private readonly IConvertible flag_;
        private readonly RoadEditorPanel parentPanel_;

        public QuayRoadsPanelField(NetInfo netInfo, int sectionIndex, FieldInfo fieldInfo, IConvertible flag, UIPanel parent, RoadEditorPanel parentPanel) {
            netInfo_ = netInfo;
            sectionIndex_ = sectionIndex;
            fieldInfo_ = fieldInfo;
            flag_ = flag;
            parentPanel_ = parentPanel;
            if (flag_ is not null) {

                Log.Debug(flag.ToString());
                Log.Debug(flag.ToUInt64().ToString());
                var propertyCheckbox = parent.AddUIComponent<UICheckBoxExt>();
                propertyCheckbox.Label = "";
                propertyCheckbox.width = 20f;
                propertyCheckbox.isChecked = (AssetValue as IConvertible).IsFlagSet(flag);
                propertyCheckbox.eventCheckChanged += (_, _isChecked) => {
                    //TODO: find a better / more robust way to do this.
                    if (_isChecked) {
                        if (Enum.GetUnderlyingType(fieldInfo_.FieldType) == typeof(Int32)) {
                            AssetValue = (AssetValue as IConvertible).ToInt32(CultureInfo.InvariantCulture) | flag.ToInt32(CultureInfo.InvariantCulture);
                        } else {
                            AssetValue = (AssetValue as IConvertible).ToInt64() | flag.ToInt64();
                        }
                    } else {
                        if (Enum.GetUnderlyingType(fieldInfo_.FieldType) == typeof(Int32)) {
                            AssetValue = (AssetValue as IConvertible).ToInt32(CultureInfo.InvariantCulture) & ~flag.ToInt32(CultureInfo.InvariantCulture);
                        } else {
                            AssetValue = (AssetValue as IConvertible).ToInt64() &~flag.ToInt64();
                        }
                    }
                };
            } else {
                // TODO: right now, this assumes everything that is not a flags enum is a float
                // TODO: find a better way to create a text field - maybe something similar to UICheckBoxExt from KianCommons?
                var propertyHelper = new UIHelper(parent);
                var propertyTextField = propertyHelper.AddTextfield("wawa", (AssetValue as float?).ToString(), (_) => { }, (_) => { }) as UITextField;
                var labelObject = parent.Find<UILabel>("Label");
                labelObject.parent.RemoveUIComponent(labelObject);
                Destroy(labelObject.gameObject);
                (propertyTextField.parent as UIPanel).autoFitChildrenHorizontally = true;
                (propertyTextField.parent as UIPanel).autoFitChildrenVertically = true;
                propertyTextField.numericalOnly = true;
                propertyTextField.allowFloats = true;
                propertyTextField.allowNegative = true;
                propertyTextField.eventTextSubmitted += (_, value) => {
                    float newValue = (float)LenientStringToDouble(value, (double)(float)AssetValue);
                    propertyTextField.text = newValue.ToString();
                    if (newValue != (float)AssetValue) {
                        AssetValue = newValue;
                    }
                };
            }
        }
        private ProfileSection ProfileSection {
            get => netInfo_.GetMetaData().QuayRoadsProfile[sectionIndex_];
            set => netInfo_.GetMetaData().QuayRoadsProfile[sectionIndex_] = value;
        }
        private object AssetValue {
            get => fieldInfo_.GetValue(ProfileSection);
            set {
                object tmp = ProfileSection;
                fieldInfo_.SetValue(tmp, value);
                ProfileSection = (ProfileSection) tmp;
                Log.Debug(fieldInfo_.Name + " set to " + AssetValue + " should new value: " + value);
                parentPanel_.OnObjectModified();
            }
        }

        private static double LenientStringToDouble(string s, double fallback) {
            IFormatProvider invariantProvider = CultureInfo.InvariantCulture;
            NumberStyles style = NumberStyles.Any;
            if (Double.TryParse(s.Replace(',', '.'), style, invariantProvider, out double x)) {
                return x;
            } else {
                foreach (IFormatProvider provider in new IFormatProvider[]{CultureInfo.InvariantCulture, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture,
    CultureInfo.InstalledUICulture }) {
                    if (Double.TryParse(s, style, provider, out x)) {
                        return x;
                    }
                }
                return fallback;
            }
        }
    }
}
