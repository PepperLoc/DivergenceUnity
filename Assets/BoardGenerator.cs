using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public GameObject tilePrefab; // Assign your tile prefab in the Inspector
    public int width = 5;
    public int height = 15;
    public float tileSpacing = 1.0f; // Adjust spacing as needed
    public Vector3 boardPosition = new Vector3(5.90514f, -69f, 998f); // Set your desired position

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

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 position = new Vector3(x * tileSpacing, 0, y * tileSpacing) + boardPosition; // Add board position offset
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = "Tile_" + x + "_" + y;
            }
        }
        CenterBoard(); // Center the board after positioning it.
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
}