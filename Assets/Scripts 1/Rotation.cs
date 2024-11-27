/*
 * Name: Bhoomika Manot
 * Date: 16 September 2024
 * Description: Script for model/asset rotation on smartboard using touch fingers and desktop using mouse.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;
//using Lean.Touch;

public class Rotation : MonoBehaviour
{
    //Declaration
    public Transform models;
    public float rotationSpeed = 2f;  
    public float sensitivityMultiplier = 0.25f; 

    private Vector2 initialTouchPosition1;
    private Vector2 initialTouchPosition2;
    private Vector2 previousMidpoint;

    private bool isRotating = false;

    void Update()
    {
        //Mouse Input (Right mouse button to rotate)
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;

            //Stores current mouse pos
            previousMidpoint = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1) && isRotating)
        {
            //Claculate distance btwn prev and current mouse pos
            Vector2 mouseDelta = (Vector2)Input.mousePosition - previousMidpoint;
            previousMidpoint = Input.mousePosition;

            //Rotate less sensitively
            RotateModelByDelta(mouseDelta * sensitivityMultiplier);
        }
        //Stop rotation
        else if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        //2 finger touch rotation
        if (Input.touchCount == 2)
        {
            //Label touches
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            //If fingers are touching
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                isRotating = true;

                //Declare pos
                initialTouchPosition1 = touch1.position;
                initialTouchPosition2 = touch2.position;

                //Calc average prev pos 
                previousMidpoint = (initialTouchPosition1 + initialTouchPosition2) / 2;
            }
            //If fingers are moving
            else if ((touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved) && isRotating)
            {
                //Record pos
                Vector2 currentTouchPosition1 = touch1.position;
                Vector2 currentTouchPosition2 = touch2.position;

                //Calc average current pos 
                Vector2 currentMidpoint = (currentTouchPosition1 + currentTouchPosition2) / 2;

                //Calc distance moved
                Vector2 touchDelta = currentMidpoint - previousMidpoint;

                previousMidpoint = currentMidpoint;

                //Rotation func
                RotateModelByDelta(touchDelta * sensitivityMultiplier);
            }
            //Stop 1 finger is off screen
            else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Canceled)
            {
                isRotating = false;
            }
        }
    }

    //Func for rotating
    void RotateModelByDelta(Vector2 delta)
    {
        //Rotation
        float rotationX = delta.y * rotationSpeed * Time.deltaTime; 
        float rotationY = -delta.x * rotationSpeed * Time.deltaTime;

        models.Rotate(Camera.main.transform.right, rotationX, Space.World);  
        models.Rotate(Vector3.up, rotationY, Space.World);  
    }
}

    //Touch Input for mobile (Using 3 fingers)
    //Using Lean Touch

    /*
    //Touch Input for mobile (Using 3 fingers)
    if (Input.touchCount == 3)
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        Touch touch2 = Input.GetTouch(2);

        if (Input.touchCount == 3 && touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
        {
            Vector2 averageDeltaPosition = (touch0.deltaPosition + touch1.deltaPosition + touch2.deltaPosition) / 3f;

            float deltaX = averageDeltaPosition.x * rotationSBSpeed * Time.deltaTime;
            float deltaY = averageDeltaPosition.y * rotationSBSpeed * Time.deltaTime;

            rotX += deltaY;
            rotY += deltaX;

            if (models != null)
            {
                models.transform.rotation = initialRotation * Quaternion.Euler(rotX, rotY, 0f);
            }
        }
    }


    [SerializeField] private InputAction pressed, axis;
    [SerializeField] private float speed = 1;

    private Transform cam;

    private Vector2 rotation; 
    private bool rotateAllowed;

    private void Awake()
    {
        cam = Camera.main.transform;

        pressed.Enable();
        axis.Enable();

        pressed.performed += _ => { StartCoroutine(Rotate()); };
        pressed.canceled += _ => { rotateAllowed = false; };

        axis.performed += context => { rotation = context.ReadValue<Vector2>(); };
    }

    private IEnumerator Rotate()
    {
        rotateAllowed = true;
        while (rotateAllowed)
        {
            rotation *= speed;
            transform.Rotate(Vector3.up, rotation.x, Space.World);
            transform.Rotate(cam.right, rotation.y, Space.World);

            yield return null;
        }
    } */





