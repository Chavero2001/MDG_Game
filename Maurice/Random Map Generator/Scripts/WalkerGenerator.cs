using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator : MonoBehaviour
{
    public enum Cell : byte { Empty = 0, Floor = 1, Wall = 2 }
    public enum Mode { Caves, Dungeon }

    [Header("Mode")]
    public Mode GenerationMode = Mode.Caves;
    public int Seed = 12345;

    [Header("Maps")]
    public Tilemap floorTileMap;
    public Tilemap wallTileMap;
    public Tile Floor;
    public Tile Wall;
    public GameObject wallObject;
    public Transform parentTransform;

    [Header("Size")]
    public int MapWidth = 120;
    public int MapHeight = 80;

    [Header("Walls")]
    public bool FillRemainderAsWalls = true;

    // ---- CAVES (drunk-walk) params ----
    [Header("Caves Settings")]
    [Range(0f, 1f)] public float FillPercentage = 0.40f;
    public int MaximumWalkers = 10;
    [Range(0f, 1f)] public float ChangeChance = 0.5f;

    // ---- DUNGEON (rooms + corridors) params ----
    [Header("Dungeon Settings")]
    public int RoomAttempts = 80;
    public Vector2Int RoomSizeMinMax = new Vector2Int(4, 12);
    [Range(0f, 1f)] public float CorridorJitterChance = 0.15f; // adds small wiggles/loops
    [Range(0f, 1f)] public float ExtraConnectionsChance = 0.10f; // additional room links

    [Header("Visualization (optional)")]
    public bool Visualize = false;
    public float WaitTime = 0.02f;

    Cell[,] grid;
    System.Random rnd;

    struct Walker { public Vector2Int pos, dir; }

    void Start() => Generate();
    public void Regenerate()
    {
        // stop any ongoing visualize coroutine from fighting you
        StopAllCoroutines();

        // clear tiles before making a new grid
        if (floorTileMap) floorTileMap.ClearAllTiles();
        if (wallTileMap) wallTileMap.ClearAllTiles();

        // (re)build
        Generate();
    }
    public void Generate()
    {
        rnd = new System.Random(Seed);
        grid = new Cell[MapWidth, MapHeight];

        switch (GenerationMode)
        {
            case Mode.Caves: GenerateCaves(); break;
            case Mode.Dungeon: GenerateDungeon(); break;
        }

        BuildWalls();
        if (FillRemainderAsWalls) FillAllEmptyToWalls();
        PushToTilemaps();
        if (Visualize) StartCoroutine(VisualizeTiles());
    }

    // ================== CAVES ==================
    void GenerateCaves()
    {
        int total = MapWidth * MapHeight;
        int target = Mathf.Max(1, Mathf.FloorToInt(FillPercentage * total));

        var walkers = new List<Walker>(MaximumWalkers);
        var center = new Vector2Int(MapWidth / 2, MapHeight / 2);
        walkers.Add(new Walker { pos = center, dir = RandDir() });
        grid[center.x, center.y] = Cell.Floor;

        int tileCount = 1;
        while (tileCount < target)
        {
            // stamp floors
            for (int i = 0; i < walkers.Count; i++)
            {
                var w = walkers[i];
                if (grid[w.pos.x, w.pos.y] != Cell.Floor)
                {
                    grid[w.pos.x, w.pos.y] = Cell.Floor;
                    if (++tileCount >= target) break;
                }
            }
            if (tileCount >= target) break;

            // mutate walkers (separate passes)
            if (walkers.Count > 1 && rnd.NextDouble() < ChangeChance)
                walkers.RemoveAt(rnd.Next(walkers.Count));

            for (int i = 0; i < walkers.Count; i++)
                if (rnd.NextDouble() < ChangeChance)
                    walkers[i] = new Walker { pos = walkers[i].pos, dir = RandDir() };

            if (walkers.Count < MaximumWalkers && rnd.NextDouble() < ChangeChance)
            {
                var src = walkers[rnd.Next(walkers.Count)];
                walkers.Add(new Walker { pos = src.pos, dir = RandDir() });
            }

            // move
            for (int i = 0; i < walkers.Count; i++)
            {
                var w = walkers[i];
                w.pos += w.dir;
                w.pos.x = Mathf.Clamp(w.pos.x, 1, MapWidth - 2);
                w.pos.y = Mathf.Clamp(w.pos.y, 1, MapHeight - 2);
                walkers[i] = w;
            }
        }
    }

    // ================== DUNGEON ==================
    void GenerateDungeon()
    {
        var rooms = new List<RectInt>(RoomAttempts);

        // place non-overlapping rooms with a 1-tile padding
        for (int i = 0; i < RoomAttempts; i++)
        {
            int rw = rnd.Next(RoomSizeMinMax.x, RoomSizeMinMax.y + 1);
            int rh = rnd.Next(RoomSizeMinMax.x, RoomSizeMinMax.y + 1);
            if (rw >= MapWidth - 4 || rh >= MapHeight - 4) continue;

            int rx = rnd.Next(2, MapWidth - rw - 2);
            int ry = rnd.Next(2, MapHeight - rh - 2);
            var r = new RectInt(rx, ry, rw, rh);

            bool overlaps = false;
            for (int k = 0; k < rooms.Count; k++)
            {
                var o = Inflate(rooms[k], 1);
                if (o.Overlaps(r)) { overlaps = true; break; }
            }
            if (overlaps) continue;

            rooms.Add(r);
            CarveRect(r);
        }

        if (rooms.Count == 0) return;

        // connect rooms: sort by x and link neighbors; add a few extra links
        rooms.Sort((a, b) => a.x.CompareTo(b.x));
        for (int i = 0; i < rooms.Count - 1; i++)
            CarveCorridor(CenterInt(rooms[i]), CenterInt(rooms[i+1]));

        // optional extra connections to reduce dead-ends
        for (int i = 0; i < rooms.Count - 1; i++)
            if (rnd.NextDouble() < ExtraConnectionsChance)
            {
                int j = Mathf.Clamp(i + 1 + rnd.Next(1, 3), 0, rooms.Count - 1);
                CarveCorridor(CenterInt(rooms[i]), CenterInt(rooms[j]));
            }

        // ensure full connectivity (cheap flood fill + connect nearest)
        EnsureConnectivity();
    }

    // ----- Dungeon helpers -----
    static RectInt Inflate(RectInt r, int m)
        => new RectInt(r.xMin - m, r.yMin - m, r.width + 2 * m, r.height + 2 * m);
    static Vector2Int CenterInt(RectInt r)
    => new Vector2Int(r.x + r.width / 2, r.y + r.height / 2);

    void FillAllEmptyToWalls()
    {
        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
                if (grid[x, y] == Cell.Empty) grid[x, y] = Cell.Wall;
    }

    void CarveRect(RectInt r)
    {
        for (int y = r.yMin; y < r.yMax; y++)
            for (int x = r.xMin; x < r.xMax; x++)
                grid[x, y] = Cell.Floor;
    }

    void CarveCorridor(Vector2Int a, Vector2Int b)
    {
        // L-shaped with optional jitter for variety
        int x = a.x, y = a.y;
        while (x != b.x)
        {
            grid[x, y] = Cell.Floor;
            x += x < b.x ? 1 : -1;
            if (rnd.NextDouble() < CorridorJitterChance && y != b.y) // little wiggle
            {
                y += y < b.y ? 1 : -1;
                grid[x, y] = Cell.Floor;
            }
        }
        while (y != b.y)
        {
            grid[x, y] = Cell.Floor;
            y += y < b.y ? 1 : -1;
        }
        grid[x, y] = Cell.Floor;
    }

    void EnsureConnectivity()
    {
        // Label components (BFS)
        int[,] comp = new int[MapWidth, MapHeight];
        var dirs = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        int compCount = 0;
        var q = new Queue<Vector2Int>();

        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
            {
                if (grid[x, y] != Cell.Floor || comp[x, y] != 0) continue;
                compCount++;
                comp[x, y] = compCount;
                q.Clear(); q.Enqueue(new Vector2Int(x, y));
                while (q.Count > 0)
                {
                    var p = q.Dequeue();
                    foreach (var d in dirs)
                    {
                        int nx = p.x + d.x, ny = p.y + d.y;
                        if (nx < 0 || ny < 0 || nx >= MapWidth || ny >= MapHeight) continue;
                        if (grid[nx, ny] != Cell.Floor || comp[nx, ny] != 0) continue;
                        comp[nx, ny] = compCount;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        if (compCount <= 1) return;

        // Collect edge cells per component (floors that touch an empty)
        var edges = new List<Vector2Int>[compCount + 1];
        for (int i = 0; i <= compCount; i++) edges[i] = new List<Vector2Int>();

        for (int y = 1; y < MapHeight - 1; y++)
            for (int x = 1; x < MapWidth - 1; x++)
            {
                if (grid[x, y] != Cell.Floor) continue;
                int c = comp[x, y];
                if (c == 0) continue;
                // touches empty?
                if (grid[x + 1, y] == Cell.Empty || grid[x - 1, y] == Cell.Empty ||
                    grid[x, y + 1] == Cell.Empty || grid[x, y - 1] == Cell.Empty)
                {
                    edges[c].Add(new Vector2Int(x, y));
                }
            }

        // Union-Find over components
        int[] parent = new int[compCount + 1];
        for (int i = 1; i <= compCount; i++) parent[i] = i;
        int Find(int a) => parent[a] == a ? a : (parent[a] = Find(parent[a]));
        void Union(int a, int b) { a = Find(a); b = Find(b); if (a != b) parent[b] = a; }

        // Repeatedly connect the two closest distinct components via their edge cells
        int groupsLeft()
        {
            var seen = new HashSet<int>();
            for (int i = 1; i <= compCount; i++) if (edges[i].Count > 0) seen.Add(Find(i));
            return seen.Count;
        }

        while (groupsLeft() > 1)
        {
            int bestA = -1, bestB = -1, bestD = int.MaxValue;
            Vector2Int pa = default, pb = default;

            // Compare edge lists only (much smaller than all floor cells)
            for (int a = 1; a <= compCount; a++)
            {
                int ra = Find(a); if (edges[a].Count == 0) continue;
                for (int b = a + 1; b <= compCount; b++)
                {
                    int rb = Find(b); if (rb == ra || edges[b].Count == 0) continue;

                    // crude early exit if components already united in later iterations
                    if (Find(a) == Find(b)) continue;

                    foreach (var ea in edges[a])
                        foreach (var eb in edges[b])
                        {
                            int dx = ea.x - eb.x, dy = ea.y - eb.y;
                            int d = dx * dx + dy * dy;
                            if (d < bestD)
                            {
                                bestD = d; bestA = a; bestB = b; pa = ea; pb = eb;
                            }
                        }
                }
            }

            if (bestA == -1) break; // nothing found (shouldn't happen)

            // Carve corridor between best pair (Manhattan carve)
            CarveCorridor(pa, pb);

            // Relabel new path quickly: flood from pb, converting any connected floors to bestA's root
            int targetComp = Find(bestA);
            q.Clear(); q.Enqueue(pb);
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (comp[p.x, p.y] == targetComp) continue;
                comp[p.x, p.y] = targetComp;
                foreach (var d in dirs)
                {
                    int nx = p.x + d.x, ny = p.y + d.y;
                    if (nx < 0 || ny < 0 || nx >= MapWidth || ny >= MapHeight) continue;
                    if (grid[nx, ny] != Cell.Floor) continue;
                    if (comp[nx, ny] == targetComp) continue;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }

            Union(bestA, bestB);

            // Update edge lists along the carved path (they may no longer be edges)
            // For simplicity, just rebuild edges for the two sets involved (cheap enough):
            edges[bestA].Clear();
            edges[bestB].Clear();
            for (int y = 1; y < MapHeight - 1; y++)
                for (int x = 1; x < MapWidth - 1; x++)
                {
                    if (grid[x, y] != Cell.Floor) continue;
                    int c = comp[x, y];
                    if (c == 0) continue;
                    if (grid[x + 1, y] == Cell.Empty || grid[x - 1, y] == Cell.Empty ||
                        grid[x, y + 1] == Cell.Empty || grid[x, y - 1] == Cell.Empty)
                    {
                        edges[c].Add(new Vector2Int(x, y));
                    }
                }
        }
    }


    // ================== Walls + Tilemaps ==================
    void BuildWalls()
    {
        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
            {
                if (grid[x, y] != Cell.Floor) continue;

                if (x + 1 < MapWidth && grid[x + 1, y] == Cell.Empty) grid[x + 1, y] = Cell.Wall;
                if (x - 1 >= 0 && grid[x - 1, y] == Cell.Empty) grid[x - 1, y] = Cell.Wall;
                if (y + 1 < MapHeight && grid[x, y + 1] == Cell.Empty) grid[x, y + 1] = Cell.Wall;
                if (y - 1 >= 0 && grid[x, y - 1] == Cell.Empty) grid[x, y - 1] = Cell.Wall;
            }
    }

    void PushToTilemaps()
    {
        var floors = new List<Vector3Int>();
        var floorTiles = new List<TileBase>();
        var walls = new List<Vector3Int>();
        var wallTiles = new List<TileBase>();

        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
            {
                var p = new Vector3Int(x, y, 0);
                switch (grid[x, y])
                {
                    case Cell.Floor: floors.Add(p); floorTiles.Add(Floor); break;
                    case Cell.Wall: walls.Add(p); wallTiles.Add(Wall); Instantiate(wallObject, new Vector3(x, 0, y), Quaternion.identity, parentTransform); break;
                        break;
                }
            }

        floorTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();
        floorTileMap.SetTiles(floors.ToArray(), floorTiles.ToArray());
        wallTileMap.SetTiles(walls.ToArray(), wallTiles.ToArray());
        floorTileMap.CompressBounds();
        wallTileMap.CompressBounds();
    }

    System.Collections.IEnumerator VisualizeTiles()
    {
        floorTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();

        // draw floors then walls over time for fun
        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
                if (grid[x, y] == Cell.Floor)
                { floorTileMap.SetTile(new Vector3Int(x, 0, y), Floor); yield return new WaitForSeconds(WaitTime); }

        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
                if (grid[x, y] == Cell.Wall)
                { wallTileMap.SetTile(new Vector3Int(x, 0, y), Wall); yield return new WaitForSeconds(WaitTime); }
    }

    // ================== Utils ==================
    Vector2Int RandDir()
    {
        switch (rnd.Next(4))
        {
            case 0: return Vector2Int.down;
            case 1: return Vector2Int.left;
            case 2: return Vector2Int.up;
            default: return Vector2Int.right;
        }
    }

    // ================== Testing ==================
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Regenerated with Seed: " + Seed);
            Regenerate();
        }
            
    }
}
