using UnityEngine;
using Random = UnityEngine.Random;

public class LandmineSpawner : MonoBehaviour
{
    public GameObject landminePrefab;
    public int width = 5;
    public int height = 15;
    public float landmineSpawnChance = 0.2f;
    public string playerTag = "Player";
    public int damageAmount = 15;
    public float tileSpacing = 1.0f; // Add this public variable for tile spacing

    private GameObject[,] boardTiles;

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
        // We will now use the local tileSpacing variable instead of the one from BoardGenerator
        // float tileSpacing = boardGenerator.tileSpacing;

        boardTiles = new GameObject[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 tilePosition = new Vector3(x * tileSpacing, 0, y * tileSpacing) + transform.position;
                GameObject tile = Instantiate(boardGenerator.tilePrefab, tilePosition, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = "Tile_" + x + "_" + y;
                boardTiles[x, y] = tile;

                if (Random.value < landmineSpawnChance && landminePrefab != null)
                {
                    Vector3 landminePosition = tile.transform.position + Vector3.up * 0.1f;
                    GameObject landmine = Instantiate(landminePrefab, landminePosition, Quaternion.identity);
                    landmine.transform.parent = tile.transform;

                    LandmineTrigger trigger = tile.AddComponent<LandmineTrigger>();
                    trigger.damage = damageAmount;
                    trigger.playerTag = playerTag;
                }
            }
        }
        // If you want the LandmineSpawner to handle centering based on its own spacing,
        // you would need to modify the CenterBoard logic or create a separate centering function here.
        // For now, we'll assume the BoardGenerator's centering is sufficient.
        // boardGenerator.CenterBoard();
    }
}