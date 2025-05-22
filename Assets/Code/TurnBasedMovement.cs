using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedMovement : MonoBehaviour
{
    // --- Public Variables (Assign in Inspector) ---
    public List<GameObject> players; // List of player GameObjects
    public int boardWidth = 10;     // Width of the board in tiles
    public int boardHeight = 10;    // Height of the board in tiles
    public float tileSpacing = 1f;  // Distance between tile centers
    public GameObject currentActivePlayerHighlight; // Visual highlight for the current player
    public Material highlightMaterial; // Material for highlighting movable tiles
    public int movesPerTurn = 3;    // How many moves each player gets per turn
    public float movementSpeed = 5f; // Speed at which player moves between tiles
    public float teleportImmunityDuration = 0.7f; // Duration player cannot move/teleport again after teleport

    // --- Private Variables (Internal State) ---
    private int currentPlayerIndex;
    private Vector3Int currentTilePosition; // Position of the current player's tile (x, 0, z)
    private List<GameObject> boardTiles = new List<GameObject>(); // Stores all generated board tiles
    private List<GameObject> highlightedTiles = new List<GameObject>(); // Stores currently highlighted tiles
    private bool isMoving = false; // Flag to prevent input during player movement
    private int remainingMoves;    // Moves left for the current player's turn
    private GameObject playerBeingMoved;
    private bool isPlayerInputDisabled = false; // Flag to disable input after teleport or during other actions

    // --- Unity Lifecycle Methods ---

    void Start()
    {
        PopulateBoardTilesArray();
        InitializePlayers();
        SetInitialPlayerPositions();
        currentPlayerIndex = Random.Range(0, players.Count); // Start with a random player
        UpdateCurrentTilePosition(); // Set initial currentTilePosition for the starting player
        remainingMoves = movesPerTurn;
        HighlightAvailableMoves();
    }

    void Update()
    {
        // Prevent input if player is already moving or input is temporarily disabled (e.g., after teleport)
        if (isMoving || isPlayerInputDisabled) return;

        // Player movement input handling (mouse click)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Ensure the ray hits a board tile (assuming tiles are on a "BoardTile" layer)
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("BoardTile")))
            {
                GameObject targetTile = hit.collider.gameObject;
                AttemptMove(targetTile);
            }
        }
    }

    // --- Core Game Logic Methods ---

    // Handles the direct teleportation of a player and manages related states
    public void HandleTeleport(GameObject player, Vector3 destinationPosition)
    {
        // Set the player's position directly
        player.transform.position = destinationPosition;
        Debug.Log($"{player.name} instantly teleported to {destinationPosition}");

        // Update the internal tile position of the player who just teleported
        UpdateCurrentTilePosition(player); // Use the overloaded method for specific player

        // Start a coroutine to temporarily disable player input
        StartCoroutine(DisablePlayerInputTemporarily(teleportImmunityDuration));
    }

    // Overloaded method to update a specific player's tile position
    public void UpdateCurrentTilePosition(GameObject playerToUpdate)
    {
        Vector3 playerPos = playerToUpdate.transform.position;
        // Calculate the tile coordinates based on player's world position
        int x = Mathf.RoundToInt((playerPos.x - transform.position.x) / tileSpacing);
        int z = Mathf.RoundToInt((playerPos.z - transform.position.z) / tileSpacing);

        // Clamp coordinates to ensure they are within board boundaries
        x = Mathf.Clamp(x, 0, boardWidth - 1);
        z = Mathf.Clamp(z, 0, boardHeight - 1);

        // Update the current active player's internal tile position
        if (playerToUpdate == players[currentPlayerIndex])
        {
            currentTilePosition = new Vector3Int(x, 0, z);
            Debug.Log($"Current Player {currentPlayerIndex + 1} is now internally registered at tile: {currentTilePosition.x}, {currentTilePosition.z} after teleport/move.");
        }
        else
        {
            // This case might be for other players (e.g., if another player is moved by an ability)
            // You might need a more complex system (e.g., a dictionary mapping player to Vector3Int)
            // if you need to track precise tile positions for *all* players simultaneously.
            Debug.Log($"Info: Player {playerToUpdate.name} is now at {x},{z}. (Not the active player's currentTilePosition).");
        }
    }

    // Original method to update the current active player's tile position
    public void UpdateCurrentTilePosition()
    {
        UpdateCurrentTilePosition(players[currentPlayerIndex]);
    }

    // Coroutine to temporarily disable player input
    private IEnumerator DisablePlayerInputTemporarily(float duration)
    {
        isPlayerInputDisabled = true;
        Debug.Log("Player input disabled for " + duration + " seconds after teleport/action.");
        yield return new WaitForSeconds(duration);
        isPlayerInputDisabled = false;
        Debug.Log("Player input re-enabled.");

        // After immunity, if it's still the player's turn and they have moves left, re-highlight
        if (remainingMoves > 0 && !isMoving)
        {
            HighlightAvailableMoves();
        }
    }

    // Attempts to move the current player to the clicked target tile
    public void AttemptMove(GameObject targetTile)
    {
        // Clear previous highlights as a new move attempt is made
        ClearHighlightedTiles();

        // Parse target tile's coordinates from its name
        // Example tile names: "Tile_(X,Z)", "TeleportTile_Pair_1_A_(X,Z)"
        int targetX = 0; // Initialize to prevent 'unassigned local variable' error
        int targetZ = 0; // Initialize to prevent 'unassigned local variable' error
        bool coordsParsed = false;

        string tileName = targetTile.name;
        string[] parts = tileName.Split('_');

        // Handles names like "Tile_(X,Z)"
        if (parts.Length > 0 && parts[parts.Length - 1].Contains("(") && parts[parts.Length - 1].Contains(")"))
        {
            string coordsString = parts[parts.Length - 1].Trim('(', ')');
            string[] coordParts = coordsString.Split(',');
            if (coordParts.Length == 2 && int.TryParse(coordParts[0], out targetX) && int.TryParse(coordParts[1], out targetZ))
            {
                coordsParsed = true;
            }
        }
        // Fallback for names like "TeleportTile_Pair_1_A_(X,Z)"
        else if (parts.Length >= 1 && parts[parts.Length - 1].Contains("(") && parts[parts.Length - 1].Contains(")"))
        {
            string coordsString = parts[parts.Length - 1].Trim('(', ')');
            string[] coordParts = coordsString.Split(',');
            if (coordParts.Length == 2 && int.TryParse(coordParts[0], out targetX) && int.TryParse(coordParts[1], out targetZ))
            {
                coordsParsed = true;
            }
        }
        // Handle generic "Tile X Z" format (if still used)
        else if (parts.Length == 3 && parts[0] == "Tile" && int.TryParse(parts[1], out targetX) && int.TryParse(parts[2], out targetZ))
        {
            coordsParsed = true;
        }

        if (!coordsParsed)
        {
            Debug.LogError($"Invalid tile name format: {tileName}. Could not parse coordinates.");
            HighlightAvailableMoves(); // Re-highlight moves if parsing failed
            return;
        }

        // Calculate Manhattan distance to check if the move is valid
        // Manhattan distance: abs(x1 - x2) + abs(z1 - z2)
        int distance = Mathf.Abs(currentTilePosition.x - targetX) + Mathf.Abs(currentTilePosition.z - targetZ);

        bool isValidMove = distance > 0 && distance <= remainingMoves;

        if (isValidMove)
        {
            // Move the player
            playerBeingMoved = players[currentPlayerIndex]; // Assign the player being moved
            StartCoroutine(MovePlayer(players[currentPlayerIndex], targetTile.transform.position));
        }
        else
        {
            Debug.Log("Invalid move! Distance: " + distance + ", Remaining Moves: " + remainingMoves);
            HighlightAvailableMoves(); // Re-highlight if move was invalid
        }
    }

    // Coroutine for smooth player movement
    private IEnumerator MovePlayer(GameObject player, Vector3 targetPosition)
    {
        isMoving = true;
        Vector3 startPos = player.transform.position;
        // Adjust target position for player height, assuming tiles are at y=0
        Vector3 adjustedTargetPosition = new Vector3(targetPosition.x, startPos.y, targetPosition.z);

        float journeyLength = Vector3.Distance(startPos, adjustedTargetPosition);
        float startTime = Time.time;

        while (Vector3.Distance(player.transform.position, adjustedTargetPosition) > 0.01f)
        {
            float distCovered = (Time.time - startTime) * movementSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            player.transform.position = Vector3.Lerp(startPos, adjustedTargetPosition, fractionOfJourney);
            yield return null; // Wait for next frame
        }

        player.transform.position = adjustedTargetPosition; // Snap to final position
        isMoving = false; // Movement finished

        UpdateCurrentTilePosition(); // Update the player's internal tile position after the move

        remainingMoves--;

        // Check if turn ends
        if (remainingMoves <= 0)
        {
            EndTurn();
        }
        else
        {
            HighlightAvailableMoves(); // Re-highlight for remaining moves
        }
    }

    // Highlights tiles within the current player's remaining movement range
    void HighlightAvailableMoves()
    {
        ClearHighlightedTiles(); // Clear previous highlights

        for (int x = 0; x < boardWidth; x++)
        {
            for (int z = 0; z < boardHeight; z++)
            {
                int distance = Mathf.Abs(currentTilePosition.x - x) + Mathf.Abs(currentTilePosition.z - z);
                if (distance > 0 && distance <= remainingMoves)
                {
                    // Find the tile by name (assuming naming convention "Tile_X_Z" or similar)
                    GameObject tileToHighlight = FindTileByCoordinates(x, z);
                    if (tileToHighlight != null)
                    {
                        // Apply highlight material
                        Renderer tileRenderer = tileToHighlight.GetComponent<Renderer>();
                        if (tileRenderer != null && highlightMaterial != null)
                        {
                            tileRenderer.material = highlightMaterial;
                            highlightedTiles.Add(tileToHighlight);
                        }
                    }
                }
            }
        }
    }

    // Clears all currently highlighted tiles
    void ClearHighlightedTiles()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            // Restore original material (assuming default is white, or you store original materials)
            // You might need a more robust way to store and restore original materials if you have varied tile types.
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                // Simple: reset to a default white material, or null to revert to prefab default
                tileRenderer.material = null; // Reverts to the material defined on the Mesh Renderer in the prefab
            }
        }
        highlightedTiles.Clear();
    }

    // Ends the current player's turn and switches to the next player
    void EndTurn()
    {
        ClearHighlightedTiles();
        remainingMoves = 0; // Ensure moves are reset

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count; // Move to next player

        UpdateCurrentTilePosition(); // Update current tile for the new player
        remainingMoves = movesPerTurn; // Reset moves for the new player
        HighlightAvailableMoves();

        Debug.Log("Player " + (currentPlayerIndex + 1) + "'s turn. Remaining Moves: " + remainingMoves);
    }

    // --- Helper Methods ---

    // Populates the boardTiles list by finding all tiles under the "Board" GameObject
    void PopulateBoardTilesArray()
    {
        GameObject boardParent = GameObject.Find("Board"); // IMPORTANT: BoardGenerator names its parent "Board"
        if (boardParent != null)
        {
            foreach (Transform child in boardParent.transform)
            {
                boardTiles.Add(child.gameObject);
            }
            Debug.Log($"Found {boardTiles.Count} board tiles.");
        }
        else
        {
            Debug.LogError("Could not find 'Board' GameObject. Ensure your BoardGenerator creates a parent named 'Board'.");
        }
    }

    // Finds a tile GameObject by its (x, z) coordinates, assuming naming convention.
    private GameObject FindTileByCoordinates(int x, int z)
    {
        // Iterates through the list of board tiles and parses their names.
        // This is efficient enough for small to medium boards.
        // For very large boards, a dictionary mapping coordinates to GameObjects might be faster.
        foreach (GameObject tile in boardTiles)
        {
            string tileName = tile.name;
            string[] parts = tileName.Split('_');

            int tileX = 0;
            int tileZ = 0;
            bool parsed = false;

            // Handle names like "Tile_(X,Z)" or "TeleportTile_Pair_1_A_(X,Z)"
            if (parts.Length > 0 && parts[parts.Length - 1].Contains("(") && parts[parts.Length - 1].Contains(")"))
            {
                string coordsString = parts[parts.Length - 1].Trim('(', ')');
                string[] coordParts = coordsString.Split(',');
                if (coordParts.Length == 2 && int.TryParse(coordParts[0], out tileX) && int.TryParse(coordParts[1], out tileZ))
                {
                    parsed = true;
                }
            }
            // Fallback for generic "Tile X Z" format
            else if (parts.Length == 3 && parts[0] == "Tile" && int.TryParse(parts[1], out tileX) && int.TryParse(parts[2], out tileZ))
            {
                parsed = true;
            }


            if (parsed && tileX == x && tileZ == z)
            {
                return tile;
            }
        }
        return null; // Tile not found
    }

    // Initializes player objects and attaches PlayerHealth scripts if not already present
    void InitializePlayers()
    {
        // Get references to players from the scene if not assigned in Inspector
        if (players == null || players.Count == 0)
        {
            GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
            players = new List<GameObject>(foundPlayers);
            // Sort players if needed, e.g., by name or a specific order
            // players.Sort((p1, p2) => string.Compare(p1.name, p2.name));
        }

        // Ensure each player has a PlayerHealth component
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerHealth>() == null)
            {
                player.AddComponent<PlayerHealth>();
                Debug.LogWarning($"Added PlayerHealth component to {player.name}. Consider adding it to the prefab.");
            }
        }
    }

    // Sets the initial positions of players based on board layout
    void SetInitialPlayerPositions()
    {
        // Example: Place players at specific corners or predetermined tiles
        if (players.Count >= 1)
        {
            players[0].transform.position = GetWorldPositionForTile(0, 0);
        }
        if (players.Count >= 2)
        {
            players[1].transform.position = GetWorldPositionForTile(boardWidth - 1, boardHeight - 1);
        }
        if (players.Count >= 3)
        {
            players[2].transform.position = GetWorldPositionForTile(0, boardHeight - 1);
        }
        if (players.Count >= 4)
        {
            players[3].transform.position = GetWorldPositionForTile(boardWidth - 1, 0);
        }
        // Add more initial positions if you have more players
    }

    // Converts tile coordinates (x, z) to world position
    Vector3 GetWorldPositionForTile(int x, int z)
    {
        // Assuming the 'TurnBasedMovement' GameObject itself is at (0,0,0) or acts as the board's origin.
        // Adjust for tile spacing and add half a tileSpacing to center on tile
        float worldX = x * tileSpacing;
        float worldZ = z * tileSpacing;
        // Y position might need adjustment depending on your tile height and player pivot
        float worldY = 0.5f; // Example: assuming players stand 0.5 units high above tiles at y=0

        return new Vector3(worldX, worldY, worldZ);
    }

    // --- Special Tile Interactions (called by SpecialTileTrigger.cs) ---

    public void DamagePlayer(GameObject player, int damage)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{player.name} took {damage} damage. Health: {playerHealth.currentHealth}");
        }
    }

    public void FreezeOpponent(GameObject player, int turns)
    {
        // Placeholder for freezing logic
        Debug.Log($"{player.name} is frozen for {turns} turns.");
        // You would implement actual "freeze" logic here, perhaps by setting a flag on the player's movement component
        // or deducting moves from their next turn.
    }

    public void PaintballOpponent(GameObject player, Color color)
    {
        // Placeholder for paintball logic
        Debug.Log($"{player.name} got painted {color}.");
        // You would change the player's material or spawn a particle effect here.
        Renderer playerRenderer = player.GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.material.color = color;
        }
    }
}