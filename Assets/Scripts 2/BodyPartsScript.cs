/*
 * Name: Bhoomika Manot
 * Date: 30 September 2024
 * Description: Script for touching an individual body part and dragging in 3D space 
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

    //Label
    [Header("Label")]
    public TMP_Text labelText;

    //Toggels
    [Header("Toggles")]
    public Toggle singleSelectToggle;
    public Toggle multiSelectToggle;

    //Body toggles
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

    //Colours
    private Color selectedColor, defaultColor;

    public void Awake()
    {
        //Get cam
        if (mainCamera == null) mainCamera = Camera.main;
        normalOrthographicSize = mainCamera.orthographicSize;
        initialCameraPosition = mainCamera.transform.position;

        //historyManager = FindObjectOfType<UndoScript>();

        //Set colours
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
                    ClearSelection();
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
                    ClearSelection();
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
        //Reset if toggle is off
        if (!singleSelectToggle.isOn && !multiSelectToggle.isOn)
        {
            ClearSelection();
            labelText.text = "No parts selected";
            return;
        }

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 screenPosition = GetInputPosition();
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                //Check for tag
                if (hit.collider.CompareTag("Movable"))
                {
                    //Check for time btwn touches - for deselection
                    GameObject bodyPart = hit.collider.gameObject;
                    float currentTime = Time.time;

                    //Check for multi select on & contains body parts
                    if (isMultiSelect && selectedBodyParts.Contains(bodyPart))
                    {
                        //Deslection logic
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
                        //Multi select logic
                        if (isMultiSelect)
                        {
                            selectedBodyParts.Add(bodyPart);
                            dragOffsets[bodyPart] = Vector3.zero;
                            ChangeColor(bodyPart, selectedColor);
                            labelText.text = $"Selected {selectedBodyParts.Count} Parts";
                        }

                        //Single select logic
                        else if (!isMultiSelect && singleSelectToggle.isOn)
                        {
                            ClearSelection();
                            selectedBodyParts.Add(bodyPart);
                            dragOffsets[bodyPart] = Vector3.zero;
                            ChangeColor(bodyPart, selectedColor);
                            labelText.text = "Selected Part: " + bodyPart.name;
                        }
                    }
                    //Reset time
                    lastClickTime = currentTime;
                }
            }
        }

        //Prevents dragging
        if (selectedBodyParts.Count == 0) return;  

        //Dragging logic
        if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
        {
            if (!isDragging && selectedBodyParts.Count > 0)
            {
                if (isMultiSelect || selectedBodyParts.Count == 1)
                {
                    Drag();
                }
            }

            if (isDragging)
            {
                MoveBodyParts(selectedBodyParts);
            }
        }

        //Dragging ended logic
        if (isDragging && (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)))
        {
            StopDrag();
        }
    }

    //Reset logic
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

    //Deselection outline func
    public void ResetColour(GameObject bodyPart)
    {
        Renderer renderer = bodyPart.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = ColorUtility.TryParseHtmlString("#B2AFAF", out Color defaultColor) ? defaultColor : Color.gray; 
        }
    }

    public void ClearColours()
    {
        //Single select logic
        if (!isMultiSelect && selectedBodyParts.Count > 0)
        {
            ResetColour(selectedBodyParts[0]);
        }
        //Multi select logic
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
        /*foreach (GameObject bodyPart in selectedBodyParts)
        {
            historyManager.RecordState(bodyPart, bodyPart.transform.position, bodyPart.transform.rotation);
        }*/

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
                List<GameObject> toIsolate = entry.Value.Except(selectedBodyParts).ToList();

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

