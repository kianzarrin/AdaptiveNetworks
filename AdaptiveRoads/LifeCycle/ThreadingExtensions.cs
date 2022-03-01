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

        public static Stopwatch timer;
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            timer ??= Stopwatch.StartNew();
            var ns_total = timer.ElapsedMilliseconds;
            if (ns_total > 500) {
                var timer_propcheck = CheckPropFlagsCommons.timer;
                var timer_propcheck2 = NetInfoExtionsion.LaneProp.timer;
                var ns_propcheck = timer_propcheck.ElapsedMilliseconds;
                var ns_propcheck2 = timer_propcheck2.ElapsedMilliseconds;

                Log.Debug($"propcheck = %{100 * ns_propcheck / ns_total}");
                Log.Debug($"propcheck2 = %{100 * ns_propcheck2 / ns_total}");

                timer.Restart();
                timer_propcheck.Reset();
                timer_propcheck2.Reset();
            }

        }
    }
    public static class TimerExtensions {
        public static void Restart(this Stopwatch sw) {
            sw.Reset();
            sw.Start();
        }
    }
}
