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
}
