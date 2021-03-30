/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using Vuforia;

public class VuMarksHideInstructions : MonoBehaviour
{
    public GameObject target;

    private VuMarkManager vuMarkManager;
    private List<VuMarkBehaviour> vuMarkBehaviours = new List<VuMarkBehaviour>();

    public void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(VuforiaStarted);
    }

    public void VuforiaStarted()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(VuforiaStarted);

        // Listen for any new VuMark being detected
        this.vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        this.vuMarkManager.RegisterVuMarkBehaviourDetectedCallback(VuMarkBehaviourDetectedCallback);
    }

    public void OnDestroy()
    {
        if (this.vuMarkManager != null)
        {
            this.vuMarkManager.UnregisterVuMarkBehaviourDetectedCallback(VuMarkBehaviourDetectedCallback);
        }

        foreach (VuMarkBehaviour vuMarkBehaviour in this.vuMarkBehaviours)
        {
            vuMarkBehaviour.UnregisterOnTrackableStatusChanged(OnTrackableStatusChanged);
        }
    }

    public void VuMarkBehaviourDetectedCallback(VuMarkBehaviour vuMarkBehaviour)
    {
        // When a new VuMark is detected, we start listening for its status changes
        vuMarkBehaviours.Add(vuMarkBehaviour);
        vuMarkBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStatusChanged);
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
        // Check if any VuMark target is currently being rendered, in that case we hide the instructions
        foreach (VuMarkBehaviour vuMarkBehaviour in this.vuMarkBehaviours)
        {
            if (ShouldBeRendered(vuMarkBehaviour.CurrentStatus))
            {
                target.SetActive(false);
                return;
            }
        }

        target.SetActive(true);
    }
}
