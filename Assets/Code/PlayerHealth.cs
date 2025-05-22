using UnityEngine;
using System.Collections; // <--- MAKE SURE THIS LINE IS HERE

// This is the one and only class declaration for PlayerHealth
public class PlayerHealth : MonoBehaviour
{
    // ... your existing health variables (maxHealth, currentHealth, etc.) ...
     public int maxHealth = 100;
     public int currentHealth;
     public TurnBasedMovement tbmController; // if you have this
     public PlayerUIController uiController; // if you have this

    // This section is for teleport immunity
    public bool isTeleportingImmune = false;
    public float teleportImmunityDuration = 10f; // Duration of immunity

    public void GrantTeleportImmunity()
    {
        if (!isTeleportingImmune)
        {
            StartCoroutine(TeleportImmunityCoroutine());
        }
    }

    private IEnumerator TeleportImmunityCoroutine()
    {
        isTeleportingImmune = true;
        // Optionally, disable player's collider during immunity if it helps prevent re-triggers
         Collider playerCollider = GetComponent<Collider>();
         if (playerCollider != null) playerCollider.enabled = false;

        yield return new WaitForSeconds(teleportImmunityDuration);

        isTeleportingImmune = false;
         if (playerCollider != null) playerCollider.enabled = true; // Re-enable collider
    }

     private void Start() // if you have a Start method, it should be here
     {
         currentHealth = maxHealth;
         UpdateHealthUI();
     }

     public void TakeDamage(int damage) // if you have this method
     {
         currentHealth -= damage;
         if (currentHealth < 0)
         {
             currentHealth = 0;
             // Handle player death here
         }
       UpdateHealthUI();
     }

     public void UpdateHealthUI() // if you have this method
     {
         float healthRatio = (float)currentHealth / maxHealth;
         // uiController.UpdateHealthBar(currentHealth, maxHealth); // Example UI update
     }

    // ... rest of your PlayerHealth.cs code ...
}