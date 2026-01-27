using UnityEngine;

public class LevelMap : MonoBehaviour
{
    public static LevelMap Instance;

    /*
        3D Array: map[floor, y, x]
        
        floor = height level (0 = ground, 1 = mid, 2 = top)
        y = vertical on screen (isometric depth)
        x = horizontal on screen
        
        1 = solid ground (can stand)
        0 = empty (air/gap)
    */
    public int[,,] map = new int[3, 16, 16];

    public float tileSize = 1f;      // World units per tile
    public float floorHeight = 2f;   // World Z per floor

    void Awake()
    {
        Instance = this;
        BuildMap();
    }

    void BuildMap()
    {
        // Check which game level we're in
        int gameLevel = GameTracker.currentLevel; // or SceneManager.GetActiveScene().buildIndex

        switch (gameLevel)
        {
            case 1: BuildLevel1(); break;
            case 2: BuildLevel2(); break;
            case 3: BuildLevel3(); break;
            default: BuildLevel1(); break;
        }
    }

    void BuildLevel1()
    {
        string[] floor0 = {
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
    };
        LoadFloor(0, floor0);

        string[] floor1 = {
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "1111000000001111",
        "1111000000001111",
        "1111000000001111",
        "1111000000001111",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
    };
        LoadFloor(1, floor1);

        string[] floor2 = {
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000001111000000",
        "0000001111000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
    };
        LoadFloor(2, floor2);
    }

    void BuildLevel2()
    {
        string[] floor0 = {
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
        "1111111111111111",
    };
        LoadFloor(0, floor0);

        string[] floor1 = {
        "1111111100000000",
        "1111111100000000",
        "0000000000000000",
        "0000000000000000",
        "0000000011111111",
        "0000000011111111",
        "0000000000000000",
        "0000000000000000",
        "1111111100000000",
        "1111111100000000",
        "0000000000000000",
        "0000000000000000",
        "0000000011111111",
        "0000000011111111",
        "0000000000000000",
        "0000000000000000",
    };
        LoadFloor(1, floor1);

        string[] floor2 = {
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000111111110000",
        "0000111111110000",
        "0000111111110000",
        "0000111111110000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
        "0000000000000000",
    };
        LoadFloor(2, floor2);
    }

    void BuildLevel3()
    {
        // Your level 3 layout here
        // Same pattern: floor0, floor1, floor2
    }
    /// <summary>
    /// Load floor from string array
    /// '1' = ground, '0' or anything else = empty
    /// </summary>
    void LoadFloor(int floor, string[] rows)
    {
        for (int y = 0; y < 16; y++)
        {
            if (y >= rows.Length) break;

            string row = rows[y];
            for (int x = 0; x < 16; x++)
            {
                if (x >= row.Length) break;
                map[floor, y, x] = row[x] == '1' ? 1 : 0;
            }
        }
    }


    // ===== CORE HELPERS =====

    /// <summary>
    /// Check if coordinates are within map bounds
    /// </summary>
    bool InBounds(int floor, int x, int y)
    {
        return floor >= 0 && floor < 3 &&
               x >= 0 && x < 16 &&
               y >= 0 && y < 16;
    }

    /// <summary>
    /// Check if tile has ground (is walkable)
    /// </summary>
    public bool HasGround(int floor, int x, int y)
    {
        if (!InBounds(floor, x, y)) return false;
        return map[floor, y, x] == 1;
    }

    /// <summary>
    /// Convert world position to grid coordinates (clamped to bounds)
    /// </summary>
    public Vector3Int GetGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.y / tileSize);
        int z = Mathf.RoundToInt(worldPos.z / floorHeight);

        // Clamp to prevent out of bounds
        x = Mathf.Clamp(x, 0, 15);
        y = Mathf.Clamp(y, 0, 15);
        z = Mathf.Clamp(z, 0, 2);

        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Convert grid coordinates to world position (center of tile)
    /// </summary>
    public Vector3 GetWorld(int floor, int x, int y)
    {
        return new Vector3(
            (x + 0.5f) * tileSize,
            (y + 0.5f) * tileSize,
            floor * floorHeight
        );
    }

    // ===== PLAYER ACTION CHECKS =====

    /// <summary>
    /// SPACE: Can jump up? (tile above must be solid)
    /// </summary>
    public bool CanJumpUp(Vector3 pos)
    {
        Vector3Int grid = GetGrid(pos);
        if (grid.z >= 2) return false; // Already at top
        return HasGround(grid.z + 1, grid.x, grid.y);
    }

    /// <summary>
    /// SHIFT: Can dash across? (max 3 empty tiles, same floor)
    /// Returns target position or null
    /// </summary>
    public Vector3? CanDashAcross(Vector3 pos, Vector2 dir)
    {
        Vector3Int grid = GetGrid(pos);

        int stepX = Mathf.RoundToInt(dir.x);
        int stepY = Mathf.RoundToInt(dir.y);

        if (stepX == 0 && stepY == 0) return null;

        int emptyCount = 0;
        int checkX = grid.x;
        int checkY = grid.y;

        for (int i = 0; i < 4; i++)
        {
            checkX += stepX;
            checkY += stepY;

            // Out of bounds
            if (!InBounds(grid.z, checkX, checkY)) return null;

            // Found ground
            if (HasGround(grid.z, checkX, checkY))
            {
                return emptyCount <= 3 ? GetWorld(grid.z, checkX, checkY) : null;
            }

            emptyCount++;
        }

        return null; // No ground found
    }

    /// <summary>
    /// Walking: About to walk off edge?
    /// </summary>
    public bool IsAtLedge(Vector3 pos, Vector2 dir)
    {
        Vector3Int grid = GetGrid(pos);
        if (grid.z == 0) return false; // Can't fall below floor 0

        int nextX = grid.x + Mathf.RoundToInt(dir.x);
        int nextY = grid.y + Mathf.RoundToInt(dir.y);

        return !HasGround(grid.z, nextX, nextY);
    }

    /// <summary>
    /// Get position one floor down
    /// </summary>
    public Vector3 GetPosBelow(Vector3 pos)
    {
        Vector3Int grid = GetGrid(pos);
        return GetWorld(Mathf.Max(0, grid.z - 1), grid.x, grid.y);
    }

    /// <summary>
    /// Get position one floor up
    /// </summary>
    public Vector3 GetPosAbove(Vector3 pos)
    {
        Vector3Int grid = GetGrid(pos);
        return GetWorld(Mathf.Min(2, grid.z + 1), grid.x, grid.y);
    }
}