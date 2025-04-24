using UnityEngine;
using Random = UnityEngine.Random;

public class LandmineSpawner : MonoBehaviour
{
    public GameObject landminePrefab; // Assign your landmine PNG prefab in the Inspector
    public int width = 15;
    public int height = 5;
    public float landmineSpawnChance = 0.2f; // Adjust the probability (0.0 to 1.0)
    public string playerTag = "Player"; // Tag of your player GameObject
    public int damageAmount = 15;

    private GameObject[,] boardTiles; // To store references to the board tiles

    void Start()
    {
        GenerateBoardWithLandmines();
    }

    void GenerateBoardWithLandmines()
    {
        // Assuming you have a BoardGenerator script attached to the same GameObject
        BoardGenerator boardGenerator = GetComponent<BoardGenerator>();
        if (boardGenerator == null || boardGenerator.tilePrefab == null)
        {
            Debug.LogError("BoardGenerator component not found or tilePrefab is not assigned!");
            return;
        }

        width = boardGenerator.width;
        height = boardGenerator.height;
        float tileSpacing = boardGenerator.tileSpacing;

        boardTiles = new GameObject[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 tilePosition = new Vector3(x * tileSpacing, 0, y * tileSpacing) + transform.position;
                GameObject tile = Instantiate(boardGenerator.tilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = "Tile_" + x + "_" + y;
                boardTiles[x, y] = tile; // Store the tile reference

                // Randomly spawn a landmine on this tile
                if (Random.value < landmineSpawnChance && landminePrefab != null)
                {
                    // Adjust the landmine's position to be slightly above the tile
                    Vector3 landminePosition = tile.transform.position + Vector3.up * 0.1f;
                    GameObject landmine = Instantiate(landminePrefab, landminePosition, Quaternion.identity);
                    landmine.transform.parent = tile.transform; // Make the landmine a child of the tile

                    // Optionally, add a component to the tile to indicate it has a landmine
                    LandmineTrigger trigger = tile.AddComponent<LandmineTrigger>();
                    trigger.damage = damageAmount;
                    trigger.playerTag = playerTag;
                }
            }
        }
        //boardGenerator.CenterBoard(); // Ensure the board is centered
    }
}