/*
 * Name: Bhoomika Manot
 * Date: 30 September 2024
 * Description: Script for touching an individual body part and dragging in 3D space with camera zoom functionality [NOT IN USE]
 */
/*
 * Name: Bhoomika Manot
 * Date: 1 Dec 2024
 * Description: Script for isolating body part with camera zoom functionality 
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
    //private List<GameObject> previouslyActiveBodyParts = new List<GameObject>();

    public GameObject selectedBodyPart = null;
    public bool isDragging = false;
    public bool isTouchingMovable = false;

    [Header("Text")]
    public TMP_Text labelText;

    [Header("Toggles")]
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;

    //Multi select 
    public bool isMultiSelect = false;
    public List<GameObject> selectedBodyParts = new List<GameObject>();
    public Dictionary<GameObject, Vector3> dragOffsets = new Dictionary<GameObject, Vector3>();

    //Undo script ref
    //private UndoScript historyManager;

    //Double click deselect for multi select
    private float lastClickTime = 0f;

    public void Awake()
    {
        //Add cam
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        //Ref to Undo script
        //historyManager = FindObjectOfType<UndoScript>();

        //Add single select toggle
        if (singleSelectToggle != null)
        {
            singleSelectToggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    isMultiSelect = false;
                    multiSelectToggle.isOn = false;
                    ClearOutlines();
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
                    isMultiSelect = true;
                    singleSelectToggle.isOn = false;
                    ClearOutlines();
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

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Movable"))
                {
                    GameObject bodyPart = hit.collider.gameObject;
                    Debug.Log("Collider working");

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
                            RemoveOutline(bodyPart);
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
                            OutlineSelected(bodyPart);
                            labelText.text = $"Selected {selectedBodyParts.Count} Parts";
                        }
                        //Single select add
                        else if (!isMultiSelect && singleSelectToggle.isOn)
                        {
                            selectedBodyParts.Clear();
                            dragOffsets.Clear();
                            selectedBodyParts.Add(bodyPart);
                            dragOffsets[bodyPart] = Vector3.zero;
                            ClearOutlines();
                            OutlineSelected(bodyPart);
                            //Update label
                            labelText.text = "Selected Part: " + bodyPart.name;
                        }
                    }
                    lastClickTime = currentTime; 
                }
            }
        }

        //Dragging - left mouse/touch
        if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
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

    // Outline a selected body part
    private void OutlineSelected(GameObject bodyPart)
    {
        var outline = bodyPart.GetComponent<Outline>();
        if (outline == null)
        {
            outline = bodyPart.AddComponent<Outline>();
            outline.OutlineColor = Color.magenta;
            outline.OutlineWidth = 6.0f;
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.GetComponent<Renderer>().material.SetInt("_ZWrite", 100);
            //outline.GetComponent<Renderer>().material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }
        outline.enabled = true;
    }

    // Remove outline from a body part
    private void RemoveOutline(GameObject bodyPart)
    {
        var outline = bodyPart.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }

    // Clear outlines from all selected body parts
    private void ClearOutlines()
    {
        foreach (GameObject bodyPart in selectedBodyParts)
        {
            RemoveOutline(bodyPart);
        }
    }

    //Drag func
    public void Drag()
    {
        isDragging = true;
        Vector3 screenPosition = GetInputPosition();

        foreach (GameObject bodyPart in selectedBodyParts)
        {
            // Calculate drag offsets only when dragging starts
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.WorldToScreenPoint(bodyPart.transform.position).z));
            dragOffsets[bodyPart] = bodyPart.transform.position - worldPosition;
        }
    }

    //Drag ended func
    public void StopDrag()
    {
        isDragging = false;

        // Record the final position and rotation of all selected body parts
        foreach (GameObject bodyPart in selectedBodyParts)
        {
            //historyManager.RecordState(bodyPart, bodyPart.transform.position, bodyPart.transform.rotation);
        }

        selectedBodyPart = null;
    }

    //Single movement
    public void MoveBodyPart(GameObject bodyPart)
    {
        if (isDragging)
        {
            //Recalculate pos according to movement
            Vector3 screenPosition = GetInputPosition();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.WorldToScreenPoint(bodyPart.transform.position).z));
            bodyPart.transform.position = Vector3.Lerp(bodyPart.transform.position, worldPosition + dragOffsets[bodyPart], 0.5f);
        }
    }

    //Multi movement 
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

