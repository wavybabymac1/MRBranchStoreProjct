/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using UnityEngine;
using Vuforia;

public class VuMarkTrackableEventHandler : DefaultTrackableEventHandler
{
    #region PRIVATE_MEMBERS
    const int nthFrame = 15;
    const int persistentNumberOfChildren = 2;  // Persistent Children: 1. Canvas, 2. LineRenderer
    VuMarkBehaviour vumarkBehaviour;
    LineRenderer lineRenderer;
    #endregion // PRIVATE_MEMBERS


    #region PROTECTED_METHODS
    protected override void Start()
    {
        base.Start();

        if (this.mTrackableBehaviour is VuMarkBehaviour)
        {
            this.vumarkBehaviour = (VuMarkBehaviour)this.mTrackableBehaviour;
            this.vumarkBehaviour.RegisterVuMarkTargetAssignedCallback(OnVuMarkTargetAssigned);
            this.vumarkBehaviour.RegisterVuMarkTargetLostCallback(OnVuMarkTargetLost);
        }
    }

    // Override, but don't implement these base methods,
    // since VuMark gameobjects are automatically disabled
    protected override void OnTrackingFound() { }
    protected override void OnTrackingLost() { }

    #endregion // PROTECTED_METHODS

    #region MONOBEHAVIOUR_METHODS

    void OnDisable()
    {
        DestroyChildAugmentationsOfTransform(transform);
    }

    void Update()
    {
        // Every nth frame, update the VuMark border outline to catch any changes to target tracking status
        if (Time.frameCount % nthFrame == 0)
        {
            UpdateVuMarkBorderOutline();
        }
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS

    void UpdateVuMarkBorderOutline()
    {
        if (this.lineRenderer)
        {
            // Only enable line renderer when target becomes Extended Tracked or when running in Unity Editor.
            this.lineRenderer.enabled =
                (m_NewStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) || VuforiaRuntimeUtilities.IsPlayMode();

            if (this.lineRenderer.enabled)
            {
                // If the Device Tracker is enabled and the target becomes Extended Tracked,
                // set the VuMark outline to green. If in Unity Editor PlayMode, set to cyan.
                // Note that on HoloLens, the Device Tracker is always enabled (as of Vuforia 7.2).
                this.lineRenderer.material.color =
                    (m_NewStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) ? Color.green : Color.cyan;
            }
        }
        else
        {
            this.lineRenderer = GetComponentInChildren<LineRenderer>();
        }
    }
    
    void DestroyChildAugmentationsOfTransform(Transform parent)
    {
        if (parent.childCount > persistentNumberOfChildren)
        {
            for (int x = persistentNumberOfChildren; x < parent.childCount; x++)
            {
                Destroy(parent.GetChild(x).gameObject);
            }
        }
    }
    #endregion // PRIVATE_METHODS


    #region VUMARK_CALLBACK_METHODS
    public void OnVuMarkTargetAssigned()
    {
        VLog.Log("cyan", "VuMarkBehaviour.OnVuMarkTargetAssigned() called.");

        if (this.mTrackableBehaviour is VuMarkBehaviour)
        {
            this.vumarkBehaviour = this.mTrackableBehaviour as VuMarkBehaviour;
            if (this.vumarkBehaviour.VuMarkTarget != null)
                VLog.Log("cyan", "VuMark ID Tracked: " + this.vumarkBehaviour.VuMarkTarget.InstanceId.NumericValue);
        }
    }

    public void OnVuMarkTargetLost()
    {
        VLog.Log("cyan", "VuMarkBehaviour.OnVuMarkLost() called.");
    }

    #endregion // VUMARK_CALLBACK_METHODS

}
