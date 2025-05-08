using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public TurnBasedMovement turnBasedMovement;

    void OnMouseDown()
    {
        if (turnBasedMovement != null)
        {
            turnBasedMovement.AttemptMove(gameObject);
        }
        else
        {
            Debug.LogError("TurnBasedMovement reference is not set on ClickableTile!");
        }
    }
}

