namespace AdaptiveRoads {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AdaptiveRoads.Manager;

    public static class API {
        public static bool IsAdaptive(this NetInfo info) =>
            NetInfoExtionsion.IsAdaptive(info);
    }
}
