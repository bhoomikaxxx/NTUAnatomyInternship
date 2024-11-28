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

public class UndoScript : MonoBehaviour
{
    private Stack<(GameObject bodyPart, Vector3 position, Quaternion rotation)> globalMovementHistory = new Stack<(GameObject, Vector3, Quaternion)>();

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

