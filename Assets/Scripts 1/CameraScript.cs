/*
 * Name: Bhoomika Manot
 * Date: 19 September 2024
 * Description: Script for orthographic camera panning and zooming on smartboard using touch fingers and desktop using mouse.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using UnityEngine.UI;

public class CameraScript : MonoBehaviour
{
    //Declaration
    Vector3 touchStart;
    public float minZoom = 0.05f;
    public float maxZoom = 1.5f;
    public float panSensitivity = 0.5f;
    public float zoomSensitivity = 0.01f;

    //Script
    public DragObj dragObjScript;

    //Selection toggles
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;

    void Update()
    {
        //Check for dragging
        if (!dragObjScript.isDragging)
        {
            HandleCameraMovement();
        }
    }

    //Camera movement (panning and zooming)
    void HandleCameraMovement()
    {
        //Disable camera movement if any selection toggle is on
        if (singleSelectToggle.isOn || multiSelectToggle.isOn)
        {
            return;
        }

        //Mouse Input (Using middle mouse click to pan)
        if (Input.GetMouseButtonDown(2))
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(2))
        {
            Vector3 direction = (touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * panSensitivity;
            Camera.main.transform.position += direction;
        }

        //Touch Input (1 finger swipe to pan)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = Camera.main.ScreenToWorldPoint(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 direction = (touchStart - Camera.main.ScreenToWorldPoint(touch.position)) * panSensitivity;
                Camera.main.transform.position += direction;
            }
        }

        //Touch Input(2 finger pinch to zoom)
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            //Check for zoom distance via finger pos on screen 
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            if (Mathf.Abs(difference) > 0.01f)
            {
                Zoom(difference * zoomSensitivity);
            }
        }

        //Mouse Input (Scroll wheel to zoom)
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Zoom(Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    //Zoom clamp
    void Zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, minZoom, maxZoom);
    }
}







/*
//Touch Zoom with touch (Using 2 finger pinch)
if (Input.touchCount == 2)
{
    Touch touch0 = Input.GetTouch(0);
    Touch touch1 = Input.GetTouch(1);

    if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
    {

        float prevTouchDeltaMag = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
        float touchDeltaMag = (touch0.position - touch1.position).magnitude;

        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        mainCamera.orthographicSize += deltaMagnitudeDiff * zoomSBSpeed;

        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
    }
}
*/


