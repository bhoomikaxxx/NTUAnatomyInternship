/*
 * Name: Bhoomika Manot
 * Date: 05 Jan 2025
 * Description: Script for cross sectioning heart and brain 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CrossSectionController : MonoBehaviour
{
    //Declarations
    public Material crossSectionMaterial;  
    public Transform planeTransform;      
    public Slider crossSectionSlider;      
    public Toggle crossSectionToggle;

    public Toggle nervous, cardio;

    private Vector3 defaultPlanePosition;  

    void Start()
    {
        //Store the initial plane pos
        defaultPlanePosition = planeTransform.position;

        crossSectionSlider.onValueChanged.AddListener(UpdateCrossSection);
        crossSectionToggle.onValueChanged.AddListener(ToggleCrossSection);
    }

    //Slider func
    void UpdateCrossSection(float sliderValue)
    {
        //Check if toggle is on
        if (crossSectionToggle.isOn) 
        {
            //Invert slider
            float invertedValue = 1f - sliderValue;

            //Slider func
            planeTransform.position = new Vector3(defaultPlanePosition.x, invertedValue, defaultPlanePosition.z);
            crossSectionMaterial.SetVector("_PlanePosition", planeTransform.position);
        }
    }

    //Toggle func
    void ToggleCrossSection(bool isEnabled)
    {
        if (!isEnabled)
        {
            //Reset plane pos
            planeTransform.position = new Vector3(0f, 0.75f, 0f);
        }

        crossSectionMaterial.SetVector("_PlanePosition", planeTransform.position);
    }
}
//need to change plane pos Y in material to 0 once resetted