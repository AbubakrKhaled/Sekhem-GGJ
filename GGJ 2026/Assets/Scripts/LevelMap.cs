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

    public float tileSize = 5f;      // World units per tile
    public float floorHeight = 1f;   // World Z per floor

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


    // ===== CORE HELPERS =====

    /// <summary>
    /// Check if coordinates are within map bounds
    /// </summary>
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

    public Vector3Int GetGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.y / tileSize);
        int z = Mathf.RoundToInt(worldPos.z / floorHeight);

        x = Mathf.Clamp(x, 0, 11);  // 0-11 for 12 tiles
        y = Mathf.Clamp(y, 0, 11);
        z = Mathf.Clamp(z, 0, 2);

        return new Vector3Int(x, y, z);
    }

    public Vector3 GetWorld(int floor, int x, int y)
    {
        return new Vector3(
            (x + 0.5f) * tileSize,  // Center of tile
            (y + 0.5f) * tileSize,
            floor * floorHeight
        );
    }

    // ===== PLAYER ACTIONS =====

    public bool CanJumpUp(Vector3 pos)
    {
        Vector3Int grid = GetGrid(pos);
        if (grid.z >= 2) return false;
        return HasGround(grid.z + 1, grid.x, grid.y);
    }

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

            if (!InBounds(grid.z, checkX, checkY)) return null;

            if (HasGround(grid.z, checkX, checkY))
            {
                return emptyCount <= 3 ? GetWorld(grid.z, checkX, checkY) : null;
            }

            emptyCount++;
        }

        return null;
    }

    public bool IsAtLedge(Vector3 pos, Vector2 dir)
    {
        Vector3Int grid = GetGrid(pos);
        if (grid.z == 0) return false;

        int nextX = grid.x + Mathf.RoundToInt(dir.x);
        int nextY = grid.y + Mathf.RoundToInt(dir.y);

        return !HasGround(grid.z, nextX, nextY);
    }

    public Vector3 GetPosBelow(Vector3 pos)
    {
        Vector3Int grid = GetGrid(pos);
        return GetWorld(Mathf.Max(0, grid.z - 1), grid.x, grid.y);
    }

    public Vector3 GetPosAbove(Vector3 pos)
    {
        Vector3Int grid = GetGrid(pos);
        return GetWorld(Mathf.Min(2, grid.z + 1), grid.x, grid.y);
    }
}