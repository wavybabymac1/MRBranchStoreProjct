/*==============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vuforia;

public class ModelTargetsManager : MonoBehaviour
{
    public enum ModelTargetMode
    {
        MODE_STANDARD,
        MODE_ADVANCED
    }

    [Header("Initial Model Target Mode")]
    [SerializeField] ModelTargetMode modelTargetMode = ModelTargetMode.MODE_STANDARD;

    [Header("DataSet Names")]
    [SerializeField] string dataSetStandard = "";
    [SerializeField] string dataSetAdvanced = "";

    [Header("UI Images")]
    [SerializeField] GameObject imageStandard = null;
    [SerializeField] GameObject imageAdvanced = null;

    ObjectTracker objectTracker;
    StateManager stateManager;
    List<ModelTargetBehaviour> modelTargetBehaviours;
    string currentActiveDataSet = string.Empty;

    void Awake()
    {
        this.modelTargetBehaviours = new List<ModelTargetBehaviour>();
    }
    
    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
    }

    void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);

        DeactivateActiveDataSets(true);
    }

    void OnVuforiaStarted()
    {
        this.stateManager = TrackerManager.Instance.GetStateManager();
        this.objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

        LoadDataSet(dataSetStandard);
        LoadDataSet(dataSetAdvanced);

        // We can only have one ModelTarget active at a time, so disable all MTBs at start.
        ModelTargetBehaviour[] behaviours = FindObjectsOfType<ModelTargetBehaviour>();
        foreach (ModelTargetBehaviour mtb in behaviours)
        {
            mtb.enabled = false;
        }

        switch (modelTargetMode)
        {
            case ModelTargetMode.MODE_STANDARD:
                // Start with the Standard Model Target DataSet
                SelectDataSetStandard();
                break;
            case ModelTargetMode.MODE_ADVANCED:
                // Start with the Advanced Model Target DataSet
                SelectDataSetAdvanced();
                break;
        }
    }

    public void SelectDataSetStandard()
    {
        ActivateDataSet(dataSetStandard);
        this.modelTargetMode = ModelTargetMode.MODE_STANDARD;

        this.imageStandard.SetActive(true);
        this.imageAdvanced.SetActive(false);
    }

    public void SelectDataSetAdvanced()
    {
        ActivateDataSet(dataSetAdvanced);
        this.modelTargetMode = ModelTargetMode.MODE_ADVANCED;

        this.imageStandard.SetActive(false);
        this.imageAdvanced.SetActive(true);
    }

    void LoadDataSet(string datasetName)
    {
        if (DataSet.Exists(datasetName))
        {
            DataSet dataset = this.objectTracker.CreateDataSet();

            if (dataset.Load(datasetName))
            {
                Debug.Log("Loaded DataSet: " + datasetName);
            }
            else
            {
                Debug.LogError("Failed to load DataSet: " + datasetName);
            }
        }
        else
        {
            Debug.Log("The following DataSet not found in 'StreamingAssets/Vuforia': " + datasetName);
        }
    }

    void DeactivateActiveDataSets(bool destroyDataSets = false)
    {
        if (this.objectTracker == null)
        {
            return;
        }

        List<DataSet> activeDataSets = this.objectTracker.GetActiveDataSets().ToList();
        foreach (DataSet ds in activeDataSets)
        {
            // The VuforiaEmulator.xml dataset (used by GroundPlane) is managed by Vuforia.
            if (!ds.Path.Contains("VuforiaEmulator.xml"))
            {
                Debug.Log("Deactivating: " + ds.Path);
                this.objectTracker.DeactivateDataSet(ds);
                if (destroyDataSets)
                {
                    this.objectTracker.DestroyDataSet(ds, false);
                }
            }
        }
    }

    void ActivateDataSet(string datasetName)
    {
        Debug.Log("ActivateDataSet() called: " + datasetName);

        if (this.currentActiveDataSet == datasetName)
        {
            Debug.Log("The selected dataset is already active.");
            // If the current dataset is already active, return.
            return;
        }

        if (this.objectTracker == null)
        {
            return;
        }

        // Stop the Object Tracker before activating/deactivating datasets.
        this.objectTracker.Stop();

        // Deactivate the currently active datasets.
        DeactivateActiveDataSets();

        var dataSets = this.objectTracker.GetDataSets();

        bool dataSetFoundAndActivated = false;

        foreach (DataSet ds in dataSets)
        {
            if (ds.Path.Contains(datasetName + ".xml"))
            {
                // Activate the selected dataset.
                Debug.Log("Activating: " + ds.Path);
                if (this.objectTracker.ActivateDataSet(ds))
                {
                    this.currentActiveDataSet = datasetName;
                }

                dataSetFoundAndActivated = true;

                var trackables = ds.GetTrackables();

                foreach (Trackable t in trackables)
                {
                    ModelTarget modelTarget = t as ModelTarget;

                    Debug.Log(modelTarget.Name + ": Active Guide View Index " +
                        modelTarget.GetActiveGuideViewIndex().ToString() + " of " +
                        modelTarget.GetNumGuideViews().ToString() + " total Guide Views.");
                }

                // Once we find and process selected dataset, exit foreach loop.
                break;
            }
        }

        if (!dataSetFoundAndActivated)
        {
            Debug.LogError("DataSet Not Found: " + datasetName);
        }

        // Start the Object Tracker.
        this.objectTracker.Start();
    }
}