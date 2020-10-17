using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.LifeCycle {
    public class ThreadingExtensions : ThreadingExtensionBase {
        public override void OnBeforeSimulationTick() {
            base.OnBeforeSimulationTick();
            NetworkExtensionManager.Instance.SimulationStep();
        }
    }
}
