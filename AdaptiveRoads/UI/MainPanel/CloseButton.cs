using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using UnityEngine;

namespace AdvancedRoads.UI.MainPanel {
    public class CloseButton: UIButton {
        public static CloseButton Instance { get; private set; }

        public static CloseButton Instace { get; private set; }

        public static string AtlasName = "CloseButtonUI_rev" +
            typeof(CloseButton).Assembly.GetName().Version.Revision;
        const int SIZE = 40;
        const string CONTAINING_PANEL_NAME = "RoadsOptionPanel";
        readonly static Vector2 RELATIVE_POSITION = new Vector2(94, 38);

        const string CloseButtonBg = "CloseButtonBg";
        const string CloseButtonBgPressed = "CloseButtonBgPressed";
        const string CloseButtonBgHovered = "CloseButtonBgHovered";
        const string CloseIcon = "CloseIcon";

        public override void Awake() {
            base.Awake();
            Log.Debug("CloseButton.Awake() is called.");
            name = nameof(CloseButton);
            size = new Vector2(SIZE, SIZE);
            Instace = this;
        }

        public override void Start() {
            base.Start();
            Log.Info("CloseButton.Start() is called.");

            playAudioEvents = true;
            tooltip = "Close";

            string[] spriteNames = new string[]
            {
                CloseButtonBg,
                CloseButtonBgHovered,
                CloseButtonBgPressed,
                CloseIcon,
            };

            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas("close.png", AtlasName, SIZE, SIZE, spriteNames);
            }

            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;

            hoveredBgSprite = CloseButtonBgHovered;
            pressedBgSprite = CloseButtonBgPressed;
            normalBgSprite = focusedBgSprite = disabledBgSprite = CloseButtonBg;
            normalFgSprite = focusedFgSprite = disabledFgSprite = hoveredFgSprite = pressedFgSprite = CloseIcon;

            Show();
            Unfocus();
            Invalidate();
            Log.Info("CloseButton created sucessfully.");
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("ON CLICK CALLED");
            base.OnClick(p);
            MainPanel.Instance.Close();
        }
    }
}
