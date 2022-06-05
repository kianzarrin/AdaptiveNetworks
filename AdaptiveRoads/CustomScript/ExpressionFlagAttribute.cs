namespace AdaptiveRoads.CustomScript {
    using System;
    using AdaptiveRoads.Manager;

    class ExpressionFlagAttribute : Attribute {
        public static ExpressionWrapper GetExpression(Enum flag, NetInfo netInfo) {
            var dict = netInfo?.GetMetaData()?.ScriptedFlags;
            if (dict != null && dict.TryGetValue(flag, out var ret))
                return ret;
            return null;
        }
    }
}
