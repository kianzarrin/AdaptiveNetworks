using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdaptiveRoads.UI.RoadEditor {
    public interface IDataUI: IHint {
        void Refresh();
    }
    public interface IHint {
        string GetHint();
        bool IsHovered();
    }
}
