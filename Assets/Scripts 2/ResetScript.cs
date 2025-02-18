/*
 * Name: Bhoomika Manot
 * Date: 15 October 2024
 * Description: Script to center models or reset back to original
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResetScript : MonoBehaviour
{
    //Declarations
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Camera States")]
    public Vector3 initialCameraPosition;
    public Quaternion initialCameraRotation;
    public float initialZoom; 

    [Header("Models")]
    public GameObject models;
    public Quaternion initialModelRotation;

    [Header("Text")]
    public TMP_Text labelText;

    [Header("Toggles")]
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;

    //Dictionary to store the initial values
    public Dictionary<GameObject, Vector3> initialBodyPositions = new Dictionary<GameObject, Vector3>();
    public Dictionary<GameObject, Quaternion> initialBodyRotations = new Dictionary<GameObject, Quaternion>();
    public Dictionary<GameObject, Transform> initialBodyParents = new Dictionary<GameObject, Transform>();

    //BodyPartsScript script ref
    public BodyPartsScript bodyPartManager;
    public CrossSectionController crossSectionScript;

    void Awake()
    {
        bodyPartManager = FindObjectOfType<BodyPartsScript>();

        //Get initial values of cam
        if (mainCamera != null)
        {
            initialCameraPosition = mainCamera.transform.position;
            initialCameraRotation = mainCamera.transform.rotation;

            //Storing initial zoom
            if (mainCamera.orthographic)
            {
                initialZoom = mainCamera.orthographicSize;
            }
        }

        //Storing the initial values of the body parts
        GameObject[] bodyParts = GameObject.FindGameObjectsWithTag("Movable");
        foreach (GameObject part in bodyParts)
        {
            initialBodyPositions[part] = part.transform.position;
            initialBodyRotations[part] = part.transform.rotation;
            initialBodyParents[part] = part.transform.parent;
        }

        //Store initial rotation
        if (models != null)
        {
            initialModelRotation = models.transform.rotation;
        }
    }

    public void Center()
    {
        //Reset cam values
        if (mainCamera != null)
        {
            mainCamera.transform.position = initialCameraPosition;
            mainCamera.transform.rotation = initialCameraRotation;

            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = initialZoom;
            }
        }

        //Reset rotation
        if (models != null)
        {
            models.transform.rotation = initialModelRotation;
        }

        if (crossSectionScript.crossSectionToggle.isOn)
        {
            crossSectionScript.planeTransform.position = new Vector3(0f, 0.75f, 0f);
        }
    }

    public void Reset()
    {
        //Reset body part values
        foreach (KeyValuePair<GameObject, Vector3> entry in initialBodyPositions)
        {
            GameObject part = entry.Key;

            bodyPartManager.ClearColours();
            bodyPartManager.Deisolate();

            part.transform.position = entry.Value;
            part.transform.rotation = initialBodyRotations[part];

            ClearSelection();

            if (crossSectionScript.crossSectionToggle.isOn) 
            {
                crossSectionScript.planeTransform.position = new Vector3(0f, 0.75f, 0f);
            }
        }

    }
    public void ClearSelection()
    {
        //Clear selected body parts and offsets
        bodyPartManager.selectedBodyParts.Clear();
        bodyPartManager.dragOffsets.Clear();

        //Reset label UI
        labelText.text = "No parts selected";

        //Reset selection toggles
        singleSelectToggle.isOn = false;
        multiSelectToggle.isOn = false;

        bodyPartManager.isDragging = false;
    }
}