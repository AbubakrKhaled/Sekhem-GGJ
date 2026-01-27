using UnityEngine.Tilemaps;
using UnityEngine;

public class LevelMap : MonoBehaviour
{
    public static LevelMap Instance;

    /*
        3D Array: map[floor, y, x]
        
        floor = height level (0 = ground, 1 = mid, 2 = top)
        y = grid Y (isometric)
        x = grid X (isometric)
        
        1 = solid ground
        0 = empty
        
        Size: 12x12 tiles, 3 floors
    */
    public int[,,] map = new int[3, 12, 12];  // Fixed to 12x12

    [Header("Tilemap Reference")]
    public Tilemap groundTilemap;  // Drag your ground tilemap here

    void Awake()
    {
        Instance = this;
        BuildMap();
    }

    void BuildMap()
    {
        int gameLevel = GameTracker.currentLevel;

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
        FillFloor(0, 1);
    }

    void BuildLevel2()
    {
        FillFloor(0, 1);

        string[] floor1 = {
            "111111000000",
            "111111000000",
            "000000000000",
            "000000111111",
            "000000111111",
            "000000000000",
            "111111000000",
            "111111000000",
            "000000000000",
            "000000111111",
            "000000111111",
            "000000000000",
        };
        LoadFloor(1, floor1);
    }

    void BuildLevel3()
    {
        FillFloor(0, 1);

        string[] floor1 = {
            "111000000111",
            "111000000111",
            "111000000111",
            "000000000000",
            "000111111000",
            "000111111000",
            "000111111000",
            "000111111000",
            "000000000000",
            "111000000111",
            "111000000111",
            "111000000111",
        };
        LoadFloor(1, floor1);

        string[] floor2 = {
            "000000000000",
            "000000000000",
            "000000000000",
            "000000000000",
            "000111111000",
            "000111111000",
            "000111111000",
            "000111111000",
            "000000000000",
            "000000000000",
            "000000000000",
            "000000000000",
        };
        LoadFloor(2, floor2);

    }

    // ===== HELPERS =====

    void FillFloor(int floor, int value)
    {
        for (int y = 0; y < 12; y++)
            for (int x = 0; x < 12; x++)
                map[floor, y, x] = value;
    }

    void LoadFloor(int floor, string[] rows)
    {
        for (int y = 0; y < 12; y++)
        {
            if (y >= rows.Length) break;

            string row = rows[y];
            for (int x = 0; x < 12; x++)
            {
                if (x >= row.Length) break;
                map[floor, y, x] = row[x] == '1' ? 1 : 0;
            }
        }
    }

    bool InBounds(int floor, int x, int y)
    {
        return floor >= 0 && floor < 3 &&
               x >= 0 && x < 12 &&
               y >= 0 && y < 12;
    }

    public bool HasGround(int floor, int x, int y)
    {
        if (!InBounds(floor, x, y)) return false;
        return map[floor, y, x] == 1;
    }

    // ===== GRID CONVERSION (Uses Tilemap!) =====

    /// <summary>
    /// Convert world position to grid coordinates
    /// Uses Tilemap.WorldToCell - no manual math!
    /// </summary>
    public Vector2Int GetGrid(Vector3 worldPos)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);

        int x = Mathf.Clamp(cell.x, 0, 11);
        int y = Mathf.Clamp(cell.y, 0, 11);

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Convert grid coordinates to world position
    /// Uses Tilemap.GetCellCenterWorld - no manual math!
    /// </summary>
    public Vector3 GetWorld(int x, int y)
    {
        return groundTilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
    }

    // ===== PLAYER ACTIONS (floor passed explicitly) =====

    /// <summary>
    /// SPACE: Can jump up? (tile above must be solid)
    /// </summary>
    public bool CanJumpUp(int floor, Vector3 worldPos)
    {
        if (floor >= 2) return false;  // Already at top

        Vector2Int grid = GetGrid(worldPos);
        return HasGround(floor + 1, grid.x, grid.y);
    }

    /// <summary>
    /// SHIFT: Can dash across? (max 3 empty tiles, same floor)
    /// </summary>
    public Vector3? CanDashAcross(int floor, Vector3 worldPos, Vector2 dir)
    {
        Vector2Int grid = GetGrid(worldPos);

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

            if (!InBounds(floor, checkX, checkY)) return null;

            if (HasGround(floor, checkX, checkY))
            {
                return emptyCount <= 3 ? GetWorld(checkX, checkY) : null;
            }

            emptyCount++;
        }

        return null;
    }

    /// <summary>
    /// Walking: About to walk off edge?
    /// </summary>
    public bool IsAtLedge(int floor, Vector3 worldPos, Vector2 dir)
    {
        if (floor == 0) return false;  // Can't fall below floor 0

        Vector2Int grid = GetGrid(worldPos);

        int nextX = grid.x + Mathf.RoundToInt(dir.x);
        int nextY = grid.y + Mathf.RoundToInt(dir.y);

        return !HasGround(floor, nextX, nextY);
    }

    /// <summary>
    /// Get world position at same grid cell (for floor changes)
    /// </summary>
    public Vector3 GetCurrentCellWorld(Vector3 worldPos)
    {
        Vector2Int grid = GetGrid(worldPos);
        return GetWorld(grid.x, grid.y);
    }
}


