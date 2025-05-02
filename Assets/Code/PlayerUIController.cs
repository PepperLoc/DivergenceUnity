using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public Text playerNameText;
    public Text healthText;
    public Image healthBar;
    private GameObject player;

    public void SetPlayer(GameObject player)
    {
        this.player = player;
        playerNameText.text = player.name;
    }

    public void SetHealthBar(Image healthBar)
    {
        this.healthBar = healthBar;
    }

    public void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }

    public void SetPlayerName(string name)
    {
        playerNameText.text = name;
    }
}

