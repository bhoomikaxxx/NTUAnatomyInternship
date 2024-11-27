/*
 * Name: Bhoomika Manot
 * Date: 30 September 2024
 * Description: Script for touching an individual body part and dragging in 3D space with camera zoom functionality
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragObj : MonoBehaviour
{
    // Declaration
    public Camera mainCamera;
    public List<GameObject> bodyParts = new List<GameObject>();
    private List<GameObject> selectedObjects = new List<GameObject>();
    private GameObject selectedObject = null;

    // UI
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;
    public TMP_Text resultText;

    // Sensitivity
    public float dragSensitivity = 0.5f;

    // Outline
    private RaycastHit raycastHit;

    // Dictionary to store original scales and parents
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Transform> originalParents = new Dictionary<GameObject, Transform>();

    internal bool isDragging;
    private Vector3 lastMousePosition; 

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // Check for toggle
        if (singleSelectToggle.isOn)
        {
            HandleSingleSelection();
            HandleSingleDragging();
        }
        else if (multiSelectToggle.isOn)
        {
            HandleMultiSelection();
            HandleMultiDragging();
        }
    }

    private void OutlineSelected(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
            outline.OutlineColor = Color.magenta;
            outline.OutlineWidth = 6.0f;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
        }

        // Enable the outline
        outline.enabled = true;
    }

    // Single selection
    public void HandleSingleSelection()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 screenPosition = GetInputPosition();
            if (Physics.Raycast(mainCamera.ScreenPointToRay(screenPosition), out raycastHit))
            {
                GameObject clickedObject = raycastHit.collider.gameObject;

                // Ensure only selecting the clicked GameObject and ignoring hierarchy.
                if (clickedObject.CompareTag("Movable"))
                {
                    // Clear previous selection
                    if (selectedObject != null)
                    {
                        DeselectObject(selectedObject);
                    }

                    selectedObject = clickedObject;

                    // Unparent the object from its hierarchy so it moves individually
                    UnparentObject(selectedObject);

                    // Store original scale and parent
                    if (!originalScales.ContainsKey(selectedObject))
                    {
                        originalScales[selectedObject] = selectedObject.transform.localScale;
                    }
                    if (!originalParents.ContainsKey(selectedObject))
                    {
                        originalParents[selectedObject] = selectedObject.transform.parent;
                    }

                    OutlineSelected(selectedObject);
                    selectedObject.transform.localScale = originalScales[selectedObject];

                    // Display name
                    resultText.text = "Name: " + selectedObject.name;
                }
            }
        }
    }

    // Dragging (single selection)
    public void HandleSingleDragging()
    {
        if (selectedObject != null)
        {
            if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
            {
                MoveObject(selectedObject);
            }
            else if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
            {
                ReparentObject(selectedObject); // Reparent after dragging
                DeselectObject(selectedObject);
            }
        }
    }

    // Multi-selection
    private void HandleMultiSelection()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 screenPosition = GetInputPosition();
            if (Physics.Raycast(mainCamera.ScreenPointToRay(screenPosition), out raycastHit))
            {
                GameObject clickedObject = raycastHit.collider.gameObject;

                if (clickedObject.CompareTag("Movable"))
                {
                    if (!selectedObjects.Contains(clickedObject))
                    {
                        // Add the clicked object to the list
                        selectedObjects.Add(clickedObject);

                        // Unparent the object from its hierarchy
                        UnparentObject(clickedObject);

                        // Store original scale and parent
                        if (!originalScales.ContainsKey(clickedObject))
                        {
                            originalScales[clickedObject] = clickedObject.transform.localScale;
                        }
                        if (!originalParents.ContainsKey(clickedObject))
                        {
                            originalParents[clickedObject] = clickedObject.transform.parent;
                        }

                        OutlineSelected(clickedObject);
                        clickedObject.transform.localScale = originalScales[clickedObject];
                    }
                    else
                    {
                        // Remove the object
                        ReparentObject(clickedObject); // Reparent when deselected
                        DeselectObject(clickedObject);
                        selectedObjects.Remove(clickedObject);
                    }
                }
            }
        }
    }

    // Dragging the selected objects for multi-selection mode
    private void HandleMultiDragging()
    {
        if (selectedObjects.Count > 0)
        {
            if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
            {
                // Calculate mouse movement delta
                Vector3 currentMousePosition = GetInputPosition();
                Vector3 mouseDelta = currentMousePosition - lastMousePosition;

                // Move all selected objects in the same direction and by the same amount
                foreach (var obj in selectedObjects)
                {
                    Vector3 newWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, mainCamera.WorldToScreenPoint(obj.transform.position).z));
                    Vector3 previousWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(lastMousePosition.x, lastMousePosition.y, mainCamera.WorldToScreenPoint(obj.transform.position).z));

                    Vector3 movement = newWorldPosition - previousWorldPosition;
                    obj.transform.position += movement * dragSensitivity;
                }

                lastMousePosition = currentMousePosition; // Update last mouse position
            }
            else if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
            {
                foreach (var obj in selectedObjects)
                {
                    ReparentObject(obj); // Reparent after dragging
                    DeselectObject(obj);
                }
                selectedObjects.Clear();
            }
        }
    }

    // Move an object based on current mouse/touch position
    private void MoveObject(GameObject obj)
    {
        Vector3 screenPosition = GetInputPosition();
        float objectZPosition = obj.transform.position.z;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.WorldToScreenPoint(obj.transform.position).z));

        obj.transform.position = Vector3.Lerp(obj.transform.position, new Vector3(worldPosition.x, worldPosition.y, objectZPosition), dragSensitivity);
    }

    // Unparent the object
    private void UnparentObject(GameObject obj)
    {
        obj.transform.SetParent(null); // Unparent the object to move it independently
    }

    // Reparent the object back to its original parent
    private void ReparentObject(GameObject obj)
    {
        if (originalParents.ContainsKey(obj))
        {
            obj.transform.SetParent(originalParents[obj]);
        }
    }

    private void DeselectObject(GameObject obj)
    {
        if (obj != null)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false; // Disable the outline
            }

            // Reset the object's original scale
            if (originalScales.ContainsKey(obj))
            {
                obj.transform.localScale = originalScales[obj];
            }
        }
    }

    private Vector3 GetInputPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        return Input.mousePosition;
    }

    public void DeselectAllObjects()
    {
        foreach (GameObject obj in selectedObjects)
        {
            ReparentObject(obj); // Reparent all selected objects before clearing the list
            DeselectObject(obj);
        }
        selectedObjects.Clear();
    }

    // Reset dragging state and deselect all objects (both single and multi-selection)
    public void ResetDraggingState()
    {
        ReparentObject(selectedObject);
        DeselectObject(selectedObject);
        DeselectAllObjects();
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
