/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using Vuforia;

public class ModelTargetsHideInstructions : MonoBehaviour
{
    public GameObject target;

    private ModelTargetBehaviour[] modelTargetBehaviours;

    public void Awake()
    {
        this.modelTargetBehaviours = FindObjectsOfType<ModelTargetBehaviour>();
    }

    public void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(VuforiaStarted);
    }

    public void VuforiaStarted()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(VuforiaStarted);

        // Listen for any status changes of the model targets
        foreach (ModelTargetBehaviour modelTargetBehaviour in this.modelTargetBehaviours)
        {
            modelTargetBehaviour.RegisterOnTrackableStatusInfoChanged(OnTrackableStatusInfoChanged);
            modelTargetBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        }
    }

    public void OnDestroy()
    {
        foreach (ModelTargetBehaviour modelTargetBehaviour in this.modelTargetBehaviours)
        {
            modelTargetBehaviour.UnregisterOnTrackableStatusInfoChanged(OnTrackableStatusInfoChanged);
            modelTargetBehaviour.UnregisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        }
    }

    public void OnTrackableStatusInfoChanged(TrackableBehaviour.StatusInfoChangeResult statusInfoChangeResult)
    {
        // If the guide view visibility changes we update the visibility of the instructions
        if (IsGuideViewRendered(statusInfoChangeResult.PreviousStatusInfo) != IsGuideViewRendered(statusInfoChangeResult.NewStatusInfo))
        {
            UpdateVisibility();
        }
    }

    public void OnTrackableStatusChanged(TrackableBehaviour.StatusChangeResult statusChangeResult)
    {
        // If the rendering state changes we update the visibility of the instructions
        if (ShouldBeRendered(statusChangeResult.PreviousStatus) != ShouldBeRendered(statusChangeResult.NewStatus))
        {
            UpdateVisibility();
        }
    }

    private bool IsGuideViewRendered(TrackableBehaviour.StatusInfo statusInfo)
    {
        if (statusInfo == TrackableBehaviour.StatusInfo.NO_DETECTION_RECOMMENDING_GUIDANCE)
        {
            return true;
        }

        return false;
    }

    private bool ShouldBeRendered(TrackableBehaviour.Status status)
    {
        if (status == TrackableBehaviour.Status.DETECTED ||
            status == TrackableBehaviour.Status.TRACKED ||
            status == TrackableBehaviour.Status.EXTENDED_TRACKED ||
            status == TrackableBehaviour.Status.LIMITED)
        {
            return true;
        }

        return false;
    }

    private void UpdateVisibility()
    {
        // Check if any model target or its guide view is currently being rendered, in that case we hide the instructions
        foreach (ModelTargetBehaviour modelTargetBehaviour in this.modelTargetBehaviours)
        {
            if (IsGuideViewRendered(modelTargetBehaviour.CurrentStatusInfo))
            {
                target.SetActive(false);
                return;
            }

            if (ShouldBeRendered(modelTargetBehaviour.CurrentStatus))
            {
                target.SetActive(false);
                return;
            }
        }

        target.SetActive(true);
    }
}
