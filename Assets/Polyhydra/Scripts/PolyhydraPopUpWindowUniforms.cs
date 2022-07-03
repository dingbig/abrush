﻿// Copyright 2022 The Open Brush Authors
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Polyhydra.Wythoff;
using TiltBrush.MeshEditing;
using UnityEngine;


namespace TiltBrush
{

    public class PolyhydraPopUpWindowUniforms : PolyhydraPopUpWindowBase
    {

        protected override List<string> GetButtonList()
        {
            return ParentPanel.GetUniformPolyNames();
        }

        public override Texture2D GetButtonTexture(string action)
        {
            return ParentPanel.GetButtonTexture(PolyhydraButtonTypes.UniformType, action);
        }

        public override void HandleButtonPress(string action)
        {
            string enumName = action.Replace(" ", "_");
            UniformTypes polyType = (UniformTypes)Enum.Parse(typeof(UniformTypes), enumName, true);
            ParentPanel.CurrentPolyhedra.UniformPolyType = polyType;
            ParentPanel.SetButtonTextAndIcon(PolyhydraButtonTypes.UniformType, action);
            ParentPanel.SetSliderConfiguration();
        }
    }
} // namespace TiltBrush