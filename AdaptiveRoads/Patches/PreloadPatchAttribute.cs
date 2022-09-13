namespace AdaptiveRoads.Patches {
    using System;

    // mark patches that should be applied in preload stage (both for game and asset editor)
    // there is no way to distinguish and there is no need for it yet.
    public class PreloadPatchAttribute : Attribute { }
}
