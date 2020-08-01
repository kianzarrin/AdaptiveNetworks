using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using UnityEngine;

namespace AdvancedRoads.UI.MainPanel {
    public class BackButton: UIButton {
        public static BackButton Instance { get; private set; }

        public static BackButton Instace { get; private set; }

        public static string AtlasName = "BackButtonUI_rev" +
            typeof(BackButton).Assembly.GetName().Version.Revision;
        const int SIZE = 40;
        const string CONTAINING_PANEL_NAME = "RoadsOptionPanel";
        readonly static Vector2 RELATIVE_POSITION = new Vector2(94, 38);

        const string BackButtonBg = "BackButtonBg";
        const string BackButtonBgPressed = "BackButtonBgPressed";
        const string BackButtonBgHovered = "BackButtonBgHovered";
        const string BackIcon = "BackIcon";

        public override void Awake() {
            base.Awake();
            Log.Debug("BackButton.Awake() is called.");
            name = nameof(BackButton);
            size = new Vector2(SIZE, SIZE);
            Instace = this;

        }

        public override void Start() {
            base.Start();
            Log.Info("BackButton.Start() is called.");

            playAudioEvents = true;
            tooltip = "Back";

            string[] spriteNames = new string[]
            {
                BackButtonBg,
                BackButtonBgHovered,
                BackButtonBgPressed,
                BackIcon,
            };

            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas("back.png", AtlasName, SIZE, SIZE, spriteNames);
            }

            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;

            hoveredBgSprite = BackButtonBgHovered;
            pressedBgSprite = BackButtonBgPressed;
            normalBgSprite = focusedBgSprite = disabledBgSprite = BackButtonBg;
            normalFgSprite = focusedFgSprite = disabledFgSprite = hoveredFgSprite = pressedFgSprite = BackIcon;

            Show();
            Unfocus();
            Invalidate();
            Log.Info("BackButton created sucessfully.");
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("ON CLICK CALLED");
            base.OnClick(p);
            MainPanel.Instance.Back();
        }
    }
}
