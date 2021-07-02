﻿// Copyright 2020 The Tilt Brush Authors
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

using System;
using UnityEngine;

namespace TiltBrush
{
    public class BrushEditorTexturePickerButton : OptionButton
    {
        [NonSerialized] public EditBrushPanel ParentPanel;
        [NonSerialized] public string TexturePropertyName;
        public int TextureIndex;
        private string m_TexturePath;
        [SerializeField] private GameObject[] m_ObjectsToHideBehindPopups;
        [NonSerialized] public BrushEditorTexturePopUpWindow popup;
        public string TexturePath
        {
            get => m_TexturePath;
            private set => m_TexturePath = value;
        }

        protected override void OnButtonPressed()
        {
            for (int i = 0; i < m_ObjectsToHideBehindPopups.Length; ++i)
            {
                m_ObjectsToHideBehindPopups[i].SetActive(false);
            }
            base.OnButtonPressed();
            
            popup = (BrushEditorTexturePopUpWindow) ParentPanel.PanelPopUp;
            popup.ActiveTextureIndex = TextureIndex;
            popup.OpenerButton = this;
            // TODO - match buttons to textures so we can make the right one active
            // Otherwise we need to fix the underlying problem where the popup can't access ParentPanel during Init
            // int buttonIndex = popup.
            // popup.SetActiveTextureButtonSelected(buttonIndex);

        }
        
        override public void GazeRatioChanged(float gazeRatio)
        {
            GetComponent<Renderer>().material.SetFloat("_Distance", gazeRatio);
        }

        public void UpdateValue(Texture2D tex, string propertyName, string texPath)
        {
            if (tex != null)
            {
                SetButtonTexture((Texture2D)tex);
                m_ButtonTexture = tex;
                GetComponent<MeshRenderer>().material.mainTexture = tex;
            }
            TexturePropertyName = propertyName;
            SetDescriptionText(propertyName);
            TexturePath = texPath;
        }
    }
} // namespace TiltBrush
