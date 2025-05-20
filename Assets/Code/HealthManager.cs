using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public PlayerHealth[] players; // Array to hold the PlayerHealth scripts of the three players

    void Start()
    {
        // Initialize the players array (assuming you've assigned the PlayerHealth scripts in the Inspector)
        players = new PlayerHealth[3];
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player"); //Find the players
        Debug.Log("Found " + playerObjects.Length + " GameObjects with 'Player' tag."); // ADDED

        for (int i = 0; i < playerObjects.Length; i++)
        {
            players[i] = playerObjects[i].GetComponent<PlayerHealth>();
            if (players[i] == null)
            {
                Debug.LogError("Player " + (i + 1) + " does not have PlayerHealth script attached."); // ADDED
            }
        }
        if (players.Length != 3)
        {
            Debug.LogError("Ensure there are exactly 3 players with the 'Player' tag and PlayerHealth script attached.");
            return;
        }

        // Initial health log
        Debug.Log("Initial Player Health:");
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log("Player " + (i + 1) + ": " + players[i].currentHealth);
        }
    }

    void Update()
    {
        // Example: Applying damage to players based on number key input.  For testing.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (players[0] != null) //ADDED NULL CHECK
            {
                players[0].TakeDamage(10);
                Debug.Log("Player 1 Health: " + players[0].currentHealth);
            }

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (players[1] != null) //ADDED NULL CHECK
            {
                players[1].TakeDamage(20);
                Debug.Log("Player 2 Health: " + players[1].currentHealth);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (players[2] != null) //ADDED NULL CHECK
            {
                players[2].TakeDamage(5);
                Debug.Log("Player 3 Health: " + players[2].currentHealth);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null) //ADDED NULL CHECK
                {
                    players[i].currentHealth = players[i].maxHealth;
                    players[i].UpdateHealthBar();
                    Debug.Log("Player " + (i + 1) + " Health: " + players[i].currentHealth);
                }
            }
        }
    }
}