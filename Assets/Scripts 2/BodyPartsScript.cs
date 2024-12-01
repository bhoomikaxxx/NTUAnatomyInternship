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
    //private const float doubleClickThreshold = 2f;

    //Camera zoom settings
    private Vector3 originalCameraPosition;
    private float originalCameraSize;
    public float targetOrthographicSize = 0.5f;  
    public float zoomDuration = 0.5f;  
    private bool isZooming = false;

    public void Awake()
    {
        //Add cam
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        //Initial camera pos and size
        originalCameraPosition = mainCamera.transform.position;
        originalCameraSize = mainCamera.orthographicSize;

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
        //Check for left mouse click or touch start
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            //Get position of the input (mouse or touch)
            Vector3 screenPosition = GetInputPosition();
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Movable"))
            {
                isTouchingMovable = true;
                GameObject bodyPart = hit.collider.gameObject;

                //Check for time between clicks 
                float currentTime = Time.time;

                if (isMultiSelect && selectedBodyParts.Contains(bodyPart))
                {
                    //Check for double-click (mouse) or double-tap (touch)
                    if (currentTime - lastClickTime < 1f)
                    {
                        //Deselect
                        selectedBodyParts.Remove(bodyPart);
                        dragOffsets.Remove(bodyPart);

                        //Label
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

                        // Update label with the selected body part's name
                        labelText.text = "Selected Part: " + bodyPart.name;
                    }
                }
                lastClickTime = currentTime; // Update last click time
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

        // Isolate function to zoom and deactivate other body parts
        public void Isolate()
    {
        if (isZooming || selectedBodyParts.Count == 0) return;

        StartCoroutine(IsolateCoroutine());
    }

    private IEnumerator IsolateCoroutine()
    {
        isZooming = true;
        // Save the current camera position and size for later Deisolate
        Vector3 originalPos = mainCamera.transform.position;
        float originalSize = mainCamera.orthographicSize;

        // Zoom in
        Vector3 targetPosition;
        if (isMultiSelect)
        {
            // If Multi Select is on, find the center of selected body parts
            targetPosition = CalculateCenterPoint(selectedBodyParts);
        }
        else
        {
            // If Single Select is on, isolate the last touched body part
            targetPosition = selectedBodyParts[selectedBodyParts.Count - 1].transform.position;
        }

        yield return ZoomToPosition(targetPosition, targetOrthographicSize);

        // Deactivate all other body parts
        foreach (GameObject part in bodyParts)
        {
            part.SetActive(false);
        }

        // Activate only the selected body parts
        foreach (GameObject selectedPart in selectedBodyParts)
        {
            selectedPart.SetActive(true);
        }

        isZooming = false;
    }

    // Deisolate function to return camera to original position and size
    public void Deisolate()
    {
        StartCoroutine(DeisolateCoroutine());
    }

    private IEnumerator DeisolateCoroutine()
    {
        isZooming = true;

        // Reactivate all body parts
        foreach (GameObject part in bodyParts)
        {
            part.SetActive(true);
        }

        // Zoom back to the original camera position and size
        yield return ZoomToPosition(originalCameraPosition, originalCameraSize);

        isZooming = false;
    }

    // Zoom to a target position and size
    private IEnumerator ZoomToPosition(Vector3 targetPosition, float targetSize)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            mainCamera.transform.position = Vector3.Lerp(startPosition, new Vector3(targetPosition.x, targetPosition.y, startPosition.z), elapsedTime / zoomDuration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsedTime / zoomDuration);
            yield return null;
        }

        mainCamera.transform.position = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);
        mainCamera.orthographicSize = targetSize;
    }

    // Helper function to calculate the center point of selected body parts
    private Vector3 CalculateCenterPoint(List<GameObject> objects)
    {
        if (objects.Count == 1) return objects[0].transform.position;

        Bounds bounds = new Bounds(objects[0].transform.position, Vector3.zero);
        foreach (GameObject obj in objects)
        {
            bounds.Encapsulate(obj.transform.position);
        }
        return bounds.center;
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

