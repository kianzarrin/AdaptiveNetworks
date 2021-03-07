namespace AdaptiveRoads.LifeCycle
{
    using ICities;
    using KianCommons;

    public class LoadingExtention : LoadingExtensionBase
    {
        // TMPE on level loaded calls LifeCycle.Load
        //public override void OnLevelLoaded(LoadMode mode)
        //{
        //    Log.Debug("LoadingExtention.OnLevelLoaded");
        //    // if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario)
        //        LifeCycle.Load();
        //}

        public override void OnLevelUnloading()
        {
            Log.Debug("LoadingExtention.OnLevelUnloading");
            LifeCycle.Unload();
        }
    }
}
