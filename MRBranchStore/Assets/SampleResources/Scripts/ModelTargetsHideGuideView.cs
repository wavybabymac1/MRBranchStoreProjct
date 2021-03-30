/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using Vuforia;

public class ModelTargetsHideGuideView : MonoBehaviour
{
    private ModelTargetBehaviour[] modelTargetBehaviours;

    public void Awake()
    {
        this.modelTargetBehaviours = FindObjectsOfType<ModelTargetBehaviour>();
    }

    public void OnEnable()
    {
        // Disable the guideview for all model targets
        foreach (ModelTargetBehaviour modelTargetBehaviour in this.modelTargetBehaviours)
        {
            modelTargetBehaviour.GuideViewMode = ModelTargetBehaviour.GuideViewDisplayMode.NoGuideView;
        }
    }

    public void OnDisable()
    {
        // Enable the guideview for all model targets
        foreach (ModelTargetBehaviour modelTargetBehaviour in this.modelTargetBehaviours)
        {
            modelTargetBehaviour.GuideViewMode = ModelTargetBehaviour.GuideViewDisplayMode.GuideView3D;
        }
    }
}
