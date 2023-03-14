﻿// Copyright 2022 The Tilt Brush Authors
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

namespace TiltBrush
{
    public class ScriptsPanel : BasePanel
    {

        public BaseButton SymmetryScriptButton;
        public BaseButton PointerScriptButton;
        public BaseButton ToolScriptButton;

        public void InitScriptUiNav()
        {
            foreach (var nav in GetComponentsInChildren<ScriptUiNav>())
            {
                nav.Init();
            }
        }

        public void TogglePointerScript(ToggleButton btn)
        {
            LuaManager.Instance.EnablePointerScript(btn.m_IsToggledOn);
        }

        public void ConfigureScriptButton(LuaManager.ApiCategory category, string scriptName, string description)
        {
            BaseButton btn = category switch {
                LuaManager.ApiCategory.PointerScript => PointerScriptButton,
                LuaManager.ApiCategory.ToolScript => ToolScriptButton,
                LuaManager.ApiCategory.SymmetryScript => SymmetryScriptButton,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
            btn.SetDescriptionText($"{category}: {scriptName}");
            if (description != null)
            {
                btn.SetExtraDescriptionText(description);
            }
        }

        public void HandleGoogleDriveSync()
        {
            if (!App.DriveSync.IsFolderOfTypeSynced(DriveSync.SyncedFolderType.Scripts))
            {
                App.DriveSync.ToggleSyncOnFolderOfType(DriveSync.SyncedFolderType.Scripts);
            }
            App.DriveSync.SyncLocalFilesAsync().AsAsyncVoid();
        }
    }
}
