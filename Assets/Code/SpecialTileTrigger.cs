using UnityEngine;
using System.Collections.Generic;

public class SpecialTileTrigger : MonoBehaviour
{
    public SpecialTileType tileType;
    public string playerTag = "Player";
    public int damage = 0;
    public int freezeTurns = 0;
    public Color paintballColor = Color.white;
    public bool isTeleporter = false;
    public bool canTeleportToOccupied = false;  //redundant but keep for now
    private bool hasTriggered = false;
    public int teleporterId = -1; //used to identify pairs.

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag(playerTag))
        {
            hasTriggered = true;
            TurnBasedMovement turnManager = FindObjectOfType<TurnBasedMovement>(); //find
            if (turnManager == null)
            {
                Debug.LogError("TurnBasedMovement not found");
                return;
            }

            switch (tileType)
            {
                case SpecialTileType.Landmine:
                    turnManager.DamagePlayer(other.gameObject, damage);
                    Destroy(gameObject.transform.GetChild(0).gameObject); //destroys the child object of the tile
                    Destroy(this);
                    break;
                case SpecialTileType.Freeze:
                    turnManager.FreezeOpponent(other.gameObject, freezeTurns);
                    break;
                case SpecialTileType.Paintball:
                    turnManager.PaintballOpponent(other.gameObject, paintballColor);
                    break;
                case SpecialTileType.Teleporter:
                    turnManager.TeleportPlayer(other.gameObject, teleporterId);
                    break;
                case SpecialTileType.None:
                    turnManager.SkipTurn(other.gameObject);
                    break;
            }
        }
    }
}

public enum SpecialTileType
{
    None,
    Landmine,
    Freeze,
    Paintball,
    Teleporter
}