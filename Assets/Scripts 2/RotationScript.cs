/*
 * Name: Bhoomika Manot
 * Date: 16 September 2024
 * Description: Script for model/asset rotation on smartboard using touch fingers and desktop using mouse. 
 * Mouse - Right click
 * Finger - 1 finger rotation
 * UPDATE: MOVED TO CONTOLLER SCRIPT
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RotationScript : MonoBehaviour
{
    /* //Declaration
    public Transform models;
    private Vector2 previousTouchPosition;
    private Vector2 currentDelta;

    //Controls
    private bool isRotating = false;
    public float rotationSpeed = 2f;
    public float sensitivityMultiplierMouse = 0.75f;
    public float sensitivityMultiplierTouch = 0.1f;
    public float smoothFactor = 0.1f; 

    void Update()
    {
        //Mouse Input (Right mouse button)
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            previousTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1) && isRotating)
        {
            //Calculate distance
            Vector2 mouseDelta = (Vector2)Input.mousePosition - previousTouchPosition;
            previousTouchPosition = Input.mousePosition;

            //Smoother movement
            currentDelta = Vector2.Lerp(currentDelta, mouseDelta * sensitivityMultiplierMouse, smoothFactor);
            RotateModelByDelta(currentDelta);
        }
        //Detects if rotation is finished
        else if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        //1 finger touch rotation
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            //Check for the touch
            if (touch.phase == TouchPhase.Began)
            {
                //Check for tag
                if (!IsPointerOverMovableObject()) 
                {
                    isRotating = true;
                    previousTouchPosition = touch.position;
                }
            }
            //Detects if finger is moving
            else if (touch.phase == TouchPhase.Moved && isRotating)
            {
                //Calculate distance moved
                Vector2 touchDelta = touch.position - previousTouchPosition;
                previousTouchPosition = touch.position;

                //Smoother movement
                currentDelta = Vector2.Lerp(currentDelta, touchDelta * sensitivityMultiplierTouch, smoothFactor);
                RotateModelByDelta(currentDelta);
            }
            //Detects if finger is lifted to stop
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isRotating = false;
            }
        }

        //Allow rotation in world
        if (Input.GetMouseButton(1) && !IsPointerOverMovableObject())
        {
            isRotating = true;
            Vector2 mouseDelta = (Vector2)Input.mousePosition - previousTouchPosition;
            previousTouchPosition = Input.mousePosition;
            RotateModelByDelta(mouseDelta * sensitivityMultiplierTouch);
        }
    }

    //Function for rotating
    void RotateModelByDelta(Vector2 delta)
    {
        //Rotation
        float rotationX = delta.y * rotationSpeed * Time.deltaTime;
        float rotationY = -delta.x * rotationSpeed * Time.deltaTime;

        models.Rotate(Camera.main.transform.right, rotationX, Space.World);
        models.Rotate(Vector3.up, rotationY, Space.World);
    }

    //Check for finger location
    private bool IsPointerOverMovableObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //Check for tag
            return hit.collider.CompareTag("Movable");
        }
        return false;
    } */
}


