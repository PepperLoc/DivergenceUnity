using UnityEngine;
using System.Collections.Generic;

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public int width = 5;
    public int height = 15;
    public float tileSpacing = 1.0f;
    public Vector3 boardPosition = Vector3.zero;
    public Vector3 tileScale = Vector3.one;

    // Special tile prefabs
    public GameObject landmineTilePrefab;
    public GameObject freezeTilePrefab;
    public GameObject paintballTilePrefab;
    public GameObject teleporterTilePrefab;

    // Number of special tiles to spawn
    public int numLandmineTiles = 3;
    public int numFreezeTiles = 2;
    public int numPaintballTiles = 2;
    public int numTeleporterTiles = 2;

    // Special tile settings
    public int landmineDamage = 50;
    public int freezeTurns = 3;
    public Color paintballColor = Color.green;
    public int teleporterId1 = 1; // Unique ID for teleporter pair 1
    public int teleporterId2 = 2; // Unique ID for teleporter pair 2

    // 2D array to store tile positions and instantiated tiles
    private GameObject[,] boardTiles;

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab is not assigned in BoardGenerator!");
            return;
        }

        GameObject boardParent = new GameObject("Board");
        boardParent.transform.position = boardPosition;

        // Initialize the 2D array
        boardTiles = new GameObject[width, height];

        // Calculate tile positions
        Vector3[,] tilePositions = new Vector3[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 tilePosition = new Vector3(
                    x * tileSpacing,
                    0,
                    z * tileSpacing
                ) + boardPosition;
                tilePositions[x, z] = tilePosition;
            }
        }

        // Keep track of special tile positions
        List<Vector2Int> specialTilePositions = new List<Vector2Int>();

        // Instantiate regular tiles first
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject tile = Instantiate(tilePrefab, tilePositions[x, z], Quaternion.identity, boardParent.transform);
                tile.transform.localScale = tileScale;
                tile.name = "Tile_" + x + "_" + z;

                // Add ClickableTile component and set TurnBasedMovement reference
                ClickableTile clickableTile = tile.AddComponent<ClickableTile>();
                TurnBasedMovement turnBasedMovement = FindObjectOfType<TurnBasedMovement>();
                if (turnBasedMovement != null)
                {
                    clickableTile.turnBasedMovement = turnBasedMovement;
                }
                else
                {
                    Debug.LogError("TurnBasedMovement script not found in the scene. Make sure you have a GameObject with TurnBasedMovement attached.");
                }

                boardTiles[x, z] = tile; // Store the tile in the array
            }
        }

        // Instantiate special tiles
        SpawnSpecialTiles(landmineTilePrefab, numLandmineTiles, boardParent.transform, tilePositions, specialTilePositions);
        SpawnSpecialTiles(freezeTilePrefab, numFreezeTiles, boardParent.transform, tilePositions, specialTilePositions);
        SpawnSpecialTiles(paintballTilePrefab, numPaintballTiles, boardParent.transform, tilePositions, specialTilePositions);
        SpawnTeleporterTiles(teleporterTilePrefab, numTeleporterTiles / 2, boardParent.transform, tilePositions, specialTilePositions); // Spawn half, since it's pairs
    }



    // Helper function to spawn special tiles
    void SpawnSpecialTiles(GameObject prefab, int count, Transform parent, Vector3[,] tilePositions, List<Vector2Int> spawnedPositions)
    {
        if (prefab != null)
        {
            for (int i = 0; i < count; i++)
            {
                int x, z;
                do
                {
                    x = Random.Range(0, width);
                    z = Random.Range(0, height);
                } while (spawnedPositions.Contains(new Vector2Int(x, z))); // Ensure no overlap

                // Destroy the normal tile at this position
                if (boardTiles[x, z] != null)
                {
                    Destroy(boardTiles[x, z]);
                }

                GameObject specialTile = Instantiate(prefab, tilePositions[x, z], Quaternion.identity, parent);
                specialTile.transform.localScale = tileScale;
                specialTile.name = prefab.name + "_" + x + "_" + z;

                // Add ClickableTile component and set TurnBasedMovement reference
                ClickableTile clickableTile = specialTile.AddComponent<ClickableTile>();
                TurnBasedMovement turnBasedMovement = FindObjectOfType<TurnBasedMovement>();
                if (turnBasedMovement != null)
                {
                    clickableTile.turnBasedMovement = turnBasedMovement;
                }
                else
                {
                    Debug.LogError("TurnBasedMovement script not found in the scene. Make sure you have a GameObject with TurnBasedMovement attached.");
                }

                boardTiles[x, z] = specialTile; // Store the special tile in the array
                spawnedPositions.Add(new Vector2Int(x, z));
            }
        }
    }

    //spawn teleporters, 2 at a time
    void SpawnTeleporterTiles(GameObject prefab, int pairCount, Transform parent, Vector3[,] tilePositions, List<Vector2Int> spawnedPositions)
    {
        if (prefab != null)
        {
            for (int i = 0; i < pairCount; i++)
            {
                int x1, z1, x2, z2;
                do
                {
                    x1 = Random.Range(0, width);
                    z1 = Random.Range(0, height);
                    x2 = Random.Range(0, width);
                    z2 = Random.Range(0, height);
                } while (spawnedPositions.Contains(new Vector2Int(x1, z1)) || spawnedPositions.Contains(new Vector2Int(x2, z2)) || (x1 == x2 && z1 == z2));

                //teleporter 1
                if (boardTiles[x1, z1] != null)
                {
                    Destroy(boardTiles[x1, z1]);
                }
                GameObject teleporter1 = Instantiate(prefab, tilePositions[x1, z1], Quaternion.identity, parent);
                teleporter1.transform.localScale = tileScale;
                teleporter1.name = prefab.name + "_" + x1 + "_" + z1 + "_" + teleporterId1;

                // Add ClickableTile component and set TurnBasedMovement reference
                ClickableTile clickableTile1 = teleporter1.AddComponent<ClickableTile>();
                TurnBasedMovement turnBasedMovement1 = FindObjectOfType<TurnBasedMovement>();
                if (turnBasedMovement1 != null)
                {
                    clickableTile1.turnBasedMovement = turnBasedMovement1;
                }
                else
                {
                    Debug.LogError("TurnBasedMovement script not found in the scene. Make sure you have a GameObject with TurnBasedMovement attached.");
                }
                boardTiles[x1, z1] = teleporter1;
                spawnedPositions.Add(new Vector2Int(x1, z1));

                //teleporter 2
                if (boardTiles[x2, z2] != null)
                {
                    Destroy(boardTiles[x2, z2]);
                }
                GameObject teleporter2 = Instantiate(prefab, tilePositions[x2, z2], Quaternion.identity, parent);
                teleporter2.transform.localScale = tileScale;
                teleporter2.name = prefab.name + "_" + x2 + "_" + z2 + "_" + teleporterId1;
                ClickableTile clickableTile2 = teleporter2.AddComponent<ClickableTile>();
                TurnBasedMovement turnBasedMovement2 = FindObjectOfType<TurnBasedMovement>();
                if (turnBasedMovement2 != null)
                {
                    clickableTile2.turnBasedMovement = turnBasedMovement2;
                }
                else
                {
                    Debug.LogError("TurnBasedMovement script not found in the scene. Make sure you have a GameObject with TurnBasedMovement attached.");
                }
                boardTiles[x2, z2] = teleporter2;
                spawnedPositions.Add(new Vector2Int(x2, z2));
            }
        }
    }
}

