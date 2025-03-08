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
    public GameObject skeletonModel;
    public GameObject jointsModel;
    public GameObject loModel;
    public GameObject nsModel;
    public GameObject vsModel;
    public GameObject cardioModel;
    public GameObject msModel;
    public GameObject humanModel;
    public Quaternion initialModelRotation;

    [Header("Text")]
    public TMP_Text labelText;

    [Header("Toggles")]
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;

    // Dictionary to store the initial values
    private Dictionary<GameObject, Vector3> initialBodyPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> initialBodyRotations = new Dictionary<GameObject, Quaternion>();
    private Dictionary<GameObject, Transform> initialBodyParents = new Dictionary<GameObject, Transform>();

    // BodyPartsScript script ref
    public BodyPartsScript bodyPartManager;
    public CrossSectionController crossSectionScript;

    void Awake()
    {
        bodyPartManager = FindObjectOfType<BodyPartsScript>();

        if (mainCamera != null)
        {
            initialCameraPosition = mainCamera.transform.position;
            initialCameraRotation = mainCamera.transform.rotation;
            if (mainCamera.orthographic)
            {
                initialZoom = mainCamera.orthographicSize;
            }
        }

        StoreInitialBodyPartValues(skeletonModel);
        StoreInitialBodyPartValues(jointsModel);
        StoreInitialBodyPartValues(loModel);
        StoreInitialBodyPartValues(nsModel);
        StoreInitialBodyPartValues(vsModel);
        StoreInitialBodyPartValues(cardioModel);
        StoreInitialBodyPartValues(msModel);
        StoreInitialBodyPartValues(humanModel);
    }

    private void StoreInitialBodyPartValues(GameObject model)
    {
        if (model != null)
        {
            foreach (Transform part in model.GetComponentsInChildren<Transform>())
            {
                if (part.CompareTag("Movable"))
                {
                    initialBodyPositions[part.gameObject] = part.position;
                    initialBodyRotations[part.gameObject] = part.rotation;
                    initialBodyParents[part.gameObject] = part.parent;
                }
            }
        }
    }

    public void Center()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = initialCameraPosition;
            mainCamera.transform.rotation = initialCameraRotation;
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = initialZoom;
            }
        }
    }

    public void Reset()
    {
        foreach (KeyValuePair<GameObject, Vector3> entry in initialBodyPositions)
        {
            GameObject part = entry.Key;
            if (part != null)
            {
                //bodyPartManager.ClearColours();
                bodyPartManager.ClearSelection();
                bodyPartManager.Deisolate();
                part.transform.position = entry.Value;
                part.transform.rotation = initialBodyRotations[part];
            }
        }
        ClearSelection();
    }

    public void ClearSelection()
    {
        bodyPartManager.selectedBodyParts.Clear();
        bodyPartManager.dragOffsets.Clear();
        labelText.text = "No parts selected";
        singleSelectToggle.isOn = false;
        multiSelectToggle.isOn = false;
        bodyPartManager.isDragging = false;
    }
}
