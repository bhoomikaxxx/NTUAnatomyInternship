/*
 * Name: Bhoomika Manot
 * Date: 30 September 2024
 * Description: Script for touching an individual body part and dragging in 3D space with camera zoom functionality [NOT IN USE]
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BodyPartsScript : MonoBehaviour
{
    //Declarations
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Models")]
    public List<GameObject> bodyParts = new List<GameObject>();

    [Header("Text")]
    public TMP_Text labelText;

    public GameObject selectedBodyPart = null;
    public bool isDragging = false;
    public bool isTouchingMovable = false;

    [Header("Toggles")]
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;

    //Multi select 
    public bool isMultiSelect = false;
    public List<GameObject> selectedBodyParts = new List<GameObject>();
    public Dictionary<GameObject, Vector3> dragOffsets = new Dictionary<GameObject, Vector3>();

    //Undo script ref
    private UndoScript historyManager;

    //Double click deselect for multi select
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 3f;

    public void Awake()
    {
        //Add cam
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        //Ref to Undo script
        historyManager = FindObjectOfType<UndoScript>();

        //Add single select toggle
        if (singleSelectToggle != null)
        {
            singleSelectToggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    //Multi select is OFF when single select is on
                    isMultiSelect = false;
                    multiSelectToggle.isOn = false;
                }
            });
        }

        // Add multi select toggle
        if (multiSelectToggle != null)
        {
            multiSelectToggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    //Single select is OFF when multi select is on
                    isMultiSelect = true;
                    singleSelectToggle.isOn = false;
                }
            });
        }
    }

    public void Update()
    {
        Dragging();
    }

    //Drag check func
    public void Dragging()
    {
        //Check for right mouse click/touch
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            //Get pos
            Vector3 screenPosition = GetInputPosition();
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            //Check if body part
            //To prevent intefering with model rotation (also single finger drag)
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Movable"))
            {
                isTouchingMovable = true;
                GameObject bodyPart = hit.collider.gameObject;

                //Check for time btwn touch
                float currentTime = Time.time;

                if (isMultiSelect && selectedBodyParts.Contains(bodyPart))
                {
                    if (currentTime - lastClickTime < doubleClickThreshold)
                    {
                        //Double click less than 3s to deselect
                        selectedBodyParts.Remove(bodyPart);
                        dragOffsets.Remove(bodyPart);

                        //Count
                        labelText.text = $"Selected {selectedBodyParts.Count} Parts";
                    }
                }
                else
                {
                    //Multi select add
                    if (isMultiSelect)
                    {
                        selectedBodyParts.Add(bodyPart);
                        dragOffsets[bodyPart] = Vector3.zero;
                        labelText.text = $"Selected {selectedBodyParts.Count} Parts";
                    }
                    //Single select add
                    else if (singleSelectToggle.isOn)
                    {
                        selectedBodyParts.Clear();
                        dragOffsets.Clear();
                        selectedBodyParts.Add(bodyPart);
                        dragOffsets[bodyPart] = Vector3.zero;

                        //Label
                        labelText.text = "Selected Part: " + bodyPart.name;
                    }
                }
                lastClickTime = currentTime;
            }
        }

        //Dragging check
        if (isTouchingMovable && (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)))
        {
            if (!isDragging && selectedBodyParts.Count > 0)
            {
                Drag();
            }
            if (isDragging)
            {
                MoveBodyParts(selectedBodyParts);
            }
        }

        //Dragging ended logic
        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            StopDrag();
        }
    }

    //Drag func
    public void Drag()
    {
        isDragging = true;
        Vector3 screenPosition = GetInputPosition();

        foreach (GameObject bodyPart in selectedBodyParts)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.WorldToScreenPoint(bodyPart.transform.position).z));
            dragOffsets[bodyPart] = bodyPart.transform.position - worldPosition;
        }
    }

    //Drag ended func
    public void StopDrag()
    {
        isDragging = false;
        isTouchingMovable = false;

        //Record final state
        foreach (GameObject bodyPart in selectedBodyParts)
        {
            historyManager.RecordState(bodyPart, bodyPart.transform.position, bodyPart.transform.rotation);
        }
        selectedBodyPart = null;
    }

    //Multi select list logic
    public void MoveBodyParts(List<GameObject> bodyParts)
    {
        Vector3 screenPosition = GetInputPosition();

        foreach (GameObject bodyPart in bodyParts)
        {
            if (dragOffsets.TryGetValue(bodyPart, out Vector3 offset))
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.WorldToScreenPoint(bodyPart.transform.position).z));
                bodyPart.transform.position = Vector3.Lerp(bodyPart.transform.position, worldPosition + offset, 0.5f);
            }
        }
    }

    //Pos logic
    public Vector3 GetInputPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        return Input.mousePosition;
    }
}




//Coroutine to zoom to the selected object
/*private IEnumerator ZoomToSelectedObject(GameObject target)
{
    isZooming = true;

    float elapsedTime = 0f;
    Vector3 initialPosition = mainCamera.transform.position;
    Vector3 targetPosition = new Vector3(target.transform.position.x, target.transform.position.y, mainCamera.transform.position.z);

    float initialSize = mainCamera.orthographicSize;

    //Perform zoom over time
    while (elapsedTime < zoomDuration)
    {
        elapsedTime += Time.deltaTime;

        //Lerp the camera's position and orthographic size
        mainCamera.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / zoomDuration);
        mainCamera.orthographicSize = Mathf.Lerp(initialSize, targetOrthographicSize, elapsedTime / zoomDuration);

        yield return null;
    }

    //Ensure camera is set to exact values after zooming
    mainCamera.transform.position = targetPosition;
    mainCamera.orthographicSize = targetOrthographicSize;

    //Wait for some time and then zoom out
    yield return new WaitForSeconds(1f);  

    // oom out back to normal
    elapsedTime = 0f;
    while (elapsedTime < zoomDuration)
    {
        elapsedTime += Time.deltaTime;

        //Lerp back to the original position and size
        mainCamera.transform.position = Vector3.Lerp(targetPosition, initialPosition, elapsedTime / zoomDuration);
        mainCamera.orthographicSize = Mathf.Lerp(targetOrthographicSize, normalOrthographicSize, elapsedTime / zoomDuration);

        yield return null;
    }

    //Ensure camera is back to normal values
    mainCamera.transform.position = initialPosition;
    mainCamera.orthographicSize = normalOrthographicSize;

    isZooming = false;
}
*/



/*
public List<GameObject> bodyParts = new List<GameObject>();

void Update()
{
    //Handle mouse input (for desktop)
    if (Input.GetMouseButtonDown(0))
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        RaycastHit hit;

        if (Physics.Raycast(worldPosition, Camera.main.transform.forward, out hit))
        {
            GameObject clickedObject = hit.collider.gameObject;
            Debug.Log("GameObject clicked: " + clickedObject.name);

            if (bodyParts.Contains(clickedObject))
            {
                Debug.Log("Clicked on a body part: " + clickedObject.name);
            }
        }
    }

    //Handle touch input (for mobile)
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, Camera.main.nearClipPlane));
        RaycastHit hit;

        if (Physics.Raycast(touchPosition, Camera.main.transform.forward, out hit))
        {
            GameObject touchedObject = hit.collider.gameObject;
            Debug.Log("GameObject touched: " + touchedObject.name);

            if (bodyParts.Contains(touchedObject))
            {
                Debug.Log("Touched a body part: " + touchedObject.name);
            }
        }
    }
}
*/

