/*
 * Name: Bhoomika Manot
 * Date: 12 November 2024
 * Description: Script to undo body part movement/model rotation.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class UndoScript : MonoBehaviour
{
    //[Header("Text")]
    public TMP_Text labelText;

    //List of history movements of the model and parts
    private Stack<(GameObject bodyPart, Vector3 position, Quaternion rotation)> globalMovementHistory = new Stack<(GameObject, Vector3, Quaternion)>();

    //Recording values if changed
    public void RecordState(GameObject bodyPart, Vector3 position, Quaternion rotation)
    {
        //Get initial values
        if (globalMovementHistory.Count == 0 ||
            globalMovementHistory.Peek().position != position ||
            globalMovementHistory.Peek().rotation != rotation)
        {
            //Add additional values
            globalMovementHistory.Push((bodyPart, position, rotation));
            Debug.Log($"[RECORD] Recorded state: {bodyPart.name} at position: {position}, rotation: {rotation}");

            Debug.Log(globalMovementHistory.Count);
        }
    }

    public void UndoLastState()
    {
        //Check for recorded data
        if (globalMovementHistory.Count > 0)
        {
            //Remove last data and return to prev data
            var lastState = globalMovementHistory.Peek();
            lastState.bodyPart.transform.position = lastState.position;
            lastState.bodyPart.transform.rotation = lastState.rotation;
            globalMovementHistory.Pop();
            Debug.Log($"[UNDO] Reverted to state: {lastState.bodyPart.name} at position: {lastState.position}, rotation: {lastState.rotation}");

        }
        else
        {
            //Label
            labelText.text = "Cannot undo";
            return;
        }
    }
}




//Declarations
/*//[Header("Text")]
public TMP_Text labelText;

//List of history movements of the model and parts
private Stack<(GameObject bodyPart, Vector3 position, Quaternion rotation)> globalMovementHistory = new Stack<(GameObject, Vector3, Quaternion)>();

//Recording values if changed
public void RecordState(GameObject bodyPart, Vector3 position, Quaternion rotation)
{
    //Get initial values
    if (globalMovementHistory.Count == 0 ||
        globalMovementHistory.Peek().position != position ||
        globalMovementHistory.Peek().rotation != rotation)
    {
        //Add additional values
        globalMovementHistory.Push((bodyPart, position, rotation));
        Debug.Log($"[RECORD] Recorded state: {bodyPart.name} at position: {position}, rotation: {rotation}");
    }
}

public void UndoLastState()
{
    //Check for recorded data
    if (globalMovementHistory.Count > 0)
    {
        //Remove last data and return to prev data
        globalMovementHistory.Pop();
        var lastState = globalMovementHistory.Peek();
        lastState.bodyPart.transform.position = lastState.position;
        lastState.bodyPart.transform.rotation = lastState.rotation;
        Debug.Log($"[UNDO] Reverted to state: {lastState.bodyPart.name} at position: {lastState.position}, rotation: {lastState.rotation}");

    }
    else
    {
        //Label
        labelText.text = "Cannot undo";
    }
}
}*/


/*private Stack<(GameObject bodyPart, Vector3 position, Quaternion rotation)> globalMovementHistory = new Stack<(GameObject, Vector3, Quaternion)>();

public void RecordState(GameObject bodyPart, Vector3 position, Quaternion rotation)
{
    if (globalMovementHistory.Count == 0 ||
        globalMovementHistory.Peek().position != position ||
        globalMovementHistory.Peek().rotation != rotation)
    {
        globalMovementHistory.Push((bodyPart, position, rotation));
        Debug.Log($"[RECORD] Recorded state: {bodyPart.name} at position: {position}, rotation: {rotation}");
    }
}

public void UndoLastState()
{
    if (globalMovementHistory.Count > 1) 
    {
        var lastState = globalMovementHistory.Pop();
        var previousState = globalMovementHistory.Peek();

        previousState.bodyPart.transform.position = previousState.position;
        previousState.bodyPart.transform.rotation = previousState.rotation;
    }
}*/

