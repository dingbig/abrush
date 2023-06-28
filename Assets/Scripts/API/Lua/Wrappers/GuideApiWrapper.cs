﻿using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;

namespace TiltBrush
{
    [LuaDocsDescription("A guide widget")]
    [MoonSharpUserData]
    public class GuideApiWrapper
    {
        public StencilWidget _StencilWidget;

        public GuideApiWrapper(StencilWidget widget)
        {
            _StencilWidget = widget;
        }

        [LuaDocsDescription("The index of the active widget")]
        public int index => WidgetManager.m_Instance.GetActiveWidgetIndex(_StencilWidget);

        [LuaDocsDescription("Returns a string representation of the GuideApiWrapper")]
        [LuaDocsReturnValue("A string representation of the GuideApiWrapper")]
        public override string ToString()
        {
            return $"Guide({_StencilWidget})";
        }

        [LuaDocsDescription("The transform of the Guide Widget")]
        public TrTransform transform
        {
            get => App.Scene.MainCanvas.AsCanvas[_StencilWidget.transform];
            set
            {
                value = App.Scene.Pose * value;
                App.Scene.ActiveCanvas.AsCanvas[_StencilWidget.transform] = value;
            }
        }

        [LuaDocsDescription("The 3D position of the Guide Widget")]
        public Vector3 position
        {
            get => transform.translation;
            set
            {
                var tr_CS = transform;
                var newTransform = TrTransform.T(value);
                newTransform = App.Scene.Pose * newTransform;
                tr_CS.translation = newTransform.translation;
                transform = tr_CS;
            }
        }

        [LuaDocsDescription("The 3D orientation of the Guide Widget")]
        public Quaternion rotation
        {
            get => transform.rotation;
            set
            {
                var tr_CS = transform;
                var newTransform = TrTransform.R(value);
                newTransform = App.Scene.Pose * newTransform;
                tr_CS.rotation = newTransform.rotation;
                transform = tr_CS;
            }
        }

        [LuaDocsDescription("The scale of the Guide Widget")]
        public float scale
        {
            get => transform.scale;
            set
            {
                var tr_CS = transform;
                var newTransform = TrTransform.S(value);
                newTransform = App.Scene.Pose * newTransform;
                tr_CS.scale = newTransform.scale;
                transform = tr_CS;
            }
        }

        [LuaDocsDescription("Creates a new GuideApiWrapper with a cube stencil")]
        [LuaDocsParameter("transform", "The transform of the Guide Widget")]
        [LuaDocsReturnValue("A new GuideApiWrapper with a cube stencil")]
        public static GuideApiWrapper NewCube(TrTransform transform) => _Add(StencilType.Cube, transform);

        [LuaDocsDescription("Creates a new GuideApiWrapper with a sphere stencil")]
        [LuaDocsParameter("transform", "The transform of the Guide Widget")]
        [LuaDocsReturnValue("A new GuideApiWrapper with a sphere stencil")]
        public static GuideApiWrapper NewSphere(TrTransform transform) => _Add(StencilType.Sphere, transform);

        [LuaDocsDescription("Creates a new GuideApiWrapper with a capsule stencil")]
        [LuaDocsParameter("transform", "The transform of the Guide Widget")]
        [LuaDocsReturnValue("A new GuideApiWrapper with a capsule stencil")]
        public static GuideApiWrapper NewCapsule(TrTransform transform) => _Add(StencilType.Capsule, transform);

        [LuaDocsDescription("Creates a new GuideApiWrapper with a cone stencil")]
        [LuaDocsParameter("transform", "The transform of the Guide Widget")]
        [LuaDocsReturnValue("A new GuideApiWrapper with a cone stencil")]
        public static GuideApiWrapper NewCone(TrTransform transform) => _Add(StencilType.Cone, transform);

        [LuaDocsDescription("Creates a new GuideApiWrapper with an ellipsoid stencil")]
        [LuaDocsParameter("transform", "The transform of the Guide Widget")]
        [LuaDocsReturnValue("A new GuideApiWrapper with an ellipsoid stencil")]
        public static GuideApiWrapper NewEllipsoid(TrTransform transform) => _Add(StencilType.Ellipsoid, transform);

        [LuaDocsDescription("Creates a new GuideApiWrapper with a custom stencil")]
        [LuaDocsParameter("transform", "The transform of the Guide Widget")]
        [LuaDocsParameter("model", "The ModelApiWrapper to use for the custom stencil")]
        [LuaDocsReturnValue("A new GuideApiWrapper with a custom stencil")]
        public static GuideApiWrapper NewCustom(TrTransform transform, ModelApiWrapper model)
        {
            var guide = _Add(StencilType.Custom, transform);
            var customGuide = guide._StencilWidget as CustomStencil;
            if (customGuide == null) return null;
            customGuide.SetCustomStencil(model._ModelWidget.Model.GetMeshes().First().mesh);
            customGuide.SetColliderScale(model._ModelWidget.InitSize_CS);
            return guide;
        }

        [LuaDocsDescription("Selects the Guide Widget")]
        public void Select() => ApiMethods.SelectWidget(_StencilWidget);

        [LuaDocsDescription("Deletes the Guide Widget")]
        public void Delete() => ApiMethods.DeleteWidget(_StencilWidget);

        [LuaDocsDescription("Scales the Guide Widget")]
        [LuaDocsParameter("scale", "The scale vector for scaling the Guide Widget")]
        public void Scale(Vector3 scale) => SketchMemoryScript.m_Instance.PerformAndRecordCommand(
            new MoveWidgetCommand(_StencilWidget, _StencilWidget.LocalTransform, scale));

        private static GuideApiWrapper _Add(StencilType type, TrTransform tr)
        {
            CreateWidgetCommand createCommand = new CreateWidgetCommand(
                WidgetManager.m_Instance.GetStencilPrefab(type), tr, forceTransform: true);
            SketchMemoryScript.m_Instance.PerformAndRecordCommand(createCommand);
            var widget = createCommand.Widget as StencilWidget;
            return new GuideApiWrapper(widget);
        }
    }
}