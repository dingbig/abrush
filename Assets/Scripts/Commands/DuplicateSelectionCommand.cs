// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TiltBrush
{
    public class DuplicateSelectionCommand : BaseCommand
    {
        // This command stores a copy of the selection and a copy of the duplicate.
        private List<Stroke> m_SelectedStrokes;
        private List<GrabWidget> m_SelectedWidgets;

        private List<Stroke> m_DuplicatedStrokes;
        private List<GrabWidget> m_DuplicatedWidgets;

        private TrTransform m_OriginTransform;
        private TrTransform m_DuplicateTransform;

        private CanvasScript m_CurrentCanvas;

        private bool m_DupeInPlace;

        public DuplicateSelectionCommand(TrTransform xf, BaseCommand parent = null) : base(parent)
        {
            // Save selected and duplicated strokes.
            m_SelectedStrokes = SelectionManager.m_Instance.SelectedStrokes.ToList();
            m_DuplicatedStrokes = new List<Stroke>();
            foreach (var stroke in m_SelectedStrokes)
            {
                if (PointerManager.m_Instance.CurrentSymmetryMode == PointerManager.SymmetryMode.FourAroundY)
                {
                    var matrices = PointerManager.m_Instance.CustomMirrorMatrices;
                    TrTransform strokeTransform = Coords.AsCanvas[stroke.StrokeTransform];
                    TrTransform tr;
                    var xfWidget = TrTransform.FromTransform(PointerManager.m_Instance.SymmetryWidget);
                    foreach (var m in matrices)
                    {
                        tr = PointerManager.m_Instance.TrFromMatrix(m);
                        tr = xfWidget * tr * xfWidget.inverse; // convert from widget-local coords to world coords
                        var tmp = tr; // * strokeTransform;       // Work around 2018.3.x Mono parse bug
                        tmp *= App.Scene.Pose;
                        tmp *= TrTransform.T(Vector3.one * (Random.value * .00001f)); // Small jitter to prevent z-fighting
                        var duplicatedStroke = SketchMemoryScript.m_Instance.DuplicateStroke(stroke, App.Scene.SelectionCanvas, tmp);
                        m_DuplicatedStrokes.Add(duplicatedStroke);
                    }
                }
                else
                {
                    var duplicatedStroke = SketchMemoryScript.m_Instance.DuplicateStroke(stroke, App.Scene.SelectionCanvas, null);
                    m_DuplicatedStrokes.Add(duplicatedStroke);
                }
            }

            // Save selected widgets.
            m_SelectedWidgets = SelectionManager.m_Instance.SelectedWidgets.ToList();
            // Save duplicated widgets
            m_DuplicatedWidgets = new List<GrabWidget>();
            foreach (var widget in m_SelectedWidgets)
            {
                if (PointerManager.m_Instance.CurrentSymmetryMode == PointerManager.SymmetryMode.FourAroundY)
                {
                    var matrices = PointerManager.m_Instance.CustomMirrorMatrices;
                    TrTransform widgetTransform = TrTransform.FromTransform(widget.transform);
                    TrTransform tr;
                    var xfWidget = TrTransform.FromTransform(PointerManager.m_Instance.SymmetryWidget);
                    foreach (var m in matrices)
                    {
                        var duplicatedWidget = widget.Clone();
                        tr = PointerManager.m_Instance.TrFromMatrix(m);
                        tr = xfWidget * tr * xfWidget.inverse; // convert from widget-local coords to world coords
                        var tmp = tr * widgetTransform; // Work around 2018.3.x Mono parse bug
                        // Preserve size but mirror if needed
                        duplicatedWidget.RecordAndSetSize(widget.GetSignedWidgetSize() * Mathf.Sign(tmp.scale));
                        duplicatedWidget.RecordAndSetPosRot(tmp);
                        m_DuplicatedWidgets.Add(duplicatedWidget);
                    }
                }
                else
                {
                    var duplicatedWidget = widget.Clone();
                    m_DuplicatedWidgets.Add(duplicatedWidget);
                }
            }

            m_CurrentCanvas = App.ActiveCanvas;

            GroupManager.MoveStrokesToNewGroups(m_DuplicatedStrokes, null);

            m_OriginTransform = SelectionManager.m_Instance.SelectionTransform;
            m_DuplicateTransform = xf;
            m_DupeInPlace = m_OriginTransform == m_DuplicateTransform;
        }

        public override bool NeedsSave { get { return true; } }

        protected override void OnRedo()
        {
            // Deselect selected strokes to current canvas.
            if (m_SelectedStrokes != null)
            {
                SelectionManager.m_Instance.DeselectStrokes(m_SelectedStrokes, m_CurrentCanvas);
            }

            // Deselect selected widgets.
            if (m_SelectedWidgets != null)
            {
                SelectionManager.m_Instance.DeselectWidgets(m_SelectedWidgets, m_CurrentCanvas);
            }

            // Place duplicated strokes.
            foreach (var stroke in m_DuplicatedStrokes)
            {
                switch (stroke.m_Type)
                {
                    case Stroke.Type.BrushStroke:
                        {
                            BaseBrushScript brushScript = stroke.m_Object.GetComponent<BaseBrushScript>();
                            if (brushScript)
                            {
                                brushScript.HideBrush(false);
                            }
                        }
                        break;
                    case Stroke.Type.BatchedBrushStroke:
                        {
                            stroke.m_BatchSubset.m_ParentBatch.EnableSubset(stroke.m_BatchSubset);
                        }
                        break;
                    default:
                        Debug.LogError("Unexpected: redo NotCreated duplicate stroke");
                        break;
                }
                TiltMeterScript.m_Instance.AdjustMeter(stroke, up: true);
            }
            SelectionManager.m_Instance.RegisterStrokesInSelectionCanvas(m_DuplicatedStrokes);

            // Place duplicated widgets.
            for (int i = 0; i < m_DuplicatedWidgets.Count; ++i)
            {
                m_DuplicatedWidgets[i].RestoreFromToss();
            }
            SelectionManager.m_Instance.RegisterWidgetsInSelectionCanvas(m_DuplicatedWidgets);

            // Set selection widget transforms.
            SelectionManager.m_Instance.SelectionTransform = m_DuplicateTransform;
            SelectionManager.m_Instance.UpdateSelectionWidget();
        }

        protected override void OnUndo()
        {
            // Remove duplicated strokes.
            foreach (var stroke in m_DuplicatedStrokes)
            {
                switch (stroke.m_Type)
                {
                    case Stroke.Type.BrushStroke:
                        {
                            BaseBrushScript brushScript = stroke.m_Object.GetComponent<BaseBrushScript>();
                            if (brushScript)
                            {
                                brushScript.HideBrush(true);
                            }
                        }
                        break;
                    case Stroke.Type.BatchedBrushStroke:
                        {
                            stroke.m_BatchSubset.m_ParentBatch.DisableSubset(stroke.m_BatchSubset);
                        }
                        break;
                    default:
                        Debug.LogError("Unexpected: undo NotCreated duplicate stroke");
                        break;
                }
                TiltMeterScript.m_Instance.AdjustMeter(stroke, up: false);
            }
            SelectionManager.m_Instance.DeregisterStrokesInSelectionCanvas(m_DuplicatedStrokes);

            // Remove duplicated widgets.
            for (int i = 0; i < m_DuplicatedWidgets.Count; ++i)
            {
                m_DuplicatedWidgets[i].Hide();
            }
            SelectionManager.m_Instance.DeregisterWidgetsInSelectionCanvas(m_DuplicatedWidgets);

            // Reset the selection transform before we select strokes.
            SelectionManager.m_Instance.SelectionTransform = m_OriginTransform;

            // Select strokes.
            if (m_SelectedStrokes != null)
            {
                SelectionManager.m_Instance.SelectStrokes(m_SelectedStrokes);
            }
            if (m_SelectedWidgets != null)
            {
                SelectionManager.m_Instance.SelectWidgets(m_SelectedWidgets);
            }

            SelectionManager.m_Instance.UpdateSelectionWidget();
        }

        public override bool Merge(BaseCommand other)
        {
            if (!m_DupeInPlace)
            {
                return false;
            }

            // If we duplicated a selection in place (the stamp feature), subsequent movements of
            // the selection should get bundled up with this command as a child.
            MoveWidgetCommand move = other as MoveWidgetCommand;
            if (move != null)
            {
                if (m_Children.Count == 0)
                {
                    m_Children.Add(other);
                }
                else
                {
                    MoveWidgetCommand childMove = m_Children[0] as MoveWidgetCommand;
                    Debug.Assert(childMove != null);
                    return childMove.Merge(other);
                }
                return true;
            }
            return false;
        }
    }
} // namespace TiltBrush
