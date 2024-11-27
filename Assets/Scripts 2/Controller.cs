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
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Zoom/Panning")]
    public float minZoom = 0.05f;
    public float maxZoom = 1.5f;
    public float panSensitivity = 1.5f;
    public float zoomSensitivity = 0.4f;
    public float panThreshold = 2.0f;
    public float zoomThreshold = 1.0f;
    public float smoothSpeed = 0.1f;

    [Header("Rotation")]
    public Transform models;
    public float rotationSpeed = 2f;
    public float sensitivityMultiplierMouse = 0.75f;
    public float sensitivityMultiplierTouch = 0.2f;

    private Vector2 previousTouchPosition;
    private bool isRotating = false;
    private CameraAction currentAction = CameraAction.None;

    private UndoScript historyManager;

    private enum CameraAction { None, Panning, Zooming, Rotating }

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        historyManager = FindObjectOfType<UndoScript>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.touchCount == 0 && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            currentAction = CameraAction.None;
            isRotating = false;
        }

        if (Input.touchCount == 2)
        {
            TouchZoomOrPan();
        }
        else if (Input.touchCount == 1 && currentAction != CameraAction.Zooming)
        {
            currentAction = CameraAction.Rotating;
            Rotate(true);
        }
        else if (Input.GetMouseButton(1))
        {
            currentAction = CameraAction.Rotating;
            Rotate(false);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && currentAction != CameraAction.Rotating)
        {
            Zoom(scroll * zoomSensitivity);
        }
    }

    private void TouchZoomOrPan()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevDistance = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentDistance = (touchZero.position - touchOne.position).magnitude;

        float distanceDelta = currentDistance - prevDistance;

        if (Mathf.Abs(distanceDelta) > zoomThreshold)
        {
            Zoom(distanceDelta * zoomSensitivity);
            currentAction = CameraAction.Zooming;
        }
        else if ((touchZero.deltaPosition - touchOne.deltaPosition).magnitude > panThreshold)
        {
            Vector3 panDelta = mainCamera.ScreenToWorldPoint(touchZero.position) - mainCamera.ScreenToWorldPoint(touchZeroPrevPos);
            mainCamera.transform.position += panDelta * panSensitivity * Time.deltaTime;
            currentAction = CameraAction.Panning;
        }
    }

    private void Rotate(bool isTouch)
    {
        if (isTouch)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isRotating = true;
                previousTouchPosition = touch.position;
                //historyManager.RecordState(models.gameObject, models.position, models.rotation);
            }
            else if (touch.phase == TouchPhase.Moved && isRotating)
            {
                Vector2 touchDelta = touch.position - previousTouchPosition;
                previousTouchPosition = touch.position;

                RotateModelByDelta(touchDelta * sensitivityMultiplierTouch);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                isRotating = true;
                previousTouchPosition = Input.mousePosition;
                //historyManager.RecordState(models.gameObject, models.position, models.rotation);
            }
            else if (Input.GetMouseButton(1) && isRotating)
            {
                Vector2 mouseDelta = (Vector2)Input.mousePosition - previousTouchPosition;
                previousTouchPosition = Input.mousePosition;

                RotateModelByDelta(mouseDelta * sensitivityMultiplierMouse);
            }
        }
    }

    private void Zoom(float increment)
    {
        float targetZoom = Mathf.Clamp(mainCamera.orthographicSize - increment, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, smoothSpeed);
    }

    private void RotateModelByDelta(Vector2 delta)
    {
        float rotationX = delta.y * rotationSpeed * Time.deltaTime;
        float rotationY = -delta.x * rotationSpeed * Time.deltaTime;

        models.Rotate(Vector3.right, rotationX, Space.World);
        models.Rotate(Vector3.up, rotationY, Space.World);
    }
}

