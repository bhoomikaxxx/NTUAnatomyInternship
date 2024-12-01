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
    //Dictionary of body parts moved
    private Stack<(GameObject bodyPart, Vector3 position, Quaternion rotation)> movementHistory = new Stack<(GameObject, Vector3, Quaternion)>();
    private bool stopRecording = false; 

    //Recording func
    public void RecordState(GameObject bodyPart, Vector3 position, Quaternion rotation)
    {
        //Stop re recording when undo is clicked to prevent duplicated state
        if (stopRecording) return; 

        if (movementHistory.Count == 0 ||
            movementHistory.Peek().position != position ||
            movementHistory.Peek().rotation != rotation)
        {
            movementHistory.Push((bodyPart, position, rotation));
            Debug.Log($"Recorded: {bodyPart.name} pos: {position}, r: {rotation}");
        }
    }

    //Undo func
    public void UndoLastState()
    {
        //Check for history
        if (movementHistory.Count > 1)
        {
            stopRecording = true;

            //Remove latest record
            movementHistory.Pop();

            //Check for prev record
            var previousState = movementHistory.Peek();

            //Undo to prev pos & r
            previousState.bodyPart.transform.position = previousState.position;
            previousState.bodyPart.transform.rotation = previousState.rotation;

            Debug.Log($"Reversed: {previousState.bodyPart.name} pos: {previousState.position}, r: {previousState.rotation}");

            stopRecording = false; 
        }
    }
}


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

