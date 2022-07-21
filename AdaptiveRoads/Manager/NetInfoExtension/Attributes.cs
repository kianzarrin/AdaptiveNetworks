namespace AdaptiveRoads.Manager {
    using System;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class FlagPairAttribute : Attribute {
        public Type MergeWithEnum;
    }

    public class AfterFieldAttribute : Attribute {
        public string FieldName;
        public AfterFieldAttribute(string fieldName) => FieldName = fieldName;
    }

    /// <summary>
    /// Field visibility in asset is controlled by settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class OptionalAttribute : Attribute {
        public string Option;
        public OptionalAttribute(string option) => Option = option;
    }

    /// <summary>
    /// like <see cref="CustomizablePropertyAttribute"/> but for methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomizableActionAttribute: Attribute {
        public CustomizableActionAttribute(string name) : this(name, null) { }
        public CustomizableActionAttribute(string name, string group) {
            this.name = name;
            this.group = group;
        }
        public string name { get; set; }
        public string group { get; set; }

    }
}
