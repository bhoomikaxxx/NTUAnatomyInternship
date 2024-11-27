/*
 * Name: Bhoomika Manot
 * Date: 15 October 2024
 * Description: Script for model/asset rotation on smartboard using touch fingers and desktop using mouse.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset : MonoBehaviour
{
    // Declaration
    public Camera mainCamera;

    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private float initialZoom; // For storing initial zoom level

    private Dictionary<GameObject, Vector3> initialBodyPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> initialBodyRotations = new Dictionary<GameObject, Quaternion>();

    public DragObj dragScript;

    void Start()
    {
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

        GameObject[] bodyParts = GameObject.FindGameObjectsWithTag("Movable");
        foreach (GameObject part in bodyParts)
        {
            initialBodyPositions[part] = part.transform.position;
            initialBodyRotations[part] = part.transform.rotation;
        }
    }

    public void ResetAll()
    {
        if (dragScript != null)
        {
            dragScript.DeselectAllObjects();
            dragScript.ResetDraggingState();
            //Debug.Log("Stopped dragging object.");
        }

        foreach (KeyValuePair<GameObject, Vector3> entry in initialBodyPositions)
        {
            GameObject part = entry.Key;
            part.transform.position = initialBodyPositions[part];
            part.transform.rotation = initialBodyRotations[part];
            //Debug.Log(part.name + " reset to initial position and rotation.");
        }

        if (mainCamera != null)
        {
            //Reset cam pos and rotation
            mainCamera.transform.position = initialCameraPosition;
            mainCamera.transform.rotation = initialCameraRotation;

            //Reset zoom
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = initialZoom; 
            }
        }
    }
}