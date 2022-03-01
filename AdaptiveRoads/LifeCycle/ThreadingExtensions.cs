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
            var ns_total = timer.ElapsedMilliseconds;
            if (ns_total > 1) {
                var timer_propcheck = CheckPropFlagsCommons.timer;
                var ns_propcheck = timer_propcheck.ElapsedMilliseconds;

                Log.Debug($"propcheck = %{100 * ns_propcheck / ns_total}");

                timer.Restart();
                timer_propcheck.Reset();
            }
        }
    }
#endif

    public static class TimerExtensions {
        public static void Restart(this Stopwatch sw) {
            sw.Reset();
            sw.Start();
        }
    }
}
