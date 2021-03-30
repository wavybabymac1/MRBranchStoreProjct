/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.UI;

public class Versions : MonoBehaviour
{
    public Text versionText;

    void Start()
    {
        var vuforiaVersion = Vuforia.VuforiaUnity.GetVuforiaLibraryVersion();
        var unityVersion = Application.unityVersion;
        versionText.text = string.Format("Vuforia Version: {0} | Unity Version: {1}", vuforiaVersion, unityVersion);
    }
}
