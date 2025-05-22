using UnityEngine;
using System.Collections;

public class TeleportTile : MonoBehaviour
{
    public Transform destinationTeleportTile;
    public string playerTag = "Player";
    public float teleportCooldown = 0.7f; // Increased cooldown slightly for more stability
    private bool isCoolingDown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCoolingDown) return; // Ignore if this tile is on cooldown

        if (other.CompareTag(playerTag))
        {
            if (destinationTeleportTile != null)
            {
                TurnBasedMovement turnBasedMovement = FindObjectOfType<TurnBasedMovement>();
                if (turnBasedMovement != null)
                {
                    // Request the TurnBasedMovement script to handle the teleport
                    // This allows TBM to manage player state (like disabling input)
                    turnBasedMovement.HandleTeleport(other.gameObject, destinationTeleportTile.position + Vector3.up * 0.5f);
                }
                else
                {
                    // Fallback if TurnBasedMovement isn't found (though it should be)
                    other.transform.position = destinationTeleportTile.position + Vector3.up * 0.5f;
                    Debug.Log($"{other.name} teleported from {transform.position} to {destinationTeleportTile.position}");
                }

                StartCoroutine(StartCooldown()); // Start cooldown for *this* tile
            }
            else
            {
                Debug.LogWarning($"Teleporter tile at {transform.position.x},{transform.position.z} has no destination assigned!");
            }
        }
    }

    private IEnumerator StartCooldown()
    {
        isCoolingDown = true;
        // Optionally, you could disable the collider of this specific teleporter tile for a moment
        // Collider teleporterCollider = GetComponent<Collider>();
        // if(teleporterCollider != null) teleporterCollider.enabled = false;

        yield return new WaitForSeconds(teleportCooldown);

        isCoolingDown = false;
        // if(teleporterCollider != null) teleporterCollider.enabled = true; // Re-enable collider
    }
}