/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using Vuforia;

public class ImageTargetsHideInstructions : MonoBehaviour
{
    public GameObject target;

    private ImageTargetBehaviour[] imageTargetBehaviours;

    public void Awake()
    {
        this.imageTargetBehaviours = FindObjectsOfType<ImageTargetBehaviour>();
    }

    public void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(VuforiaStarted);
    }

    public void VuforiaStarted()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(VuforiaStarted);

        // Listen for any status changes of the image targets
        foreach (ImageTargetBehaviour imageTargetBehaviour in this.imageTargetBehaviours)
        {
            imageTargetBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        }
    }

    public void OnDestroy()
    {
        foreach (ImageTargetBehaviour imageTargetBehaviour in this.imageTargetBehaviours)
        {
            imageTargetBehaviour.UnregisterOnTrackableStatusChanged(OnTrackableStatusChanged);
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
        // Check if any image target is currently being rendered, in that case we hide the instructions
        foreach (ImageTargetBehaviour imageTargetBehaviour in this.imageTargetBehaviours)
        {
            if (ShouldBeRendered(imageTargetBehaviour.CurrentStatus))
            {
                target.SetActive(false);
                return;
            }
        }

        target.SetActive(true);
    }
}
