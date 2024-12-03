/*
 * Name: Bhoomika Manot
 * Date: 16 September 2024
 * Description: Script for model/asset rotation on smartboard using touch fingers and desktop using mouse. 
 * Mouse - Right click
 * Finger - 1 finger rotation
 */
/*
 * Name: Bhoomika Manot
 * Date: 19 September 2024
 * Description: Script for orthographic camera panning and zooming on smartboard using touch fingers and desktop using mouse.
 * Mouse Zoom - Middle mouse scroll
 * Finger Zoom - 2 finger pinch
 * Mouse Pan - Middle mouse hold click and move
 * Finger Pan - 2 finger drag
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour
{
    //Declarations
    [Header("Camera")]
    public Camera mainCamera;
    private Vector3 touchStart;

    [Header("Rotation")]
    public Transform models;
    private Vector2 previousTouchPosition;
    private Vector2 currentDelta;
    private bool isRotating = false;

    //Cam states
    private enum CameraAction { None, Panning, Zooming, Rotating }
    private CameraAction currentAction = CameraAction.None;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        //Reset 
        if (Input.touchCount == 0 && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            currentAction = CameraAction.None;
        }

        if (Input.touchCount == 2)
        {
            CameraMovementTouch();
            currentAction = CameraAction.Zooming; 
        }
        else if (Input.touchCount == 1 && currentAction != CameraAction.Zooming)
        {
            currentAction = CameraAction.Rotating;
            RotateUpdate();
        }
        else if (Input.GetMouseButton(2))
        {
            CameraMovementMousePan();
            currentAction = CameraAction.Panning;
        }
        else if (Input.GetMouseButton(1))
        {
            currentAction = CameraAction.Rotating;
            RotateUpdate();
        }
        //Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && currentAction != CameraAction.Rotating)
        {
            Zoom(scroll * 0.5f);
            currentAction = CameraAction.Zooming;
        }
    }

    //Touch cam logic
    void CameraMovementTouch()
    {
        //Get touch pos
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        //Calc distance btwn fingers
        float prevDistance = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentDistance = (touchZero.position - touchOne.position).magnitude;
        float distanceDelta = currentDistance - prevDistance;

        Vector2 currentMidpoint = (touchZero.position + touchOne.position) / 2;
        Vector2 prevMidpoint = (touchZeroPrevPos + touchOnePrevPos) / 2;
        float midpointDelta = (currentMidpoint - prevMidpoint).magnitude;

        //If significant change in distance, then assume zoom
        if (Mathf.Abs(distanceDelta) > 1.0f && Mathf.Abs(distanceDelta) > midpointDelta)
        {
            currentAction = CameraAction.Zooming;
            Zoom(distanceDelta * 0.5f);
        }
        //Pan logic
        else if (midpointDelta > 2.0f)
        {
            currentAction = CameraAction.Panning;
            Vector3 panStartPos = mainCamera.ScreenToWorldPoint(new Vector3(currentMidpoint.x, currentMidpoint.y, mainCamera.nearClipPlane));
            Vector3 panEndPos = mainCamera.ScreenToWorldPoint(new Vector3(prevMidpoint.x, prevMidpoint.y, mainCamera.nearClipPlane));
            Vector3 panDirection = panEndPos - panStartPos;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, mainCamera.transform.position + panDirection * 1.25f, 0.1f);
        }
    }

    //Cam mouse pan via middle click btn
    void CameraMovementMousePan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            touchStart = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(2))
        {
            Vector3 direction = touchStart - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, mainCamera.transform.position + direction * 1.25f, 0.1f);
        }
    }

    //Zoom logic for ORTHO cam
    void Zoom(float increment)
    {
        float targetZoom = Mathf.Clamp(mainCamera.orthographicSize - increment, 0.05f, 1.5f);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, 0.1f);
    }

    //Rotate func
    void RotateUpdate()
    {
        if (currentAction == CameraAction.Rotating)
        {
            //Only allow rotation if not touching a "Movable" obj
            if (Input.touchCount == 1 && !IsPointerOverMovableObject())
            {
                Touch touch = Input.GetTouch(0);

                //Detect touch
                if (touch.phase == TouchPhase.Began)
                {
                    isRotating = true;
                    previousTouchPosition = touch.position;
                }
                //Rotate logic
                else if (touch.phase == TouchPhase.Moved && isRotating)
                {
                    Vector2 touchDelta = touch.position - previousTouchPosition;
                    previousTouchPosition = touch.position;

                    currentDelta = touchDelta * 0.15f;
                    RotateModelByDelta(currentDelta);
                }
                //Stop rotating
                else if (touch.phase == TouchPhase.Ended)
                {
                    isRotating = false;
                }
            }

            //Mouse logic
            if (Input.GetMouseButtonDown(1))
            {
                isRotating = true;
                previousTouchPosition = Input.mousePosition;
            }
            //Rotate logic
            else if (Input.GetMouseButton(1) && isRotating)
            {
                Vector2 mouseDelta = (Vector2)Input.mousePosition - previousTouchPosition;
                previousTouchPosition = Input.mousePosition;

                currentDelta = mouseDelta * 0.75f;
                RotateModelByDelta(currentDelta);
            }
            //Stop rotating
            else if (Input.GetMouseButtonUp(1))
            {
                isRotating = false;
            }
        }
    }

    //Check for body part - diff func [drag bdoy part(s)] in BodyPartsScript
    public bool IsPointerOverMovableObject()
    {
        if (Input.touchCount > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            return Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Movable");
        }
        return false;
    }

    //Rotate code for ORTHO cam
    void RotateModelByDelta(Vector2 delta)
    {
        float rotationX = delta.y * 2f * Time.deltaTime;
        float rotationY = -delta.x * 2f * Time.deltaTime;

        models.Rotate(Camera.main.transform.right, rotationX, Space.World);
        models.Rotate(Vector3.up, rotationY, Space.World);
    }
}