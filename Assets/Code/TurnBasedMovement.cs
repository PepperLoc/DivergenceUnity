using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.UI; // For UI elements like health bars

public class TurnBasedMovement : MonoBehaviour // Class name MUST match file name
{
    public int boardWidth = 5;
    public int boardHeight = 15;
    public float tileSpacing = 1.0f;
    public List<GameObject> players;
    public int currentPlayerIndex = 0;
    public int movementRangeWidth = 3;
    public int movementRangeHeight = 5;
    public GameObject gameOverPanel;
    public Text gameOverText;
    public GameObject playerUIPrefab;
    public Transform uiParent;
    private List<PlayerHealth> playerHealths = new List<PlayerHealth>();
    private GameObject[,] boardTiles;
    private int frozenTurnsRemaining = 0;
    private List<int> frozenPlayers = new List<int>();
    private Dictionary<int, Vector2Int> teleporterPositions = new Dictionary<int, Vector2Int>();
    private int teleporterCounter = 0;

    void Start()
    {
        if (players.Count == 0)
        {
            Debug.LogError("No players assigned to the TurnBasedMovement script!");
            enabled = false;
            return;
        }

        BoardGenerator boardGenerator = FindObjectOfType<BoardGenerator>();
        if (boardGenerator == null)
        {
            Debug.LogError("BoardGenerator not found in the scene!");
            enabled = false;
            return;
        }
        boardWidth = boardGenerator.width;
        boardHeight = boardGenerator.height;
        tileSpacing = boardGenerator.tileSpacing;
        boardTiles = new GameObject[boardWidth, boardHeight];
        PopulateBoardTilesArray();
        InitializePlayers();
        SetInitialPlayerPositions();
        currentPlayerIndex = Random.Range(0, players.Count);
        UpdateCurrentTilePosition();
        HighlightAvailableMoves();
        gameOverPanel.SetActive(false);
    }

    void PopulateBoardTilesArray()
    {
        GameObject boardParent = GameObject.Find("BoardManager");
        if (boardParent != null)
        {
            for (int i = 0; i < boardParent.transform.childCount; i++)
            {
                GameObject tile = boardParent.transform.GetChild(i).gameObject;
                string[] parts = tile.name.Split('_');
                if (parts.Length == 3 && parts[0] == "Tile" && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int z))
                {
                    if (x >= 0 && x < boardWidth && z >= 0 && z < boardHeight)
                    {
                        boardTiles[x, z] = tile;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Could not find the parent GameObject of the board tiles (assuming 'BoardManager').");
            enabled = false;
        }
    }

    void InitializePlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerHealth playerHealth = players[i].AddComponent<PlayerHealth>();
            playerHealth.maxHealth = 100;
            playerHealth.currentHealth = 100;
            playerHealths.Add(playerHealth);

            if (playerUIPrefab != null && uiParent != null)
            {
                GameObject playerUI = Instantiate(playerUIPrefab, uiParent);
                PlayerUIController uiController = playerUI.GetComponent<PlayerUIController>();
                if (uiController != null)
                {
                    uiController.SetPlayer(players[i]);
                    uiController.SetHealthBar(playerHealth.healthBar);
                    playerHealth.uiController = uiController;
                }
                else
                {
                    Debug.LogError("PlayerUIController component not found on playerUIPrefab!");
                }
            }
            else
            {
                Debug.LogError("playerUIPrefab or uiParent is not assigned!");
            }
        }
    }

    void SetInitialPlayerPositions()
    {
        if (players.Count >= 1 && boardTiles[0, 0] != null)
        {
            players[0].transform.position = boardTiles[0, 0].transform.position + Vector3.up * 0.5f;
        }
        if (players.Count >= 2 && boardTiles[1, 0] != null)
        {
            players[1].transform.position = boardTiles[1, 0].transform.position + Vector3.up * 0.5f;
        }
        if (players.Count >= 3 && boardTiles[2, 0] != null)
        {
            players[2].transform.position = boardTiles[2, 0].transform.position + Vector3.up * 0.5f;
        }
        UpdateCurrentTilePosition();
    }

    void UpdateCurrentTilePosition()
    {
        Vector3 playerPos = players[currentPlayerIndex].transform.position;
        int x = Mathf.RoundToInt((playerPos.x - transform.position.x) / tileSpacing);
        int z = Mathf.RoundToInt((playerPos.z - transform.position.z) / tileSpacing);
        currentTilePosition = new Vector3Int(Mathf.Clamp(x, 0, boardWidth - 1), 0, Mathf.Clamp(z, 0, boardHeight - 1));
        Debug.Log($"Current Player {currentPlayerIndex + 1} is at tile: {currentTilePosition.x}, {currentTilePosition.z}");
    }

    public void AttemptMove(GameObject targetTile)
    {
        if (IsCurrentPlayerFrozen())
        {
            Debug.Log($"Player {currentPlayerIndex + 1} is frozen and cannot move!");
            EndTurn();
            return;
        }

        if (targetTile == null) return;

        string[] parts = targetTile.name.Split('_');
        if (parts.Length == 3 && parts[0] == "Tile" && int.TryParse(parts[1], out int targetX) && int.TryParse(parts[2], out int targetZ))
        {
            int deltaX = Mathf.Abs(targetX - currentTilePosition.x);
            int deltaZ = Mathf.Abs(targetZ - currentTilePosition.z);

            if (deltaX <= movementRangeWidth / 2 && deltaZ <= movementRangeHeight / 2 && deltaX + deltaZ > 0)
            {
                players[currentPlayerIndex].transform.position = targetTile.transform.position + Vector3.up * 0.5f;
                UpdateCurrentTilePosition();
                EndTurn();
            }
            else
            {
                Debug.Log("Target tile is out of movement range.");
            }
        }
    }

    void EndTurn()
    {
        if (frozenPlayers.Contains(currentPlayerIndex))
        {
            frozenTurnsRemaining--;
            if (frozenTurnsRemaining <= 0)
            {
                frozenPlayers.Remove(currentPlayerIndex);
                Debug.Log($"Player {currentPlayerIndex + 1} is no longer frozen.");
            }
        }

        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }
        if (frozenPlayers.Contains(currentPlayerIndex))
        {
            Debug.Log($"Player {currentPlayerIndex + 1} is frozen!");
            EndTurn();
            return;
        }
        Debug.Log($"It's now Player {currentPlayerIndex + 1}'s turn.");
        HighlightAvailableMoves();
    }

    void HighlightAvailableMoves()
    {
        RemoveHighlight();

        if (IsCurrentPlayerFrozen())
        {
            Debug.Log($"Player {currentPlayerIndex + 1} is frozen, no moves to highlight.");
            return;
        }

        if (boardTiles == null || !boardTiles.Cast<GameObject>().Any(tile => tile != null)) return;

        for (int x = 0; x < boardWidth; x++)
        {
            for (int z = 0; z < boardHeight; z++)
            {
                int deltaX = Mathf.Abs(x - currentTilePosition.x);
                int deltaZ = Mathf.Abs(z - currentTilePosition.z);

                if (deltaX <= movementRangeWidth / 2 && deltaZ <= movementRangeHeight / 2 && deltaX + deltaZ > 0 && boardTiles[x, z] != null)
                {
                    Renderer tileRenderer = boardTiles[x, z].GetComponent<Renderer>();
                    if (tileRenderer != null)
                    {
                        Material highlightMaterial = new Material(Shader.Find("Standard"));
                        highlightMaterial.color = Color.yellow;
                        tileRenderer.material = highlightMaterial;
                        ClickableTile clickable = boardTiles[x, z].GetComponent<ClickableTile>();
                        if (clickable == null)
                        {
                            clickable = boardTiles[x, z].AddComponent<ClickableTile>();
                        }
                        clickable.turnBasedMovement = this;
                    }
                }
            }
        }
    }

    void RemoveHighlight()
    {
        if (boardTiles == null || !boardTiles.Cast<GameObject>().Any(tile => tile != null)) return;

        for (int x = 0; x < boardWidth; x++)
        {
            for (int z = 0; z < boardHeight; z++)
            {
                if (boardTiles[x, z] != null)
                {
                    Renderer tileRenderer = boardTiles[x, z].GetComponent<Renderer>();
                    if (tileRenderer != null)
                    {
                        Material defaultMaterial = new Material(Shader.Find("Standard"));
                        defaultMaterial.color = Color.white;
                        tileRenderer.material = defaultMaterial;
                        ClickableTile clickable = boardTiles[x, z].GetComponent<ClickableTile>();
                        if (clickable != null)
                        {
                            Destroy(clickable);
                        }
                    }
                }
            }
        }
    }

    public void DamagePlayer(GameObject player, int damage)
    {
        PlayerHealth targetHealth = player.GetComponent<PlayerHealth>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
            if (targetHealth.currentHealth <= 0)
            {
                HandlePlayerDeath(player);
            }
        }
    }

    public void FreezeOpponent(GameObject player, int turns)
    {
        int opponentIndex = players.IndexOf(player);
        if (opponentIndex != -1)
        {
            if (!frozenPlayers.Contains(opponentIndex))
            {
                frozenPlayers.Add(opponentIndex);
            }
            frozenTurnsRemaining = turns;
            Debug.Log($"Player {opponentIndex + 1} frozen for {turns} turns");
        }
    }

    public void PaintballOpponent(GameObject player, Color color)
    {
        int opponentIndex = players.IndexOf(player);
        if (opponentIndex != -1)
        {
            DamagePlayer(player, 30);
            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.material.color = color;
            }
            Debug.Log($"Player {opponentIndex + 1} hit with paintball!");
        }
    }

    public void TeleportPlayer(GameObject player, int teleporterId)
    {
        if (teleporterPositions.ContainsKey(teleporterId))
        {
            Vector2Int targetCoords = teleporterPositions[teleporterId];
            player.transform.position = GetTilePosition(targetCoords.x, targetCoords.y) + Vector3.up * 0.5f;
            UpdateCurrentTilePosition();
            Debug.Log($"Player teleported to {targetCoords.x}, {targetCoords.y}");
        }
        else
        {
            Debug.Log("Invalid teleporter ID");
        }
    }

    public void SkipTurn(GameObject player)
    {
        Debug.Log($"Player's turn skipped!");
        EndTurn();
    }

    void HandlePlayerDeath(GameObject player)
    {
        Debug.Log($"{player.name} has been defeated!");
        int playerIndex = players.IndexOf(player);
        if (playerIndex != -1)
        {
            players.RemoveAt(playerIndex);
        }

        Destroy(player);

        if (players.Count <= 1)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
        if (players.Count == 1)
        {
            gameOverText.text = $"{players[0].name} Wins!";
        }
        else
        {
            gameOverText.text = "Game Over!";
        }
    }

    public void RestartGame()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    bool IsTileOccupied(GameObject tile)
    {
        foreach (var player in players)
        {
            Vector3 playerPos = player.transform.position;
            Vector3 tilePos = tile.transform.position;
            if (Mathf.Abs(playerPos.x - tilePos.x) < tileSpacing / 2f &&
                Mathf.Abs(playerPos.z - tilePos.z) < tileSpacing / 2f)
            {
                return true;
            }
        }
        return false;
    }
}