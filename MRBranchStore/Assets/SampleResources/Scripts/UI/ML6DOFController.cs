/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.Events;
#if ML_ASSETS_IMPORTED
using UnityEngine.XR.MagicLeap;
#endif

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

public class ML6DOFController : MonoBehaviour
{
#if ML_ASSETS_IMPORTED
    private MLInput.Controller inputController;
#endif

    #if PLATFORM_LUMIN && ML_ASSETS_IMPORTED
    void Start()
    {
        MLInput.Start();
        inputController = MLInput.GetController(MLInput.Hand.Left);
    }

    void OnDestroy()
    {
        MLInput.Stop();
    }

    void Update()
    {
        if (inputController != null)
        {
            transform.position = inputController.Position;
            transform.rotation = inputController.Orientation;
        }
    }

    public void Vibrate(MLInput.Controller.FeedbackPatternVibe patternVibe)
    {
        inputController.StartFeedbackPatternVibe(patternVibe, MLInput.Controller.FeedbackIntensity.Medium);
    }
    #endif
}
