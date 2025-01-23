using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CrossSectionController : MonoBehaviour
{
    public Material crossSectionMaterial;  // Assign the cross-section material
    public Transform planeTransform;       // Assign the plane object (empty GameObject)
    public Slider crossSectionSlider;      // Assign the UI Slider
    public Toggle crossSectionToggle;      // Assign the UI Checkbox

    private Vector3 defaultPlanePosition;  // Store default plane position

    void Start()
    {
        // Store the initial plane position (so we can reset when toggle is off)
        defaultPlanePosition = planeTransform.position;

        // Add listeners to UI elements
        crossSectionSlider.onValueChanged.AddListener(UpdateCrossSection);
        crossSectionToggle.onValueChanged.AddListener(ToggleCrossSection);
    }

    void UpdateCrossSection(float sliderValue)
    {
        if (crossSectionToggle.isOn) // Only move the plane if cross-section is enabled
        {
            float invertedValue = 1f - sliderValue; // Invert slider effect
            planeTransform.position = new Vector3(defaultPlanePosition.x, invertedValue, defaultPlanePosition.z);
            crossSectionMaterial.SetVector("_PlanePosition", planeTransform.position);
        }
    }

    void ToggleCrossSection(bool isEnabled)
    {
        if (!isEnabled)
        {
            // Reset plane position when unchecked
            planeTransform.position = new Vector3(0f, 0.75f, 0f);
        }

        // Apply the change to the shader
        crossSectionMaterial.SetVector("_PlanePosition", planeTransform.position);
    }
}
//need to change plane pos Y in material to 0 once resetted