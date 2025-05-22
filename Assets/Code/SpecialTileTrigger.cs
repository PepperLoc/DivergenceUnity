using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialTileType
{
    Landmine,
    Freeze,
    Paintball,
    Teleporter // Keep the enum for other purposes if needed, but the case will be removed
}

public class SpecialTileTrigger : MonoBehaviour
{
    public SpecialTileType tileType;
    public string playerTag = "Player";
    public int damage = 0;
    public int freezeTurns = 0;
    public Color paintballColor = Color.magenta;
    // public bool isTeleporter = false; // No longer needed, as TeleportTile.cs handles teleporters
    // public int teleporterId; // No longer needed, as TeleportTile.cs handles teleporters
    private bool hasTriggered = false; // Prevents multiple triggers on the same tile instantly

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return; // Prevent re-triggering if already triggered
        if (other.gameObject.CompareTag(playerTag)) // Use CompareTag for efficiency
        {
            hasTriggered = true; // Mark as triggered

            TurnBasedMovement tbm = FindObjectOfType<TurnBasedMovement>(); // Get reference once
            if (tbm == null)
            {
                Debug.LogError("TurnBasedMovement script not found in the scene for SpecialTileTrigger!");
                return;
            }

            switch (tileType)
            {
                case SpecialTileType.Landmine:
                    tbm.DamagePlayer(other.gameObject, damage);
                    Destroy(gameObject); // Landmine should be removed after activation
                    break;

                case SpecialTileType.Freeze:
                    tbm.FreezeOpponent(other.gameObject, freezeTurns);
                    Destroy(gameObject); // Freeze tile should be removed after activation
                    break;

                case SpecialTileType.Paintball:
                    tbm.PaintballOpponent(other.gameObject, paintballColor);
                    Destroy(gameObject); // Paintball tile should be removed after activation
                    break;

                case SpecialTileType.Teleporter:
                    // REMOVED: This case is handled by TeleportTile.cs directly on the teleporter prefab.
                    // If this script is attached to a Teleporter tile, it should not be.
                    Debug.LogWarning($"SpecialTileTrigger of type Teleporter was triggered on {gameObject.name}. This tile should be handled by TeleportTile.cs.");
                    // Do NOT destroy the teleporter tile, as it's meant to persist.
                    break;
            }
        }
    }

    // Optional: Reset hasTriggered if you want the tile to be reusable after some time
    // public void ResetTrigger()
    // {
    //     hasTriggered = false;
    // }
}