using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public int width = 5;
    public int height = 15;
    public float tileSpacing = 1.0f;
    public Vector3 boardPosition = new Vector3(0.00486918f, 0.0008259225f, -0.009996189f);
    public Vector3 tileScale = Vector3.one;

    [Header("Special Tile Prefabs")]
    public GameObject landminePrefab;
    public GameObject freezeTilePrefab;
    public GameObject paintballTilePrefab;
    public GameObject teleporterTilePrefab;

    [Header("Special Tile Counts")]
    public int numLandmines = 10; //example
    public int numFreezeTiles = 2;
    public int numPaintballTiles = 2;
    public int numTeleporterTiles = 2;

    [Header("Special Tile Settings")]
    public int landmineDamage = 15;
    public int freezeTurns = 1;
    public Color paintballColor = Color.magenta;
    public bool canTeleportToOccupied = false;

    private GameObject[,] boardTiles;

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned!");
            return;
        }

        boardTiles = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 position = new Vector3(x * tileSpacing, 0, y * tileSpacing) + boardPosition;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = "Tile_" + x + "_" + y;
                tile.transform.localScale = tileScale;
                boardTiles[x, y] = tile;
            }
        }
        CenterBoard();
        SpawnSpecialTiles();
    }

    void CenterBoard()
    {
        float boardWidth = (width - 1) * tileSpacing;
        float boardHeight = (height - 1) * tileSpacing;
        Vector3 offset = new Vector3(-boardWidth / 2, 0, -boardHeight / 2);
        foreach (Transform child in transform)
        {
            child.position += offset;
        }
    }

    void SpawnSpecialTiles()
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                availableTiles.Add(new Vector2Int(x, y));
            }
        }

        int tilesSpawned = 0;

        tilesSpawned = SpawnSpecificTiles(availableTiles, numLandmines, landminePrefab, SpecialTileType.Landmine, tilesSpawned);
        tilesSpawned = SpawnSpecificTiles(availableTiles, numFreezeTiles, freezeTilePrefab, SpecialTileType.Freeze, tilesSpawned);
        tilesSpawned = SpawnSpecificTiles(availableTiles, numPaintballTiles, paintballTilePrefab, SpecialTileType.Paintball, tilesSpawned);
        SpawnSpecificTiles(availableTiles, numTeleporterTiles, teleporterTilePrefab, SpecialTileType.Teleporter, tilesSpawned);
    }

    int SpawnSpecificTiles(List<Vector2Int> availableTiles, int count, GameObject prefab, SpecialTileType type, int tilesSpawned)
    {
        for (int i = 0; i < count; i++)
        {
            if (availableTiles.Count == 0) break;
            int randomIndex = Random.Range(0, availableTiles.Count);
            Vector2Int tileCoords = availableTiles[randomIndex];
            availableTiles.RemoveAt(randomIndex);

            GameObject tile = boardTiles[tileCoords.x, tileCoords.y];
            Vector3 spawnPosition = tile.transform.position + Vector3.up * 0.1f;
            GameObject specialTile = Instantiate(prefab, spawnPosition, Quaternion.identity);
            specialTile.transform.parent = tile.transform;

            SpecialTileTrigger trigger = tile.AddComponent<SpecialTileTrigger>();
            trigger.tileType = type;
            trigger.playerTag = "Player";

            switch (type)
            {
                case SpecialTileType.Landmine:
                    trigger.damage = landmineDamage;
                    break;
                case SpecialTileType.Freeze:
                    trigger.freezeTurns = freezeTurns;
                    break;
                case SpecialTileType.Paintball:
                    trigger.paintballColor = paintballColor;
                    break;
                case SpecialTileType.Teleporter:
                    trigger.isTeleporter = true;
                    break;
            }
            tilesSpawned++;
        }
        return tilesSpawned;
    }

    public GameObject GetTile(int x, int z)
    {
        if (x >= 0 && x < width && z >= 0 && z < height)
        {
            return boardTiles[x, z];
        }
        return null;
    }

    public Vector3 GetTilePosition(int x, int z)
    {
        GameObject tile = GetTile(x, z);
        if (tile != null)
        {
            return tile.transform.position;
        }
        return Vector3.zero;
    }
}
