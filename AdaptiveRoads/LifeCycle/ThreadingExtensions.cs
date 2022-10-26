using AdaptiveRoads.Data.NetworkExtensions;
using AdaptiveRoads.Manager;
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
            //return;
            timer ??= Stopwatch.StartNew();
            if (timer.ElapsedMilliseconds > 1000) {
                //Handle(CheckPropFlagsCommons.timer, "propcheck");
                //Handle(CheckPropFlagsCommons.timer2, "propcheck2");
                //Handle(CheckPropFlagsCommons.timer3, "propcheck3");
                Handle2(OutlineData.timer1, OutlineData.counter1, "lane-outline");
                Handle2(OutlineData.timer2, OutlineData.counter2, "transition-outline");
                timer.Restart();
            }
        }

        public static void Handle(Stopwatch sw, string name) {
            Log.Debug($"{name} = %{100 * sw.ElapsedMilliseconds / timer.ElapsedMilliseconds}", false);
            sw.Reset();
        }
        public static void Handle2(Stopwatch sw, int count, string name) {
            long ms = sw.ElapsedMilliseconds;
            float perTime = ms / (float)count;
            Log.Debug($"{name} : {count} times => {ms}ms - {perTime} per iteration", false);
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
