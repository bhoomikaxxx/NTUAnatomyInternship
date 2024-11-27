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


    //Undo script
    private BodyPartsScript bodyPartManager;

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
        //Reset camera values
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
    }

    public void Reset()
    {
        //Reset body part values
        foreach (KeyValuePair<GameObject, Vector3> entry in initialBodyPositions)
        {
            GameObject part = entry.Key;

            part.transform.position = entry.Value;
            part.transform.rotation = initialBodyRotations[part];

            ClearSelection();
        }

    }
    public void ClearSelection()
    {
        // Clear selected body parts and offsets
        bodyPartManager.selectedBodyParts.Clear();
        bodyPartManager.dragOffsets.Clear();

        // Reset UI text or states (optional)
        if (labelText != null)
        {
            labelText.text = "No parts selected";
        }

        // Reset selection toggles
        singleSelectToggle.isOn = false;
        multiSelectToggle.isOn = false;

        bodyPartManager.isDragging = false;
    }
}