﻿/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class VuMarkHandler : MonoBehaviour
{
    // Define the number of persistent child objects of the VuMarkBehaviour. When
    // destroying the instance-specific augmentations, it will start after this value.
    // Persistent Children:
    // 1. Canvas -- displays info about the VuMark
    // 2. LineRenderer -- displays border outline around VuMark
    const int PersistentNumberOfChildren = 2;
    VuMarkManager vumarkManager;
    LineRenderer lineRenderer;
    Dictionary<string, Texture2D> vumarkInstanceTextures;
    Dictionary<string, GameObject> vumarkAugmentationObjects;
    
    [System.Serializable]
    public class AugmentationObject
    {
        public string vumarkID;
        public GameObject augmentation;
    }
    public AugmentationObject[] augmentationObjects;

    void Awake()
    {
        VuforiaConfiguration.Instance.Vuforia.MaxSimultaneousImageTargets = 10;
    }

    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);

        this.vumarkInstanceTextures = new Dictionary<string, Texture2D>();
        this.vumarkAugmentationObjects = new Dictionary<string, GameObject>();

        foreach (AugmentationObject obj in this.augmentationObjects)
        {
            this.vumarkAugmentationObjects.Add(obj.vumarkID, obj.augmentation);
        }

        // Hide the initial VuMark Template when the scene starts.
        VuMarkBehaviour vumarkBehaviour = FindObjectOfType<VuMarkBehaviour>();
        if (vumarkBehaviour)
        {
            ToggleRenderers(vumarkBehaviour.gameObject, false);
        }
    }

    void OnDestroy()
    {
        VuforiaConfiguration.Instance.Vuforia.MaxSimultaneousImageTargets = 4; // Reset back to 4 when exiting
        // Unregister callbacks from VuMark Manager
        this.vumarkManager.UnregisterVuMarkBehaviourDetectedCallback(OnVuMarkBehaviourDetected);
        this.vumarkManager.UnregisterVuMarkDetectedCallback(OnVuMarkDetected);
        this.vumarkManager.UnregisterVuMarkLostCallback(OnVuMarkLost);
    }

    void OnVuforiaStarted()
    {
        // register callbacks to VuMark Manager
        this.vumarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        this.vumarkManager.RegisterVuMarkBehaviourDetectedCallback(OnVuMarkBehaviourDetected);
        this.vumarkManager.RegisterVuMarkDetectedCallback(OnVuMarkDetected);
        this.vumarkManager.RegisterVuMarkLostCallback(OnVuMarkLost);
    }
    
    /// <summary>
    ///  Register a callback which is invoked whenever a VuMark-result is newly detected which was not tracked in the frame before
    /// </summary>
    /// <param name="vumarkBehaviour"></param>
    public void OnVuMarkBehaviourDetected(VuMarkBehaviour vumarkBehaviour)
    {
        VLog.Log("cyan", "VuMarkHandler.OnVuMarkBehaviourDetected(): " + vumarkBehaviour.TrackableName);

        GenerateVuMarkBorderOutline(vumarkBehaviour);
        
        ToggleRenderers(vumarkBehaviour.gameObject, true);

        // Check for existance of previous augmentations and delete before instantiating new ones.
        DestroyChildAugmentationsOfTransform(vumarkBehaviour.transform);

        StartCoroutine(OnVuMarkTargetAvailable(vumarkBehaviour));
    }
    
    IEnumerator OnVuMarkTargetAvailable(VuMarkBehaviour vumarkBehaviour)
    {
        // We need to wait until VuMarkTarget is available
        yield return new WaitUntil(() => vumarkBehaviour.VuMarkTarget != null);

        VLog.Log("green", "VuMarkHandler.OnVuMarkTargetAvailable() called: " + GetVuMarkId(vumarkBehaviour.VuMarkTarget));

        SetVuMarkInfoForCanvas(vumarkBehaviour);
        SetVuMarkAugmentation(vumarkBehaviour);
    }

    /// <summary>
    /// This method will be called whenever a new VuMark is detected
    /// </summary>
    public void OnVuMarkDetected(VuMarkTarget vumarkTarget)
    {
        VLog.Log("cyan", "VuMarkHandler.OnVuMarkDetected(): " + GetVuMarkId(vumarkTarget));

        // Check if this VuMark's ID already has a stored texture. Generate and store one if not.
        if (RetrieveStoredTextureForVuMarkTarget(vumarkTarget) == null)
        {
            this.vumarkInstanceTextures.Add(GetVuMarkId(vumarkTarget), GenerateTextureFromVuMarkInstanceImage(vumarkTarget));
        }
    }

    /// <summary>
    /// This method will be called whenever a tracked VuMark is lost
    /// </summary>
    public void OnVuMarkLost(VuMarkTarget vumarkTarget)
    {
        VLog.Log("cyan", "VuMarkHandler.OnVuMarkLost(): " + GetVuMarkId(vumarkTarget));
    }

    string GetVuMarkDataType(VuMarkTarget vumarkTarget)
    {
        switch (vumarkTarget.InstanceId.DataType)
        {
            case InstanceIdType.BYTES:
                return "Bytes";
            case InstanceIdType.STRING:
                return "String";
            case InstanceIdType.NUMERIC:
                return "Numeric";
        }
        return string.Empty;
    }

    string GetVuMarkId(VuMarkTarget vumarkTarget)
    {
        switch (vumarkTarget.InstanceId.DataType)
        {
            case InstanceIdType.BYTES:
                return vumarkTarget.InstanceId.HexStringValue;
            case InstanceIdType.STRING:
                return vumarkTarget.InstanceId.StringValue;
            case InstanceIdType.NUMERIC:
                return vumarkTarget.InstanceId.NumericValue.ToString();
        }
        return string.Empty;
    }

    string GetNumericVuMarkDescription(VuMarkTarget vumarkTarget)
    {
        int vumarkIdNumeric;

        if (int.TryParse(GetVuMarkId(vumarkTarget), NumberStyles.Integer, CultureInfo.InvariantCulture, out vumarkIdNumeric))
        {
            // Change the description based on the VuMark ID
            switch (vumarkIdNumeric % 4)
            {
                case 1:
                    return "Astronaut";
                case 2:
                    return "Drone";
                case 3:
                    return "Fissure";
                case 0:
                    return "Oxygen Tank";
                default:
                    return "Unknown";
            }
        }

        return string.Empty; // if VuMark DataType is byte or string
    }
    
    void SetVuMarkInfoForCanvas(VuMarkBehaviour vumarkBehaviour)
    {
        Text canvasText = vumarkBehaviour.gameObject.GetComponentInChildren<Text>();
        string vuMarkId = GetVuMarkId(vumarkBehaviour.VuMarkTarget);
        string vuMarkDesc = GetVuMarkDataType(vumarkBehaviour.VuMarkTarget);
        string vuMarkDataType = GetNumericVuMarkDescription(vumarkBehaviour.VuMarkTarget);

        canvasText.text =
            "<color=yellow>VuMark Instance Id: </color>" +
            "\n" + vuMarkId + " - " + vuMarkDesc +
            "\n\n<color=yellow>VuMark Type: </color>" +
            "\n" + vuMarkDataType;
    }
    
    void SetVuMarkAugmentation(VuMarkBehaviour vumarkBehaviour)
    {
        GameObject sourceAugmentation = GetValueFromDictionary(this.vumarkAugmentationObjects, GetVuMarkId(vumarkBehaviour.VuMarkTarget));

        if (sourceAugmentation)
        {
            GameObject augmentation = Instantiate(sourceAugmentation);
            augmentation.transform.SetParent(vumarkBehaviour.transform);
            augmentation.transform.localPosition = Vector3.zero;
            augmentation.transform.localScale = Vector3.one;
            augmentation.transform.localEulerAngles = Vector3.zero;
        }
    }

    Texture2D RetrieveStoredTextureForVuMarkTarget(VuMarkTarget vumarkTarget)
    {
        return GetValueFromDictionary(this.vumarkInstanceTextures, GetVuMarkId(vumarkTarget));
    }

    Texture2D GenerateTextureFromVuMarkInstanceImage(VuMarkTarget vumarkTarget)
    {
        VLog.Log("cyan", "SaveImageAsTexture() called.");

        if (vumarkTarget.InstanceImage == null)
        {
            Debug.Log("VuMark Instance Image is null.");
            return null;
        }
        Debug.Log(vumarkTarget.InstanceImage.Width + "," + vumarkTarget.InstanceImage.Height);

        Texture2D texture = new Texture2D(vumarkTarget.InstanceImage.Width, vumarkTarget.InstanceImage.Height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };

        vumarkTarget.InstanceImage.CopyToTexture(texture, false);

        return texture;
    }

    void GenerateVuMarkBorderOutline(VuMarkBehaviour vumarkBehaviour)
    {
        this.lineRenderer = vumarkBehaviour.GetComponentInChildren<LineRenderer>();

        if (this.lineRenderer == null)
        {
            VLog.Log("green", "Existing Line Renderer not found. Creating new one.");
            GameObject vumarkBorder = new GameObject("VuMarkBorder");
            vumarkBorder.transform.SetParent(vumarkBehaviour.transform);
            vumarkBorder.transform.localPosition = Vector3.zero;
            vumarkBorder.transform.localEulerAngles = Vector3.zero;
            vumarkBorder.transform.localScale =
                new Vector3(
                    1 / vumarkBehaviour.transform.localScale.x,
                    1,
                    1 / vumarkBehaviour.transform.localScale.z);
            this.lineRenderer = vumarkBorder.AddComponent<LineRenderer>();
            this.lineRenderer.enabled = false;
            this.lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            this.lineRenderer.receiveShadows = false;
            // This shader needs to be added in the Project's Graphics Settings,
            // unless it is already in use by a Material present in the project.
            this.lineRenderer.material.shader = Shader.Find("Unlit/Color");
            this.lineRenderer.material.color = Color.clear;
            this.lineRenderer.positionCount = 4;
            this.lineRenderer.loop = true;
            this.lineRenderer.useWorldSpace = false;
            Vector2 vumarkSize = vumarkBehaviour.GetSize();
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 1.0f);
            this.lineRenderer.widthCurve = curve;
            this.lineRenderer.widthMultiplier = 0.003f;
            float vumarkExtentsX = (vumarkSize.x * 0.5f) + (this.lineRenderer.widthMultiplier * 0.5f);
            float vumarkExtentsZ = (vumarkSize.y * 0.5f) + (this.lineRenderer.widthMultiplier * 0.5f);
            this.lineRenderer.SetPositions(new Vector3[]
            {
                new Vector3(-vumarkExtentsX, 0.001f, vumarkExtentsZ),
                new Vector3(vumarkExtentsX, 0.001f, vumarkExtentsZ),
                new Vector3(vumarkExtentsX, 0.001f, -vumarkExtentsZ),
                new Vector3(-vumarkExtentsX, 0.001f, -vumarkExtentsZ)
            });
        }
    }
    
    void DestroyChildAugmentationsOfTransform(Transform parent)
    {
        if (parent.childCount > PersistentNumberOfChildren)
        {
            for (int x = parent.childCount - 1; x >= PersistentNumberOfChildren; x--)
            {
                Destroy(parent.GetChild(x).gameObject);
            }
        }
    }

    T GetValueFromDictionary<T>(Dictionary<string, T> dictionary, string key)
    {
        if (dictionary.ContainsKey(key))
        {
            T value;
            dictionary.TryGetValue(key, out value);
            return value;
        }
        return default(T);
    }

    void ToggleRenderers(GameObject obj, bool enable)
    {
        var rendererComponents = obj.GetComponentsInChildren<Renderer>(true);
        var canvasComponents = obj.GetComponentsInChildren<Canvas>(true);

        foreach (var component in rendererComponents)
        {
            // Skip the LineRenderer
            if (!(component is LineRenderer))
            {
                component.enabled = enable;
            }
        }

        foreach (var component in canvasComponents)
        {
            component.enabled = enable;
        }
    }
}
