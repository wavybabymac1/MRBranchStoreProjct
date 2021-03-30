/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.
​
Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/


using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Imports the latest version of the Windows Mixed Reality package
/// This works around an issue where this dependency is sometimes not updated correctly by Unity
/// </summary>
[InitializeOnLoad]
public static class MagicLeapAssetsChecker
{
    const string ML_DEFINE_SYMBOL = "ML_ASSETS_IMPORTED";
    
    static readonly string ML_VERSION_SCRIPT = Path.Combine(Application.dataPath, "MagicLeap/Scripts/MLVersion.cs");

    static MagicLeapAssetsChecker()
    {
#if !ML_ASSETS_IMPORTED
        if (File.Exists(ML_VERSION_SCRIPT))
            AddScriptingDefineSymbol();
        else
            Debug.LogWarning("Magic Leap assets not found in project. Please import the appropriate version of " +
                             "the Magic Leap Unity Package to build this project correctly.");
#endif
    }

    static void AddScriptingDefineSymbol()
    {
        var existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Lumin);
        if (existingDefines.Contains(ML_DEFINE_SYMBOL))
            return;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            BuildTargetGroup.Lumin,
            existingDefines + ";" + ML_DEFINE_SYMBOL);

        existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        if (existingDefines.Contains(ML_DEFINE_SYMBOL))
            return;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            BuildTargetGroup.Standalone,
            existingDefines + ";" + ML_DEFINE_SYMBOL);
    }
}
