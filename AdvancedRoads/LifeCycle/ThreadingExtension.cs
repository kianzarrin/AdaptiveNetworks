namespace AdvancedRoads.LifeCycle {
    using ICities;
    using AdvancedRoads.Tool;

    public class ThreadingExtension : ThreadingExtensionBase{
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            var tool = ToolsModifierControl.toolController?.CurrentTool;
            bool flag = tool == null || tool is AdvancedRoadsTool ||
                tool.GetType() == typeof(DefaultTool) || tool is NetTool || tool is BuildingTool;
            if (flag && AdvancedRoadsTool.ActivationShortcut.IsKeyUp()) {
                AdvancedRoadsTool.Instance.ToggleTool();
            }
        }
    }
}
