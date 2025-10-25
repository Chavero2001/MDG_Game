using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator : MonoBehaviour
{
    public enum Cell : byte { Empty = 0, Grass = 1, Wall = 2, SideWalk = 3, Street = 4 }
    public enum Mode { City = 0 } // single oneshot mode

    [Header("Mode")]
    public Mode GenerationMode = Mode.City;
    public int Seed = 12345;

    [Header("Maps")]
    public Tilemap floorTileMap;
    public Tilemap wallTileMap;
    public Tile Street;
    public Tile SideWalk;
    public Tile Grass;
    public Tile Wall;
    public Transform parentTransform;

    [Header("Decorations")]
    public GameObject wallObject;
    public GameObject treePrefab;
    [Range(0f, 1f)] public float TreeDensity = 0.1f;
    [Min(0)] public int TreeStreetBuffer = 1;     // distance from Street
    [Min(0)] public int TreeSidewalkBuffer = 0;   // distance from SideWalk (set 1+ to keep off sidewalks)
    [Min(0)] public int TreeBuildingBuffer = 1;   // distance from Wall
    [Min(0)] public int TreeTreeBuffer = 2;
    public Vector2 TreeJitterXZ = new Vector2(0.2f, 0.2f);
    public Vector2 TreeScaleRange = new Vector2(0.9f, 1.15f);
    public GameObject streetLampPrefab;
    [Min(0f)] public float lampInwardBuffer = 0.5f; // how far to push lamp inwards
    [Min(1)] public int lampEveryNthCorner = 1;          // 1 = every corner, 2 = every other, etc.
    public bool orientLampOutward = true;

    [Header("Size")]
    public int MapWidth = 120;
    public int MapHeight = 80;

    [Header("City Grid")]
    [Min(4)] public int BlockSize = 16;
    [Min(1)] public int RoadWidth = 3;
    [Min(0)] public int SidewalkWidth = 1;
    [Range(0f, 0.5f)] public float GridJitterChance = 0.10f;

    [Header("Alleys (optional)")]
    [Range(0f, 1f)] public float AlleyChance = 0.25f;
    [Min(1)] public int AlleyWidth = 1;

    [Header("Parks (Grass)")]
    public int ParkAttempts = 35;
    public Vector2Int ParkSizeMinMax = new Vector2Int(6, 14);
    [Min(0)] public int ParkStreetSetback = 2;   // buffer from sidewalks/roads
    [Min(1)] public int EntranceWidth = 2;       // sidewalk width from park to sidewalk

    [Header("Buildings (Walls + prefabs)")]
    public int BuildingAttempts = 80;
    public Vector2Int BuildingSizeMinMax = new Vector2Int(4, 10);
    [Min(0)] public int BuildingSetbackFromRoad = 1;
    [Range(0f, 1f)] public float CourtyardChance = 0.2f;
    [Min(0)] public int CourtyardInset = 1;

    [Header("Visualization (optional)")]
    public bool Visualize = false;
    public float WaitTime = 0.01f;

    private Cell[,] grid;
    private System.Random rnd;

    // ---------- Lifecycle ----------
    void Awake()
    {
        if (floorTileMap != null)
        {
            floorTileMap.orientation = Tilemap.Orientation.XZ;
            floorTileMap.orientationMatrix = Matrix4x4.identity;
            floorTileMap.tileAnchor = new Vector3(0.5f, 0f, 0.5f);
        }
        if (wallTileMap != null)
        {
            wallTileMap.orientation = Tilemap.Orientation.XZ;
            wallTileMap.orientationMatrix = Matrix4x4.identity;
            wallTileMap.tileAnchor = new Vector3(0.5f, 0f, 0.5f);
        }
    }

    void Start() => Generate();

    public void Regenerate()
    {
        StopAllCoroutines();
        if (floorTileMap) floorTileMap.ClearAllTiles();
        if (wallTileMap) wallTileMap.ClearAllTiles();
        if (parentTransform)
            for (int i = parentTransform.childCount - 1; i >= 0; i--)
                Destroy(parentTransform.GetChild(i).gameObject);
        Generate();
    }

    public void Generate()
    {
        rnd = new System.Random(Seed);
        grid = new Cell[MapWidth, MapHeight]; // all Empty

        GenerateCityGrid();     // roads + sidewalks with jitter/alleys
        PlaceParks();           // carve Grass rectangles
        EnforceStreetRingAroundGrass();  // 1-tile Street ring only where cells were Empty
        PlaceBuildings();       // buildings (Walls) – no auto-ring
        FillAllEmptyToGrass();  // everything else becomes Grass

        PushToTilemaps();
        PlaceTrees();
        PlaceStreetLamps();
        if (Visualize) StartCoroutine(VisualizeTiles());
    }

    // ---------- Priority-safe writing ----------
    bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < MapWidth && y < MapHeight;

    // Street > SideWalk priority; prevents sidewalks inside intersections.
    void WriteCell(int x, int y, Cell t)
    {
        if (!InBounds(x, y)) return;
        var cur = grid[x, y];

        if (t == Cell.Street)
        {
            grid[x, y] = Cell.Street; // always promote to Street
        }
        else if (t == Cell.SideWalk)
        {
            if (cur != Cell.Street) grid[x, y] = Cell.SideWalk; // never overwrite Street
        }
        else
        {
            grid[x, y] = t; // Grass/Wall/Empty as-is
        }
    }

    // ---------- City Grid ----------
    void GenerateCityGrid()
    {
        // Vertical stripes
        for (int gx = BlockSize / 2; gx < MapWidth; gx += BlockSize)
        {
            int x = gx;
            if (rnd.NextDouble() < GridJitterChance) x = Mathf.Clamp(x + (rnd.Next(2) == 0 ? -1 : 1), 2, MapWidth - 3);
            CarveStripY(x, SidewalkWidth, RoadWidth, SidewalkWidth);
        }

        // Horizontal stripes
        for (int gy = BlockSize / 2; gy < MapHeight; gy += BlockSize)
        {
            int y = gy;
            if (rnd.NextDouble() < GridJitterChance) y = Mathf.Clamp(y + (rnd.Next(2) == 0 ? -1 : 1), 2, MapHeight - 3);
            CarveStripX(y, SidewalkWidth, RoadWidth, SidewalkWidth);
        }

        // Optional alleys
        if (AlleyChance > 0f && AlleyWidth > 0)
        {
            for (int gx = BlockSize; gx < MapWidth; gx += BlockSize)
                if (rnd.NextDouble() < AlleyChance) CarveVerticalAlleyNear(gx, AlleyWidth);

            for (int gy = BlockSize; gy < MapHeight; gy += BlockSize)
                if (rnd.NextDouble() < AlleyChance) CarveHorizontalAlleyNear(gy, AlleyWidth);
        }
    }

    void CarveStripY(int centerX, int sidewalkLeft, int road, int sidewalkRight)
    {
        int total = sidewalkLeft + road + sidewalkRight;
        int x0 = Mathf.Clamp(centerX - (total / 2), 0, MapWidth - 1);
        int x1 = Mathf.Clamp(centerX + (total - 1) / 2, 0, MapWidth - 1);

        for (int x = x0; x <= x1; x++)
        {
            bool leftWalk = (x - x0) < sidewalkLeft;
            bool rightWalk = (x1 - x) < sidewalkRight;
            bool inRoad = !leftWalk && !rightWalk;

            for (int y = 0; y < MapHeight; y++)
                WriteCell(x, y, inRoad ? Cell.Street : Cell.SideWalk);
        }
    }

    void CarveStripX(int centerY, int sidewalkTop, int road, int sidewalkBottom)
    {
        int total = sidewalkTop + road + sidewalkBottom;
        int y0 = Mathf.Clamp(centerY - (total / 2), 0, MapHeight - 1);
        int y1 = Mathf.Clamp(centerY + (total - 1) / 2, 0, MapHeight - 1);

        for (int y = y0; y <= y1; y++)
        {
            bool topWalk = (y - y0) < sidewalkTop;
            bool bottomWalk = (y1 - y) < sidewalkBottom;
            bool inRoad = !topWalk && !bottomWalk;

            for (int x = 0; x < MapWidth; x++)
                WriteCell(x, y, inRoad ? Cell.Street : Cell.SideWalk);
        }
    }

    void CarveVerticalAlleyNear(int nearX, int width)
    {
        int x = Mathf.Clamp(nearX + (rnd.Next(3) - 1), 1, MapWidth - 2);

        // if lots of road nearby, skip to avoid spam
        int roadCount = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int y = 0; y < MapHeight; y++)
                if (InBounds(x + dx, y) && grid[x + dx, y] == Cell.Street)
                { roadCount++; if (roadCount > MapHeight / 4) return; }

        for (int w = -width / 2; w <= (width - 1) / 2; w++)
            for (int y = 0; y < MapHeight; y++)
                WriteCell(Mathf.Clamp(x + w, 0, MapWidth - 1), y, Cell.Street);
    }

    void CarveHorizontalAlleyNear(int nearY, int width)
    {
        int y = Mathf.Clamp(nearY + (rnd.Next(3) - 1), 1, MapHeight - 2);

        int roadCount = 0;
        for (int dy = -1; dy <= 1; dy++)
            for (int x = 0; x < MapWidth; x++)
                if (InBounds(x, y + dy) && grid[x, y + dy] == Cell.Street)
                { roadCount++; if (roadCount > MapWidth / 4) return; }

        for (int w = -width / 2; w <= (width - 1) / 2; w++)
            for (int x = 0; x < MapWidth; x++)
                WriteCell(x, Mathf.Clamp(y + w, 0, MapHeight - 1), Cell.Street);
    }

    // ---------- Parks ----------
    void PlaceParks()
    {
        for (int i = 0; i < ParkAttempts; i++)
        {
            int w = rnd.Next(ParkSizeMinMax.x, ParkSizeMinMax.y + 1);
            int h = rnd.Next(ParkSizeMinMax.x, ParkSizeMinMax.y + 1);

            int x = rnd.Next(ParkStreetSetback + 1, MapWidth - w - ParkStreetSetback - 1);
            int y = rnd.Next(ParkStreetSetback + 1, MapHeight - h - ParkStreetSetback - 1);
            var r = new RectInt(x, y, w, h);

            if (!IsRectBuildable(r, ParkStreetSetback)) continue;

            // Park interior = Grass
            CarveRect(r, Cell.Grass);

            // Entrance walkway to nearest sidewalk (never overwrite Street)
            Vector2Int from = CenterInt(r);
            Vector2Int to = NearestSidewalkCell(from);
            if (to.x >= 0) CarveOrthogonalPathSidewalk(from, to, EntranceWidth);
        }
    }

    // Only affects Empty cells adjacent to Grass => makes a clean 1-tile street ring around parks.
    void EnforceStreetRingAroundGrass()
    {
        var outGrid = (Cell[,])grid.Clone();

        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
            {
                if (grid[x, y] != Cell.Empty) continue;

                bool adjGrass =
                    (x > 0 && grid[x - 1, y] == Cell.Grass) ||
                    (x + 1 < MapWidth && grid[x + 1, y] == Cell.Grass) ||
                    (y > 0 && grid[x, y - 1] == Cell.Grass) ||
                    (y + 1 < MapHeight && grid[x, y + 1] == Cell.Grass);

                if (adjGrass) outGrid[x, y] = Cell.Street;
            }

        grid = outGrid;
    }

    // ---------- Buildings ----------
    void PlaceBuildings()
    {
        for (int i = 0; i < BuildingAttempts; i++)
        {
            int w = rnd.Next(BuildingSizeMinMax.x, BuildingSizeMinMax.y + 1);
            int h = rnd.Next(BuildingSizeMinMax.x, BuildingSizeMinMax.y + 1);

            int x = rnd.Next(BuildingSetbackFromRoad + 1, MapWidth - w - BuildingSetbackFromRoad - 1);
            int y = rnd.Next(BuildingSetbackFromRoad + 1, MapHeight - h - BuildingSetbackFromRoad - 1);
            var r = new RectInt(x, y, w, h);

            if (!IsRectBuildable(r, BuildingSetbackFromRoad)) continue;

            // Building footprint
            CarveRect(r, Cell.Wall);

            // Optional courtyard (grass) inside
            if (rnd.NextDouble() < CourtyardChance &&
                r.width > 2 * (CourtyardInset + 1) &&
                r.height > 2 * (CourtyardInset + 1))
            {
                var inner = new RectInt(
                    r.x + CourtyardInset, r.y + CourtyardInset,
                    r.width - 2 * CourtyardInset, r.height - 2 * CourtyardInset
                );
                CarveRect(inner, Cell.Grass);
            }
        }
    }
    void PlaceTrees()
    {
        if (!treePrefab || !parentTransform) return;

        // Track where we’ve already placed trees to enforce TreeTreeBuffer
        bool[,] hasTree = new bool[MapWidth, MapHeight];

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                // Only plant on grass (your parks/courtyards)
                if (grid[x, y] != Cell.Grass) continue;

                // Respect global density first to keep things fast
                if (rnd.NextDouble() >= TreeDensity) continue;

                // Keep distance from roads/sidewalks/buildings
                if (TreeStreetBuffer > 0 && AnyNear(x, y, TreeStreetBuffer, c => c == Cell.Street)) continue;
                if (TreeSidewalkBuffer > 0 && AnyNear(x, y, TreeSidewalkBuffer, c => c == Cell.SideWalk)) continue;
                if (TreeBuildingBuffer > 0 && AnyNear(x, y, TreeBuildingBuffer, c => c == Cell.Wall)) continue;

                // Keep distance from other trees we placed this pass
                if (TreeTreeBuffer > 0 && AnyNear(x, y, TreeTreeBuffer, _ =>
                {
                    // Inline lookup using hasTree
                    for (int dy = -TreeTreeBuffer; dy <= TreeTreeBuffer; dy++)
                    {
                        int yy = y + dy; if (yy < 0 || yy >= MapHeight) continue;
                        for (int dx = -TreeTreeBuffer; dx <= TreeTreeBuffer; dx++)
                        {
                            int xx = x + dx; if (xx < 0 || xx >= MapWidth) continue;
                            if (hasTree[xx, yy]) return true;
                        }
                    }
                    return false;
                })) continue;

                // World position + a little random jitter for natural look
                Vector3 worldPos = floorTileMap != null
                    ? floorTileMap.GetCellCenterWorld(new Vector3Int(x, y, 0))
                    : new Vector3(x + 0.5f, 0f, y + 0.5f);

                float jx = (float)(rnd.NextDouble() * 2 - 1) * TreeJitterXZ.x;
                float jz = (float)(rnd.NextDouble() * 2 - 1) * TreeJitterXZ.y;
                worldPos += new Vector3(jx, 0f, jz);

                var go = Instantiate(treePrefab, worldPos, Quaternion.identity, parentTransform);

                // Random rotation/scale
                go.transform.Rotate(0f, rnd.Next(0, 360), 0f);
                float s = Mathf.Lerp(TreeScaleRange.x, TreeScaleRange.y, (float)rnd.NextDouble());
                go.transform.localScale *= s;

                hasTree[x, y] = true;
            }
        }
    }
    void PlaceStreetLamps()
    {
        if (!streetLampPrefab || !parentTransform) return;

        int cornerIndex = 0;

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                if (!IsSidewalkCorner(x, y, out int dirX, out int dirY)) continue;

                // Throttle: every Nth corner if you want fewer lamps
                if ((cornerIndex++ % lampEveryNthCorner) != 0) continue;
                Quaternion rot = Quaternion.identity;
                // World position of the sidewalk corner tile
                Vector3 worldPos = floorTileMap != null
                ? floorTileMap.GetCellCenterWorld(new Vector3Int(x, y, 0))
                : new Vector3(x + 0.5f, 0f, y + 0.5f);

                // Get outward normal (points toward street)
                Vector3 outward = new Vector3(-dirX, 0f, -dirY);

                // Shift lamp inward (opposite of outward)
                if (outward.sqrMagnitude > 0.5f)
                {
                    Vector3 inward = -outward.normalized;
                    worldPos += inward * lampInwardBuffer;

                    if (orientLampOutward)
                        rot = Quaternion.LookRotation(outward, Vector3.up);
                }


                Instantiate(streetLampPrefab, worldPos, rot, parentTransform);
            }
        }
    }

    /// <summary>
    /// A sidewalk "corner" is a SideWalk tile that has exactly one SideWalk neighbor on X
    /// and exactly one SideWalk neighbor on Y (an L turn, not a straight or T).
    /// Returns the directions of the sidewalk branches in dirX (±1) and dirY (±1).
    /// </summary>
    bool IsSidewalkCorner(int x, int y, out int dirX, out int dirY)
    {
        dirX = 0; dirY = 0;

        if (grid[x, y] != Cell.SideWalk) return false;

        bool left = x > 0 && grid[x - 1, y] == Cell.SideWalk;
        bool right = x + 1 < MapWidth && grid[x + 1, y] == Cell.SideWalk;
        bool down = y > 0 && grid[x, y - 1] == Cell.SideWalk;
        bool up = y + 1 < MapHeight && grid[x, y + 1] == Cell.SideWalk;

        // Must have exactly one sidewalk neighbor on X and exactly one on Y
        bool oneX = left ^ right;
        bool oneY = down ^ up;
        if (!(oneX && oneY)) return false;

        // (Optional but useful) Ensure it's really a street corner:
        // the outward diagonal should be Street. Compute the inside-corner normal first.
        // Branch directions (along sidewalks):
        dirX = right ? +1 : -1; // if right is true, branch goes +X; else it must be left -> -X
        dirY = up ? +1 : -1; // if up is true, branch goes +Y; else it must be down -> -Y

        // Outward diagonal is opposite of the inside corner: (-dirX, -dirY)
        int ox = x - dirX;
        int oy = y - dirY;
        if (InBounds(ox, oy) && grid[ox, oy] != Cell.Street)
        {
            // If you want lamps even when the diagonal isn’t street, comment this out.
            return false;
        }

        return true;
    }


    // ---------- Final fill ----------
    void FillAllEmptyToGrass()
    {
        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
                if (grid[x, y] == Cell.Empty) grid[x, y] = Cell.Grass;
    }

    // ---------- Tilemaps ----------
    void PushToTilemaps()
    {
        var floors = new List<Vector3Int>();
        var floorTiles = new List<TileBase>();
        var walls = new List<Vector3Int>();
        var wallTiles = new List<TileBase>();

        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
            {
                var cell = new Vector3Int(x, y, 0);
                switch (grid[x, y])
                {
                    case Cell.Street:
                        floors.Add(cell); floorTiles.Add(Street);
                        break;
                    case Cell.SideWalk:
                        floors.Add(cell); floorTiles.Add(SideWalk);
                        break;
                    case Cell.Grass:
                        floors.Add(cell); floorTiles.Add(Grass);
                        break;
                    case Cell.Wall:
                        walls.Add(cell); wallTiles.Add(Wall);
                        var worldPos = wallTileMap != null ? wallTileMap.GetCellCenterWorld(cell) : new Vector3(x, 0f, y);
                        if (wallObject && parentTransform) Instantiate(wallObject, worldPos, Quaternion.identity, parentTransform);
                        break;
                }
            }

        if (floorTileMap)
        {
            floorTileMap.ClearAllTiles();
            floorTileMap.SetTiles(floors.ToArray(), floorTiles.ToArray());
            floorTileMap.CompressBounds();
        }
        if (wallTileMap)
        {
            wallTileMap.ClearAllTiles();
            wallTileMap.SetTiles(walls.ToArray(), wallTiles.ToArray());
            wallTileMap.CompressBounds();
        }
    }

    System.Collections.IEnumerator VisualizeTiles()
    {
        if (floorTileMap) floorTileMap.ClearAllTiles();
        if (wallTileMap) wallTileMap.ClearAllTiles();

        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (grid[x, y] == Cell.Street) floorTileMap.SetTile(pos, Street);
                else if (grid[x, y] == Cell.SideWalk) floorTileMap.SetTile(pos, SideWalk);
                else if (grid[x, y] == Cell.Grass) floorTileMap.SetTile(pos, Grass);
                yield return new WaitForSeconds(WaitTime);
            }

        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
            {
                if (grid[x, y] != Cell.Wall) continue;
                var pos = new Vector3Int(x, y, 0);
                wallTileMap.SetTile(pos, Wall);
                var worldPos = wallTileMap.GetCellCenterWorld(pos);
                if (wallObject && parentTransform) Instantiate(wallObject, worldPos, Quaternion.identity, parentTransform);
                yield return new WaitForSeconds(WaitTime);
            }
    }

    // ---------- Helpers ----------
    static RectInt Inflate(RectInt r, int m)
        => new RectInt(r.xMin - m, r.yMin - m, r.width + 2 * m, r.height + 2 * m);

    static Vector2Int CenterInt(RectInt r)
        => new Vector2Int(r.x + r.width / 2, r.y + r.height / 2);

    void CarveRect(RectInt r, Cell type)
    {
        int xMin = Mathf.Max(0, r.xMin);
        int yMin = Mathf.Max(0, r.yMin);
        int xMax = Mathf.Min(MapWidth, r.xMax);
        int yMax = Mathf.Min(MapHeight, r.yMax);

        for (int y = yMin; y < yMax; y++)
            for (int x = xMin; x < xMax; x++)
                grid[x, y] = type;
    }

    bool IsRectBuildable(RectInt r, int setback)
    {
        var rr = Inflate(r, setback);
        if (rr.xMin < 1 || rr.yMin < 1 || rr.xMax > MapWidth - 1 || rr.yMax > MapHeight - 1) return false;

        for (int y = rr.yMin; y < rr.yMax; y++)
            for (int x = rr.xMin; x < rr.xMax; x++)
                if (grid[x, y] == Cell.Street || grid[x, y] == Cell.SideWalk) return false;
        return true;
    }

    Vector2Int NearestSidewalkCell(Vector2Int from)
    {
        var visited = new bool[MapWidth, MapHeight];
        var q = new Queue<Vector2Int>();
        q.Enqueue(from);
        visited[from.x, from.y] = true;

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            if (grid[p.x, p.y] == Cell.SideWalk)
            {
                // prefer boundary-ish sidewalks
                if ((p.x > 0 && grid[p.x - 1, p.y] == Cell.Empty) ||
                    (p.x + 1 < MapWidth && grid[p.x + 1, p.y] == Cell.Empty) ||
                    (p.y > 0 && grid[p.x, p.y - 1] == Cell.Empty) ||
                    (p.y + 1 < MapHeight && grid[p.x, p.y + 1] == Cell.Empty))
                    return p;
            }

            TryEnq(p + Vector2Int.right);
            TryEnq(p + Vector2Int.left);
            TryEnq(p + Vector2Int.up);
            TryEnq(p + Vector2Int.down);
        }
        return new Vector2Int(-1, -1);

        void TryEnq(Vector2Int v)
        {
            if (!InBounds(v.x, v.y)) return;
            if (visited[v.x, v.y]) return;
            visited[v.x, v.y] = true;
            q.Enqueue(v);
        }
    }

    // Scan a Chebyshev (square) neighborhood for any cell matching a predicate
    bool AnyNear(int cx, int cy, int radius, System.Func<Cell, bool> match)
    {
        if (radius <= 0) return false;
        for (int dy = -radius; dy <= radius; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= MapHeight) continue;
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx;
                if (x < 0 || x >= MapWidth) continue;
                if (match(grid[x, y])) return true;
            }
        }
        return false;
    }

    // Sidewalk path that NEVER overwrites Street (prevents sidewalks through intersections/roads)
    void CarveOrthogonalPathSidewalk(Vector2Int a, Vector2Int b, int width)
    {
        int x = a.x, y = a.y;
        int half = Mathf.Max(0, width / 2);

        void CarveChunk(int cx, int cy)
        {
            for (int dx = -half; dx <= half; dx++)
                for (int dy = -half; dy <= half; dy++)
                {
                    int nx = cx + dx, ny = cy + dy;
                    if (!InBounds(nx, ny)) continue;
                    if (grid[nx, ny] == Cell.Street) continue; // keep roads intact
                    WriteCell(nx, ny, Cell.SideWalk);
                }
        }

        while (x != b.x) { CarveChunk(x, y); x += x < b.x ? 1 : -1; }
        while (y != b.y) { CarveChunk(x, y); y += y < b.y ? 1 : -1; }
        CarveChunk(x, y);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Regenerated with Seed: " + Seed);
            Regenerate();
        }
    }
}
