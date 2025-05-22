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
    public GameObject teleporterTilePrefab; // <--- This prefab WILL be used!

    // NO LONGER NEEDED: public Material teleporterTileMaterial; // <--- REMOVE THIS LINE!

    // Number of special tiles to spawn
    public int numLandmineTiles = 3;
    public int numFreezeTiles = 2;
    public int numPaintballTiles = 2;
    public int numTeleporterTiles = 2; // This still means 2 individual tiles for 1 pair

    // Special tile settings
    public int landmineDamage = 50;
    public int freezeTurns = 3;
    public Color paintballColor = Color.green;
    // public int teleporterId1 = 1; // You can remove these if not used for other logic
    // public int teleporterId2 = 2; // You can remove these if not used for other logic

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

        boardTiles = new GameObject[width, height];

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

        List<Vector2Int> specialTilePositions = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject tile = Instantiate(tilePrefab, tilePositions[x, z], Quaternion.identity, boardParent.transform);
                tile.transform.localScale = tileScale;
                tile.name = "Tile_" + x + "_" + z;

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

                boardTiles[x, z] = tile;
            }
        }

        SpawnSpecialTiles(landmineTilePrefab, numLandmineTiles, boardParent.transform, tilePositions, specialTilePositions);
        SpawnSpecialTiles(freezeTilePrefab, numFreezeTiles, boardParent.transform, tilePositions, specialTilePositions);
        SpawnSpecialTiles(paintballTilePrefab, numPaintballTiles, boardParent.transform, tilePositions, specialTilePositions);

        // Call SpawnTeleporterTiles, passing the prefab now
        SpawnTeleporterTiles(teleporterTilePrefab, numTeleporterTiles, boardParent.transform, tilePositions, specialTilePositions);
    }


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
                } while (spawnedPositions.Contains(new Vector2Int(x, z)));

                if (boardTiles[x, z] != null)
                {
                    Destroy(boardTiles[x, z]);
                }

                GameObject specialTile = Instantiate(prefab, tilePositions[x, z], Quaternion.identity, parent);
                specialTile.transform.localScale = tileScale;
                specialTile.name = prefab.name + "_" + x + "_" + z;

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

                boardTiles[x, z] = specialTile;
                spawnedPositions.Add(new Vector2Int(x, z));
            }
        }
    }

    // --- REVISED SpawnTeleporterTiles METHOD TO USE YOUR PREFAB ---
    void SpawnTeleporterTiles(GameObject prefab, int numTeleportersToSpawn, Transform parent, Vector3[,] tilePositions, List<Vector2Int> spawnedPositions)
    {
        if (prefab == null)
        {
            Debug.LogError("Teleporter Tile Prefab is not assigned in BoardGenerator! Cannot spawn teleporters.");
            return;
        }

        if (numTeleportersToSpawn % 2 != 0)
        {
            Debug.LogError("Number of teleporter tiles must be an even number to form pairs!");
            return;
        }

        List<Vector2Int> teleportGridPositions = new List<Vector2Int>();

        // First, find unique positions for all teleporters
        for (int i = 0; i < numTeleportersToSpawn; i++)
        {
            int x, z;
            do
            {
                x = Random.Range(0, width);
                z = Random.Range(0, height);
            } while (spawnedPositions.Contains(new Vector2Int(x, z)) || teleportGridPositions.Contains(new Vector2Int(x, z)));

            teleportGridPositions.Add(new Vector2Int(x, z));
            spawnedPositions.Add(new Vector2Int(x, z));
        }

        // Now, instantiate the actual teleport GameObjects using the prefab and link them
        for (int i = 0; i < numTeleportersToSpawn; i += 2) // Loop for each pair
        {
            Vector2Int pos1 = teleportGridPositions[i];
            Vector2Int pos2 = teleportGridPositions[i + 1];

            // --- Teleporter Tile 1 ---
            if (boardTiles[pos1.x, pos1.y] != null)
            {
                Destroy(boardTiles[pos1.x, pos1.y]);
            }
            // Instantiate the prefab!
            GameObject teleporter1 = Instantiate(prefab, tilePositions[pos1.x, pos1.y], Quaternion.identity, parent);
            teleporter1.transform.localScale = tileScale;
            teleporter1.name = $"TeleportTile_Pair_{(i / 2 + 1)}_A_({pos1.x},{pos1.y})";

            // Ensure it has a BoxCollider and is a trigger (your prefab should ideally have this)
            BoxCollider collider1 = teleporter1.GetComponent<BoxCollider>();
            if (collider1 == null)
            {
                collider1 = teleporter1.AddComponent<BoxCollider>();
            }
            collider1.isTrigger = true;
            // Adjust collider size if the prefab's default isn't suitable for a tile
            // collider1.size = new Vector3(tileScale.x, 0.1f, tileScale.z); // Uncomment and adjust if needed

            // Add the TeleportTile script (if your prefab doesn't already have it)
            TeleportTile script1 = teleporter1.GetComponent<TeleportTile>();
            if (script1 == null)
            {
                script1 = teleporter1.AddComponent<TeleportTile>();
            }


            // Add ClickableTile component and set TurnBasedMovement reference
            ClickableTile clickableTile1 = teleporter1.GetComponent<ClickableTile>();
            if (clickableTile1 == null) // Add if not already present on prefab
            {
                clickableTile1 = teleporter1.AddComponent<ClickableTile>();
            }
            TurnBasedMovement turnBasedMovement1 = FindObjectOfType<TurnBasedMovement>();
            if (turnBasedMovement1 != null)
            {
                clickableTile1.turnBasedMovement = turnBasedMovement1;
            }
            else
            {
                Debug.LogError("TurnBasedMovement script not found for teleporter1.");
            }

            boardTiles[pos1.x, pos1.y] = teleporter1;

            // --- Teleporter Tile 2 ---
            if (boardTiles[pos2.x, pos2.y] != null)
            {
                Destroy(boardTiles[pos2.x, pos2.y]);
            }
            // Instantiate the prefab!
            GameObject teleporter2 = Instantiate(prefab, tilePositions[pos2.x, pos2.y], Quaternion.identity, parent);
            teleporter2.transform.localScale = tileScale;
            teleporter2.name = $"TeleportTile_Pair_{(i / 2 + 1)}_B_({pos2.x},{pos2.y})";

            // Ensure it has a BoxCollider and is a trigger
            BoxCollider collider2 = teleporter2.GetComponent<BoxCollider>();
            if (collider2 == null)
            {
                collider2 = teleporter2.AddComponent<BoxCollider>();
            }
            collider2.isTrigger = true;
            // collider2.size = new Vector3(tileScale.x, 0.1f, tileScale.z); // Uncomment and adjust if needed

            // Add the TeleportTile script
            TeleportTile script2 = teleporter2.GetComponent<TeleportTile>();
            if (script2 == null)
            {
                script2 = teleporter2.AddComponent<TeleportTile>();
            }

            // Add ClickableTile component and set TurnBasedMovement reference
            ClickableTile clickableTile2 = teleporter2.GetComponent<ClickableTile>();
            if (clickableTile2 == null) // Add if not already present on prefab
            {
                clickableTile2 = teleporter2.AddComponent<ClickableTile>();
            }
            TurnBasedMovement turnBasedMovement2 = FindObjectOfType<TurnBasedMovement>();
            if (turnBasedMovement2 != null)
            {
                clickableTile2.turnBasedMovement = turnBasedMovement2;
            }
            else
            {
                Debug.LogError("TurnBasedMovement script not found for teleporter2.");
            }

            boardTiles[pos2.x, pos2.y] = teleporter2;

            // --- LINK THE TWO TELEPORTER TILES ---
            script1.destinationTeleportTile = teleporter2.transform;
            script2.destinationTeleportTile = teleporter1.transform;

            Debug.Log($"Teleporter pair spawned: ({pos1.x},{pos1.y}) connected to ({pos2.x},{pos2.y})");
        }
    }
}