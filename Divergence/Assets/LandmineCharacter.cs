using UnityEngine;

public class LandmineTrigger : MonoBehaviour
{
    public int damage = 15;
    public string playerTag = "Player";
    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasExploded && other.CompareTag(playerTag))
        {
            hasExploded = true;
            Debug.Log("Player stepped on a landmine! -" + damage + " health.");
            // In a real game, you would access the player's health component here
            // and decrease their health by the 'damage' amount.
            // For example:
            // PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damage);
            // }

            // Optionally, you can trigger a visual effect or destroy the landmine
            // Destroy(gameObject.GetComponentInChildren<Transform>().gameObject); // Destroy the landmine visual
            // Destroy(this); // Destroy this trigger component
        }
    }
}