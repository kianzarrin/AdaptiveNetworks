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
            var ms_total = timer.ElapsedMilliseconds;
            if (ms_total > 1000) {
                var timer_propcheck = CheckPropFlagsCommons.timer;
                var timer_propcheck2 = CheckPropFlagsCommons.timer2;
                var ms_propcheck = timer_propcheck.ElapsedMilliseconds;
                var ms_propcheck2 = timer_propcheck2.ElapsedMilliseconds;

                Log.Debug($"propcheck = %{100 * ms_propcheck / ms_total}", false);
                Log.Debug($"propcheck2 = %{100 * ms_propcheck2 / ms_total}", false);

                timer.Restart();
                timer_propcheck.Reset();
                timer_propcheck2.Reset();
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
