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
using System.Linq;

public class BodyPartsScript : MonoBehaviour
{
    //Declarations
    [Header("Camera")]
    public Camera mainCamera;

    //Iso zoom
    private float zoomDuration = 0.5f;
    private float targetOrthographicSize = 0.3f; 
    private float normalOrthographicSize;
    private bool isZooming = false;
    private Vector3 initialCameraPosition;

    //Lists
    [Header("Models")]
    public List<GameObject> bodyParts = new List<GameObject>();
    public List<GameObject> skeletonBodyParts = new List<GameObject>();
    public List<GameObject> jointsBodyParts = new List<GameObject>();
    public List<GameObject> lymphoidOrgansBodyParts = new List<GameObject>();
    public List<GameObject> nervousSystemBodyParts = new List<GameObject>();
    public List<GameObject> visceralSystemBodyParts = new List<GameObject>();
    public List<GameObject> cardiovascularBodyParts = new List<GameObject>();
    public List<GameObject> muscularSystemBodyParts = new List<GameObject>();
    public List<GameObject> humanBodyParts = new List<GameObject>();
    //private List<GameObject> previouslyActiveBodyParts = new List<GameObject>();

    //Models
    public GameObject skeletonModel, jointsModel, loModel, nsModel, vsModel, cardioModel, msModel, humanModel;

    //Drag
    public GameObject selectedBodyPart = null;
    public bool isDragging = false;
    public bool isTouchingMovable = false;

    [Header("Text")]
    public TMP_Text labelText;

    [Header("Toggles")]
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;

    //Skeleton toggle
    public Toggle skeleton, joints, lymphoidOrgans, nervousSystem, visceralSystem, cardiovascular, muscularSystem, human;

    //Multi select
    [Header("Multi Select")]
    public bool isMultiSelect = false;
    public List<GameObject> selectedBodyParts = new List<GameObject>();
    public Dictionary<GameObject, Vector3> dragOffsets = new Dictionary<GameObject, Vector3>();

    //Undo script ref
    private UndoScript historyManager;

    //Double click deselect for multi select
    private float lastClickTime = 0f;

    // Cached colors
    private Color selectedColor, defaultColor;

    public void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        normalOrthographicSize = mainCamera.orthographicSize;
        initialCameraPosition = mainCamera.transform.position;

        historyManager = FindObjectOfType<UndoScript>();

        // Set colors
        ColorUtility.TryParseHtmlString("#EFEFEF", out selectedColor);
        ColorUtility.TryParseHtmlString("#B2AFAF", out defaultColor);

        //Add single select toggle
        if (singleSelectToggle != null)
        {
            singleSelectToggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    isMultiSelect = false;
                    multiSelectToggle.isOn = false;
                    ClearColours();
                }
            });
        }

        //Add multi select toggle
        if (multiSelectToggle != null)
        {
            multiSelectToggle.onValueChanged.AddListener((value) =>
            {
                if (value)
                {
                    isMultiSelect = true;
                    singleSelectToggle.isOn = false;
                    ClearColours();
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
        // Check for left mouse click or touch start
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            // Get position of the input (mouse or touch)
            Vector3 screenPosition = GetInputPosition();
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Movable"))
                {
                    GameObject bodyPart = hit.collider.gameObject;

                    // Check for time between clicks
                    float currentTime = Time.time;

                    if (isMultiSelect && selectedBodyParts.Contains(bodyPart))
                    {
                        // Check for double-click (mouse) or double-tap (touch)
                        if (currentTime - lastClickTime < 0.5f)
                        {
                            selectedBodyParts.Remove(bodyPart);
                            dragOffsets.Remove(bodyPart);
                            ResetColour(bodyPart);
                            labelText.text = $"Selected {selectedBodyParts.Count} Parts";
                        }
                    }
                    else
                    {
                        // Multi select add
                        if (isMultiSelect)
                        {
                            selectedBodyParts.Add(bodyPart);
                            dragOffsets[bodyPart] = Vector3.zero;
                            ChangeColor(bodyPart, selectedColor);
                            labelText.text = $"Selected {selectedBodyParts.Count} Parts";
                        }
                        // Single select add
                        else if (!isMultiSelect && singleSelectToggle.isOn)
                        {
                            ClearSelection();
                            selectedBodyParts.Add(bodyPart);
                            dragOffsets[bodyPart] = Vector3.zero;
                            ChangeColor(bodyPart, selectedColor);
                            labelText.text = "Selected Part: " + bodyPart.name;
                        }
                    
                    }
                    lastClickTime = currentTime;
                }
            }
        }

        // Dragging - left mouse/touch
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

        if (isDragging)
        {
            if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
            {
                StopDrag();
            }
        }
    }

    private void ClearSelection()
    {
        if (selectedBodyParts.Count > 0)
            ResetColour(selectedBodyParts[0]);

        selectedBodyParts.Clear();
        dragOffsets.Clear();
    }

    //Outline func
    private void ChangeColor(GameObject bodyPart, Color color)
    {
        Renderer renderer = bodyPart.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    public void ResetColour(GameObject bodyPart)
    {
        Renderer renderer = bodyPart.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = ColorUtility.TryParseHtmlString("#B2AFAF", out Color defaultColor) ? defaultColor : Color.gray; // Default gray color
        }
    }

    public void ClearColours()
    {
        // In single select mode, clear only the color of the currently selected body part
        if (!isMultiSelect && selectedBodyParts.Count > 0)
        {
            ResetColour(selectedBodyParts[0]);
        }
        // In multi-select mode, clear all selected parts
        else if (isMultiSelect)
        {
            foreach (GameObject bodyPart in selectedBodyParts)
            {
                ResetColour(bodyPart);
            }
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

        //Record the final pos/rotation for undo - not working
        foreach (GameObject bodyPart in selectedBodyParts)
        {
            historyManager.RecordState(bodyPart, bodyPart.transform.position, bodyPart.transform.rotation);
        }

        selectedBodyPart = null;
        //selectedBodyParts.Clear();
        dragOffsets.Clear();
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
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y,
            mainCamera.WorldToScreenPoint(bodyParts[0].transform.position).z));

        foreach (GameObject bodyPart in bodyParts)
        {
            if (dragOffsets.TryGetValue(bodyPart, out Vector3 offset))
            {
                //Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.WorldToScreenPoint(bodyPart.transform.position).z));
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

    //Isolate Function
    public void Isolate()
    {
        if (selectedBodyParts.Count == 0) return;

        var isolationMap = new Dictionary<Toggle, List<GameObject>>()
        {
            { skeleton, skeletonBodyParts },
            { joints, jointsBodyParts },
            { lymphoidOrgans, lymphoidOrgansBodyParts },
            { nervousSystem, nervousSystemBodyParts },
            { visceralSystem, visceralSystemBodyParts },
            { cardiovascular, cardiovascularBodyParts }
        };

        foreach (var entry in isolationMap)
        {
            if (entry.Key.isOn)
            {
                // Get the list and exclude selected parts
                List<GameObject> toIsolate = entry.Value.Except(selectedBodyParts).ToList();

                // Deactivate unselected parts
                foreach (GameObject obj in toIsolate)
                {
                    obj.SetActive(false);
                }
            }
        }
        
        //Target zoom pos
        Vector3 targetPosition = isMultiSelect ? CalculateCenterPoint(selectedBodyParts) : selectedBodyParts[0].transform.position;
        StartCoroutine(ZoomToPosition(targetPosition, targetOrthographicSize));
    }

    public void Deisolate()
    {
        var isolationMap = new Dictionary<Toggle, List<GameObject>>()
        {
            { skeleton, skeletonBodyParts },
            { joints, jointsBodyParts },
            { lymphoidOrgans, lymphoidOrgansBodyParts },
            { nervousSystem, nervousSystemBodyParts },
            { visceralSystem, visceralSystemBodyParts },
            { cardiovascular, cardiovascularBodyParts }
        };

        foreach (var entry in isolationMap)
        {
            if (entry.Key.isOn)
            {
                foreach (GameObject obj in entry.Value)
                {
                    obj.SetActive(true);
                }
            }
        }

        //Original camera pos/size
        StartCoroutine(ZoomToPosition(initialCameraPosition, normalOrthographicSize));
    }

    private IEnumerator ZoomToPosition(Vector3 targetPosition, float targetSize)
    {
        isZooming = true;
        float elapsedTime = 0f;

        Vector3 initialPosition = mainCamera.transform.position;
        float initialSize = mainCamera.orthographicSize;

        Vector3 adjustedTargetPosition = new Vector3(targetPosition.x, targetPosition.y, mainCamera.transform.position.z);

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;

            mainCamera.transform.position = Vector3.Lerp(initialPosition, adjustedTargetPosition, elapsedTime / zoomDuration);
            mainCamera.orthographicSize = Mathf.Lerp(initialSize, targetSize, elapsedTime / zoomDuration);

            yield return null;
        }

        mainCamera.transform.position = adjustedTargetPosition;
        mainCamera.orthographicSize = targetSize;
        isZooming = false;
    }

    //Center zoom
    private Vector3 CalculateCenterPoint(List<GameObject> selectedObjects)
    {
        Vector3 centerPoint = Vector3.zero;
        foreach (GameObject obj in selectedObjects)
        {
            centerPoint += obj.transform.position;
        }
        centerPoint /= selectedObjects.Count;
        return centerPoint;
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

