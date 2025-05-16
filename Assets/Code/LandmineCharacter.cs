using UnityEngine;

public class LandmineTrigger : MonoBehaviour
{
    public TurnBasedMovement turnBasedMovement; // Reference to the TurnBasedMovement script

    void Start()
    {
        // Ensure the TurnBasedMovement script is assigned.  If not, we'll try to find it.
        if (turnBasedMovement == null)
        {
            turnBasedMovement = FindObjectOfType<TurnBasedMovement>();
            if (turnBasedMovement == null)
            {
                Debug.LogError("LandmineTrigger: TurnBasedMovement script not found in the scene.  Please assign it in the inspector.");
                enabled = false; // Disable this script if we can't find the TurnBasedMovement script.
                return;
            }
        }
    }

    // This function is called when another collider enters the trigger.
    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is a player.  You might need to adjust this tag check
        // if your players are tagged differently.  It's best to use tags rather than names.
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Stepped on landmine tile!");

            // Apply damage to the player using the DamagePlayer function from TurnBasedMovement
            // turnBasedMovement.DamagePlayer(other.gameObject, damageAmount); // This line caused the error

            // You might want to destroy the landmine tile after it's triggered.
            //Destroy(gameObject); // Uncomment this line if you want the landmine to disappear.
        }
    }
}
