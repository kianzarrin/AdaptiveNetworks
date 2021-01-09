using System;
using HarmonyLib;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace experiments {
    class Program {
        static void Main(string[] args)
        {
            Harmony harmony = new Harmony("Kian.Test");
            harmony.PatchAll();
        }
    }


    [HarmonyPatch(typeof(RoadEditorCollapsiblePanel))]
    internal static class RoadEditorCollapsiblePanelPatch {
        [HarmonyPrefix]
        [HarmonyPatch("OnButtonClick")]
        static bool OnButtonClickPrefix(UIComponent component)
        {
            Console.WriteLine("OnButtonClickPrefix() is called");
            return false;
        }

        static void OnButtonControlClick(UIComponent component)
        {
            Console.WriteLine("OnButtonControlClick() is called");
        }

        public static void DisplaceAllProps(NetInfo.Lane[] lanes)
        {
            Console.WriteLine("DisplaceAllProps() is called");
        }

        public static void DisplaceAll(IEnumerable<NetLaneProps.Prop> props)
        {
            Console.WriteLine("DisplaceAllProps() is called");

        }

        public static void DisplaceAll(IEnumerable<NetLaneProps.Prop> props, int z)
        {
            Console.WriteLine("DisplaceAll() is called");
        }

        static void ClearAll(RoadEditorCollapsiblePanel instance)
        {
            Console.WriteLine("ClearAll() is called");

        }

        static void PasteAll(RoadEditorCollapsiblePanel groupPanel)
        {
            Console.WriteLine("PasteAll() is called");
        }
    }
}