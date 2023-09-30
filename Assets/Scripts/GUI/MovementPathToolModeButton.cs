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

namespace TiltBrush
{

    public class MovementPathToolModeButton : BaseButton
    {
        [SerializeField] private MovementPathTool.Mode m_Mode;

        override protected void Awake()
        {
            base.Awake();
            App.Switchboard.ToolChanged += UpdateVisuals;
        }

        override protected void OnDestroy()
        {
            base.OnDestroy();
            App.Switchboard.ToolChanged -= UpdateVisuals;
        }

        override public void UpdateVisuals()
        {
            base.UpdateVisuals();

            // Availability visuals.
            if (m_Mode != MovementPathTool.Mode.AddPositionKnot)
            {
                bool wasAvailable = IsAvailable();
                bool available = WidgetManager.m_Instance.AnyActivePathHasAKnot();
                if (wasAvailable != available)
                {
                    SetButtonAvailable(available);
                }
            }

            // Activated visuals.
            bool bWasToggleActive = m_ToggleActive;
            m_ToggleActive = false;
            MovementPathTool cpt = SketchSurfacePanel.m_Instance.ActiveTool as MovementPathTool;
            if (cpt != null)
            {
                m_ToggleActive = cpt.CurrentMode == m_Mode;
            }
            if (bWasToggleActive != m_ToggleActive)
            {
                SetButtonActivated(m_ToggleActive);
            }
        }

        override protected void OnButtonPressed()
        {

            if (m_ToggleActive)
            {
                SketchSurfacePanel.m_Instance.EnableDefaultTool();
            }
            else
            {
                WidgetManager.m_Instance.CameraPathsVisible = true;
                SketchControlsScript.m_Instance.EatGazeObjectInput();
                SketchSurfacePanel.m_Instance.RequestHideActiveTool(true);
                SketchSurfacePanel.m_Instance.EnableSpecificTool(BaseTool.ToolType.MovementPathTool);
                App.Switchboard.TriggerCameraPathModeChanged(m_Mode);
            }
        }
    }
} // namespace TiltBrush
