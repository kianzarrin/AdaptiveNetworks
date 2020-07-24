using ColossalFramework.UI;
using AdvancedRoads.Tool;
using AdvancedRoads.Util;
using System;
using System.Linq;
using UnityEngine;
using static AdvancedRoads.Util.HelpersExtensions;

/* A lot of copy-pasting from Crossings mod by Spectra and Roundabout Mod by Strad. The sprites are partly copied as well. */

namespace AdvancedRoads.GUI {
    public class AdvancedRoadsButton : UIButton {
        public static AdvancedRoadsButton Instace { get; private set;}

        public static string AtlasName = "AdvancedRoadsButtonUI_rev" +
            typeof(AdvancedRoadsButton).Assembly.GetName().Version.Revision;
        const int SIZE = 31;
        const string CONTAINING_PANEL_NAME = "RoadsOptionPanel";
        readonly static Vector2 RELATIVE_POSITION = new Vector3(94, 38);

        const string AdvancedRoadsButtonBg = "AdvancedRoadsButtonBg";
        const string AdvancedRoadsButtonBgActive = "AdvancedRoadsButtonBgFocused";
        const string AdvancedRoadsButtonBgHovered = "AdvancedRoadsButtonBgHovered";
        internal const string AdvancedRoadsIcon = "AdvancedRoadsIcon";
        internal const string AdvancedRoadsIconActive = "AdvancedRoadsIconPressed";

        static UIComponent GetContainingPanel() {
            var ret = GUI.UIUtils.Instance.FindComponent<UIComponent>(CONTAINING_PANEL_NAME, null, GUI.UIUtils.FindOptions.NameContains);
            Log.Debug("GetPanel returns " + ret);
            return ret ?? throw new Exception("Could not find " + CONTAINING_PANEL_NAME);
        }

        public override void Awake() {
            base.Awake();
            Log.Debug("AdvancedRoadsButton.Awake() is called." + Environment.StackTrace);
        }

        public override void Start() {
            base.Start();
            Log.Info("AdvancedRoadsButton.Start() is called.");

            name = "AdvancedRoadsButton";
            playAudioEvents = true;
            tooltip = "Node Controller";

            var builtinTabstrip = GUI.UIUtils.Instance.FindComponent<UITabstrip>("ToolMode", GetContainingPanel(), GUI.UIUtils.FindOptions.None);
            AssertNotNull(builtinTabstrip, "builtinTabstrip");

            UIButton tabButton = (UIButton)builtinTabstrip.tabs[0];

            string[] spriteNames = new string[]
            {
                AdvancedRoadsButtonBg,
                AdvancedRoadsButtonBgActive,
                AdvancedRoadsButtonBgHovered,
                AdvancedRoadsIcon,
                AdvancedRoadsIconActive
            };

            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas("sprites.png", AtlasName, SIZE, SIZE, spriteNames);
            }

            Log.Debug("atlas name is: " + atlas.name);
            this.atlas = atlas;

            Deactivate();
            hoveredBgSprite = AdvancedRoadsButtonBgHovered;


            relativePosition = RELATIVE_POSITION;
            size = new Vector2(SIZE, SIZE); 
            Show();
            Log.Info("AdvancedRoadsButton created sucessfully.");
            Unfocus();
            Invalidate();
            //if (parent.name == "RoadsOptionPanel(RoadOptions)") {
            //    Destroy(Instace); // destroy old instance after cloning
            //}
            Instace = this;
        }

        public void Activate() {
            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = AdvancedRoadsButtonBgActive;
            normalFgSprite = focusedFgSprite = AdvancedRoadsIconActive;
            Invalidate();
        }

        public void Deactivate() {
            focusedFgSprite = normalBgSprite = pressedBgSprite = disabledBgSprite = AdvancedRoadsButtonBg;
            normalFgSprite = focusedFgSprite = AdvancedRoadsIcon;
            Invalidate();
        }


        public static AdvancedRoadsButton CreateButton() { 
            Log.Info("AdvancedRoadsButton.CreateButton() called");
            return GetContainingPanel().AddUIComponent<AdvancedRoadsButton>();
        }

        protected override void OnClick(UIMouseEventParameter p) {
            Log.Debug("ON CLICK CALLED" + Environment.StackTrace);
            var buttons = UIUtils.GetCompenentsWithName<UIComponent>(name);
            Log.Debug(buttons.ToSTR());

            base.OnClick(p); 
            AdvancedRoadsTool.Instance.ToggleTool();
        }

        public override void OnDestroy() {
            base.OnDestroy();
        }

        public override string ToString() => $"AdvancedRoadsButton:|name={name} parent={parent.name}|";


    }
}
