namespace AdaptiveRoads.Patches.RoadEditor {
    using AdaptiveRoads.Patches.Lane;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Patches;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using static ColossalFramework.UI.UIInput;
    using ColossalFramework.UI;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using System.Reflection.Emit;
    using ColossalFramework;
    using System.Linq;

    /// <summary>
    /// fixes OnDragOver
    /// </summary>
    [HarmonyPatch(typeof(MouseHandler), nameof(MouseHandler.ProcessInput))]
    public static class ProcessInputFix {
        static MethodInfo mDistance = GetMethod(typeof(Vector2), nameof(Vector2.Distance));
        static MethodInfo mMagnitude = GetMethod(typeof(Vector2), "get_magnitude");
        static FieldInfo f_MouseMoveDelta = GetField(typeof(MouseHandler), "m_MouseMoveDelta");
        static ConstructorInfo cUIDragEventParameter = typeof(UIDragEventParameter)
            .GetConstructors(ALL).Single(c => c.GetParameters().Length == 4);


        static void Postfix(
            ref  UIDragDropState ___m_DragState,
            ref UIComponent ___m_ActiveComponent,
            UIMouseButton ___m_ButtonsUp,
            object ___m_DragData) {
            try {
                if(!___m_ActiveComponent) { // if during draggin, component dies.
                    ___m_DragState = UIDragDropState.None;
                    ___m_ActiveComponent = null;
                }
                if(___m_DragState == UIDragDropState.Dragging &&
                   ___m_ButtonsUp.IsFlagSet(UIMouseButton.Right)) {
                    var p = cUIDragEventParameter.Invoke(new[] {
                        ___m_ActiveComponent,
                        UIDragDropState.Cancelled,
                        ___m_DragData,
                        (Vector2)Input.mousePosition });
                    GetMethod(typeof(UIComponent), "OnDragEnd")
                        .Invoke(___m_ActiveComponent, new object[] { p });
                    ___m_DragState = UIDragDropState.None;
                    ___m_ActiveComponent = null;
                }
            } catch(Exception ex) {
                Log.Exception(ex);
            }
        }

        static Exception Finalize(Exception __exception) {
            if(__exception != null)
                Log.Exception(__exception);
            return null;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            try {
                var codes = instructions.ToCodeList();
                int iCallDisance = codes.Search(c => c.Calls(mDistance));
                int index = codes.Search(c => c.IsLdLoc(0), startIndex:iCallDisance, count:-1);

                /*
                 * remove the following:
                 * ldloc.0
                 * ldarg.0
                 * ldflda m_LastPosition
                 * Call Distance
                 * ldc 
                 */
                codes.RemoveRange(index, 5);

                /*
                 * problems:
                 *  m_LastPosition == mousePosition at this point
                 *  also '> 1f' does not detect slow mouse movements
                 * - Vector2.Distance(mousePosition, this.m_LastPosition) > 1f
                 * + this.m_MouseMoveDelta.magnitude > 0f
                 */
                codes.InsertInstructions(index, new[] {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldflda, f_MouseMoveDelta),
                    new CodeInstruction(OpCodes.Call, mMagnitude),
                    new CodeInstruction(OpCodes.Ldc_R4, 0f),

                });

                return codes;
            }catch(Exception ex) {
                Log.Exception(ex);
                return instructions;
            }
        }
    }
}
