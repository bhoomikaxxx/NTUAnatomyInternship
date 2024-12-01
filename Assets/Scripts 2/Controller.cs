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

    [Header("Rotation")]
    public Transform models;
    private Vector2 previousTouchPosition;
    private bool isRotating = false;

    //Camera states
    private CameraAction currentAction = CameraAction.None;
    private enum CameraAction { None, Panning, Zooming, Rotating }

    //Undo script ref
    private UndoScript historyManager;

    private void Awake()
    {
        //Add cam
        if (mainCamera == null)
            mainCamera = Camera.main;

        //Ref to Undo script
        historyManager = FindObjectOfType<UndoScript>();
    }

    private void Update()
    {
        MovementLogic();
    }

    private void MovementLogic()
    {
        if (Input.touchCount == 0 && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            currentAction = CameraAction.None;
            isRotating = false;
        }
        if (Input.touchCount == 2)
        {
            CamMovementTouch();
        }
        else if (Input.touchCount == 1 && currentAction != CameraAction.Zooming)
        {
            //Check for "Movable" obj
            if (!BodyTouchCheck())
            {
                //Else body rotation
                currentAction = CameraAction.Rotating;
                Rotate(true);
            }
            else
            {
                //Else cam panning
                currentAction = CameraAction.Panning;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            if (!BodyTouchCheck())
            {
                currentAction = CameraAction.Rotating;
                Rotate(false);
            }
            else
            {
                Debug.Log("panning action working");
                currentAction = CameraAction.Panning;
            }
        }
        else if (Input.GetMouseButton(2)) // Middle Mouse Button for panning
        {
            currentAction = CameraAction.Panning;
            PanCamera();
        }

        //Attach func to mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && currentAction != CameraAction.Rotating)
        {
            //Zoom 
            Zoom(scroll * 0.4f);
        }
    }

    //Camer pan func
    private void PanCamera()
    {
        //Get mouse pos
        Vector3 mousePosition = Input.mousePosition;
        Vector3 prevMousePosition = previousTouchPosition;

        Vector3 delta = mousePosition - prevMousePosition;

        //Convert from screen space to world space
        Vector3 worldPosCurrent = mainCamera.ScreenToWorldPoint(mousePosition);
        Vector3 worldPosPrevious = mainCamera.ScreenToWorldPoint(prevMousePosition);

        Vector3 panDelta = worldPosCurrent - worldPosPrevious;

        mainCamera.transform.position -= panDelta * 0.3f;

        previousTouchPosition = mousePosition;
    }


    //Check for touching body parts via tag "Movable"
    private bool BodyTouchCheck()
    {
        //Adding raycasting
        Ray ray;
        RaycastHit hit;

        if (Input.touchCount == 1)
        {
            ray = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            return false;
        }

        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.CompareTag("Movable");
        }

        return false;
    }

    private void CamMovementTouch()
    {
        //Get touch 
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        //Touch pos movement 
        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        //Check for zoom via difference in distance delta btwn 2 fingers
        float prevDistance = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentDistance = (touchZero.position - touchOne.position).magnitude;

        float distanceDelta = currentDistance - prevDistance;

        //Zoom logic
        if (Mathf.Abs(distanceDelta) > 1.0f)
        {
            Zoom(distanceDelta * 0.4f);
            currentAction = CameraAction.Zooming;
            Debug.Log("zooming");
        }
        //Pan logic
        else if ((touchZero.deltaPosition - touchOne.deltaPosition).magnitude > 2.0f)
        {
            Debug.Log("panning");
            Vector3 panDelta = mainCamera.ScreenToWorldPoint(touchZero.position) - mainCamera.ScreenToWorldPoint(touchZeroPrevPos);
            mainCamera.transform.position += panDelta * 1.5f * Time.deltaTime;
            currentAction = CameraAction.Panning;
            Debug.Log("panning");
        }
    }

    //Rotate func
    private void Rotate(bool isTouch)
    {
        //Touch logic
        if (isTouch)
        {
            //Get touch
            Touch touch = Input.GetTouch(0);

            //Get intial pos
            if (touch.phase == TouchPhase.Began)
            {
                isRotating = true;
                previousTouchPosition = touch.position;
                //historyManager.RecordState(models.gameObject, models.position, models.rotation);
            }
            //Rotate logic
            else if (touch.phase == TouchPhase.Moved && isRotating)
            {
                Vector2 touchDelta = touch.position - previousTouchPosition;
                previousTouchPosition = touch.position;

                RotateModelByDelta(touchDelta * 0.2f);
            }
        }
        //Mouse logic
        else
        {
            //Get intial pos
            if (Input.GetMouseButtonDown(1))
            {
                isRotating = true;
                previousTouchPosition = Input.mousePosition;
                //historyManager.RecordState(models.gameObject, models.position, models.rotation);
            }
            //Rotate logic
            else if (Input.GetMouseButton(1) && isRotating)
            {
                Vector2 mouseDelta = (Vector2)Input.mousePosition - previousTouchPosition;
                previousTouchPosition = Input.mousePosition;

                RotateModelByDelta(mouseDelta * 0.75f);
            }
        }
    }

    //Converting to zoom for ORTHO cam
    private void Zoom(float increment)
    {
        float targetZoom = Mathf.Clamp(mainCamera.orthographicSize - increment, 0.05f, 1.5f);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, 0.1f);
    }

    //Rotate code for ORTHO cam
    private void RotateModelByDelta(Vector2 delta)
    {
        float rotationX = delta.y * 2f * Time.deltaTime;
        float rotationY = -delta.x * 2f * Time.deltaTime;

        models.Rotate(Vector3.right, rotationX, Space.World);
        models.Rotate(Vector3.up, rotationY, Space.World);
    }
}

