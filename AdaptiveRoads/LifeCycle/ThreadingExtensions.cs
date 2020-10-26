using AdaptiveRoads.Manager;
using ICities;

namespace AdaptiveRoads.LifeCycle {
    public class ThreadingExtensions : ThreadingExtensionBase {
        public override void OnBeforeSimulationTick() {
            base.OnBeforeSimulationTick();
            NetworkExtensionManager.Instance.SimulationStep();
        }
    }
}
