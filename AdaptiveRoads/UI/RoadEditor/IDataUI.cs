using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdaptiveRoads.UI.RoadEditor {
    public interface IDataUI {
        string GetHint();
        void Refresh();
        bool IsHovered();
    }
}
