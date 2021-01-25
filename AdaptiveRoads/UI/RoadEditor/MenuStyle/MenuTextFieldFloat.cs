namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class MenuTextFieldFloat : MenuTextFieldInt {
        public override void Awake() {
            base.Awake();
            allowFloats = true;
        }
        new public float Number {
            get {
                if(float.TryParse(text, out float ret))
                    return ret;
                return 0;
            }
        }
    }
}

