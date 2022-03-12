using AdaptiveRoads.Manager;
using AdaptiveRoads.Patches.Lane;
using ICities;
using KianCommons;
using System.Diagnostics;

namespace AdaptiveRoads.LifeCycle {
    public class ThreadingExtensions : ThreadingExtensionBase {
        public override void OnAfterSimulationTick() {
            base.OnBeforeSimulationTick();
            NetworkExtensionManager.Instance.SimulationStep();
        }

#if DEBUG
        public static Stopwatch timer;
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            timer ??= Stopwatch.StartNew();
            if (timer.ElapsedMilliseconds > 1000) {
                Handle(CheckPropFlagsCommons.timer, "propcheck");
                Handle(CheckPropFlagsCommons.timer2, "propcheck2");
                Handle(CheckPropFlagsCommons.timer3, "propcheck3");
                timer.Restart();
            }
        }

        public static void Handle(Stopwatch sw, string name) {
            Log.Debug($"{name} = %{100 * sw.ElapsedMilliseconds / timer.ElapsedMilliseconds}", false);
            sw.Reset();
        }
#endif
    }

    public static class TimerExtensions {
        public static void Restart(this Stopwatch sw) {
            sw.Reset();
            sw.Start();
        }
    }
}
