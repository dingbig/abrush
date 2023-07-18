﻿using MoonSharp.Interpreter;
using UnityEngine;
namespace TiltBrush
{
    [LuaDocsDescription("Represents the user's wand (the controller that isn't the brush controller)")]
    [MoonSharpUserData]
    public static class WandApiWrapper
    {
        [LuaDocsDescription("The 3D position of the Wand Controller")]
        public static Vector3 position => LuaManager.Instance.GetPastWandPos(0);

        [LuaDocsDescription("The 3D orientation of the Wand")]
        public static Quaternion rotation => LuaManager.Instance.GetPastWandRot(0);

        [LuaDocsDescription("The vector representing the forward direction of the wand controller")]
        public static Vector3 direction => LuaManager.Instance.GetPastWandRot(0) * Vector3.forward;

        [LuaDocsDescription("How far the trigger on the wand contrller is pressed in")]
        public static float pressure => InputManager.Wand.GetTriggerValue();

        [LuaDocsDescription("How fast the wand contrller is currently moving")]
        public static Vector3 speed => InputManager.Wand.m_Velocity;

        [LuaDocsDescription("Check whether the wand trigger is currently pressed")]
        public static bool triggerIsPressed => InputManager.m_Instance.GetCommand(InputManager.SketchCommands.AltActivate);

        [LuaDocsDescription("Check whether the wand trigger was pressed during the current frame")]
        public static bool triggerPressedThisFrame => InputManager.m_Instance.GetCommandDown(InputManager.SketchCommands.AltActivate);

        // [LuaDocsDescription("Check whether the wand trigger was released during the current frame")]
        // public static bool triggerReleasedThisFrame => SketchSurfacePanel.m_Instance.ActiveTool.BecameInactiveThisFrame;

        [LuaDocsDescription("Clears the history and sets it's size")]
        [LuaDocsExample("Wand.ResizeHistory(100)")]
        [LuaDocsParameter("size", "The size of the history buffer")]
        public static void ResizeHistory(int size) => LuaManager.Instance.ResizeWandBuffer(size);

        [LuaDocsDescription("Sets the size of the history. Only clears it if the size has changed")]
        [LuaDocsExample("Wand.SetHistorySize(100)")]
        [LuaDocsParameter("size", "The size of the history buffer")]
        public static void SetHistorySize(int size) => LuaManager.Instance.SetWandBufferSize(size);

        [LuaDocsDescription("Recalls previous positions of the Wand from the history buffer")]
        [LuaDocsExample("myPosition = Wand.PastPosition(5)")]
        [LuaDocsParameter("back", "How far back in the history to get the position from")]
        public static Vector3 PastPosition(int back) => LuaManager.Instance.GetPastWandPos(back);

        [LuaDocsDescription("Recalls previous orientations of the Wand from the history buffer")]
        [LuaDocsExample("myRotation = Wand.PastRotation(5)")]
        [LuaDocsParameter("back", "How far back in the history to get the rotation from")]
        public static Quaternion PastRotation(int back) => LuaManager.Instance.GetPastWandRot(back);
    }
}
