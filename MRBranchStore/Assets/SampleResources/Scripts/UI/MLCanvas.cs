/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ML_ASSETS_IMPORTED
using UnityEngine.XR.MagicLeap;
#endif

public class MLCanvas : MonoBehaviour
{
    public int canvasPlane = 0;

    public enum HeadTracking {
        None, Fixed, Smooth
    }
    public HeadTracking headTracking;
    public float headTrackingSpeed;

    public Camera mainCamera;

    private List<MLButton> buttons;
    private MLButton selectedButton;

    private ML6DOFController controller;

    private AnimatedFloat animatedDepthPlane;
    private AnimatedFloat animatedAlpha;

    private Renderer[] renderers;
    private Image[] images;
    private Text[] texts;
    
    #if PLATFORM_LUMIN && ML_ASSETS_IMPORTED
    public void Awake()
    {
        animatedDepthPlane = new AnimatedFloat(canvasPlane);
        animatedAlpha = new AnimatedFloat(1.0f);

        buttons = new List<MLButton>(GetComponentsInChildren<MLButton>());
        controller = FindObjectOfType<ML6DOFController>();

        renderers = GetComponentsInChildren<Renderer>(true);
        images = GetComponentsInChildren<Image>(true);
        texts = GetComponentsInChildren<Text>(true);
    }

    public void OnEnable()
    {
        MLInput.TriggerDownThreshold = 0.5f;

        MLInput.OnControllerTouchpadGestureStart += OnControllerTouchpadGestureStart;
        MLInput.OnControllerTouchpadGestureContinue += OnControllerTouchpadGestureContinue;

        MLInput.OnTriggerUp += OnTriggerUp;

        if ((headTracking == HeadTracking.Fixed) || (headTracking == HeadTracking.Smooth))
        {
            gameObject.transform.position = GetHeadPosition();
            gameObject.transform.rotation = GetHeadRotation();
        }
    }

    public void OnDisable()
    {
        MLInput.OnControllerTouchpadGestureStart -= OnControllerTouchpadGestureStart;
        MLInput.OnControllerTouchpadGestureContinue -= OnControllerTouchpadGestureContinue;

        MLInput.OnTriggerUp -= OnTriggerUp;

        SelectButton(null);
    }

    public void Update()
    {
        if (headTracking == HeadTracking.Fixed)
        {
            gameObject.transform.position = GetHeadPosition();
            gameObject.transform.rotation = GetHeadRotation();
        }
        else if (headTracking == HeadTracking.Smooth)
        {
            float deltaSpeed = Mathf.Clamp01(Time.deltaTime * headTrackingSpeed);
            gameObject.transform.position = Vector3.SlerpUnclamped(gameObject.transform.position, GetHeadPosition(), deltaSpeed);
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, GetHeadRotation(), deltaSpeed);
        }

        if (interactionEnabled)
        {
            RaycastHit hit;
            MLButton hitButton = null;
            if (Physics.Raycast(controller.transform.position, controller.transform.forward, out hit))
            {
                hitButton = hit.collider.GetComponentInParent<MLButton>();
            }

            if (hitButton != null)
            {
                SelectButton(hitButton);
            }
        }

        if (animatedAlpha.IsAnimating())
        {
            SetAlpha(animatedAlpha.GetValue());
        }
    }

    private Vector3 GetHeadPosition()
    {
        float distance = MLConstants.CANVAS_DEPTH - animatedDepthPlane.GetValue() * MLConstants.CANVAS_DEPTH_PLANE_STEP;
        return mainCamera.transform.position + (mainCamera.transform.forward * distance);
    }

    private Quaternion GetHeadRotation()
    {
        return Quaternion.LookRotation(gameObject.transform.position - mainCamera.transform.position);
    }

    private bool interactionEnabled = true;

    public void SetInteractionEnabled(bool value)
    {
        interactionEnabled = value;

        if (interactionEnabled)
        {
            animatedDepthPlane.AnimateToValue(canvasPlane, MLConstants.ANIMATION_TIME);
            animatedAlpha.AnimateToValue(1.0f, MLConstants.ANIMATION_TIME);

            SetCollidersEnabled(true);
        }
        else
        {
            animatedDepthPlane.AnimateToValue(canvasPlane - 1, MLConstants.ANIMATION_TIME);
            animatedAlpha.AnimateToValue(0.4f, MLConstants.ANIMATION_TIME);

            SetCollidersEnabled(false);

            SelectButton(null);
        }
    }

    private void SetCollidersEnabled(bool value)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = value;
        }
    }

    private void SelectButton(MLButton button)
    {
        if (button != selectedButton)
        {
            if (selectedButton != null)
            {
                selectedButton.SetSelected(false);
            }

            selectedButton = button;

            if (selectedButton != null)
            {
                selectedButton.SetSelected(true);
                controller.Vibrate(MLInput.Controller.FeedbackPatternVibe.Click);
            }
        }
    }

    private void PreviousButton()
    {
        if (buttons.Count == 0)
        {
            return;
        }

        if (selectedButton == null)
        {
            SelectButton(buttons[0]);
        }
        else
        {
            int currentIndex = buttons.IndexOf(selectedButton);
            int previousIndex = (buttons.Count + currentIndex - 1) % buttons.Count;
            SelectButton(buttons[previousIndex]);
        }
    }

    private void NextButton()
    {
        if (buttons.Count == 0)
        {
            return;
        }

        if (selectedButton == null)
        {
            SelectButton(buttons[0]);
        }
        else
        {
            int currentIndex = buttons.IndexOf(selectedButton);
            int nextIndex = (currentIndex + 1) % buttons.Count;
            SelectButton(buttons[nextIndex]);
        }
    }

    private void OnControllerTouchpadGestureStart(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesture)
    {
        if (interactionEnabled)
        {
            OnControllerTouchpadGesture(touchpadGesture);
        }
    }

    private void OnControllerTouchpadGestureContinue(byte controllerId, MLInput.Controller.TouchpadGesture touchpadGesture)
    {
        if (interactionEnabled)
        {
            OnControllerTouchpadGesture(touchpadGesture);
        }
    }

    private void OnControllerTouchpadGesture(MLInput.Controller.TouchpadGesture touchpadGesture)
    {
        switch (touchpadGesture.Direction)
        {
            case MLInput.Controller.TouchpadGesture.GestureDirection.Left:
            case MLInput.Controller.TouchpadGesture.GestureDirection.Up:
            case MLInput.Controller.TouchpadGesture.GestureDirection.CounterClockwise:
                PreviousButton();
                break;

            case MLInput.Controller.TouchpadGesture.GestureDirection.Right:
            case MLInput.Controller.TouchpadGesture.GestureDirection.Down:
            case MLInput.Controller.TouchpadGesture.GestureDirection.Clockwise:
                NextButton();
                break;
        }
    }

    private void OnTriggerUp(byte controllerId, float triggerValue)
    {
        if (selectedButton != null)
        {
            controller.Vibrate(MLInput.Controller.FeedbackPatternVibe.Click);
            selectedButton.onClick.Invoke();
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                Color color = m.color;
                color.a = alpha;
                m.color = color;
            }
        }

        foreach (Image i in images)
        {
            Color color = i.color;
            color.a = alpha;
            i.color = color;
        }

        foreach (Text t in texts)
        {
            Color color = t.color;
            color.a = alpha;
            t.color = color;
        }
    }
    #endif 
}
