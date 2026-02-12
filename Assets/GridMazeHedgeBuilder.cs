using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridMazeHedgeBuilder : MonoBehaviour
{
    [Header("References")]
    public Transform gymCenter;      // Center of the play area
    public Transform worldRoot;      // Parent for spawned hedges
    public GameObject hedgePrefab;   // 1.5m long hedge segment (pivot centered)

    [Header("Prefab orientation")]
    [Tooltip("Enable if hedge LENGTH runs along local X. Disable if along local Z.")]
    public bool hedgeLengthAlongLocalX = true;

    [Header("Geometry (meters)")]
    public float cellSize = 1.5f;          // IMPORTANT: use 1.5 for your 1m corridor result
    public float footprintWidth = 24f;
    public float footprintLength = 15f;

    [Header("Maze shape")]
    [Tooltip("Extra empty margin around the maze footprint (meters).")]
    public float outerMargin = 0.3f;

    [Tooltip("Remove some dead ends to feel more 'wandering' than 'puzzle'. 0 = perfect maze (one unique solution).")]
    [Range(0f, 0.5f)]
    public float braidFactor = 0.0f; // NOTE: keep at 0 for 'only one solution' feel.

    [Header("Entrance / Exit")]
    public bool randomizeEntranceExit = false;

    [Tooltip("Entrance is on the SOUTH edge of this cell (x, y=0).")]
    public int entranceX = 0;

    [Tooltip("Exit is on the NORTH edge of this cell (x, y=max).")]
    public int exitX = 0;

    [Header("Solution Path Debug (Scene Gizmos)")]
    public bool drawSolutionPath = true;
    public bool drawCellCenters = false;

    [Header("Solution Path Visual (In-Game)")]
    [Tooltip("Prefab to place along the solution path (e.g., a glowing orb). Leave null to disable.")]
    public GameObject solutionBallPrefab;

    [Tooltip("How high above the floor to place the solution balls.")]
    public float solutionBallHeight = 0.25f;

    [Tooltip("Place a ball every Nth cell along the solution path. 1 = every cell, 2 = every other cell, etc.")]
    [Min(1)]
    public int solutionBallEveryNthCell = 2;

    [Tooltip("Optional: also place balls on start and end regardless of N.")]
    public bool alwaysPlaceBallAtStartAndEnd = true;

    [Header("Solution Path Sound Triggers")]
    public bool enableSoundTriggersOnSolutionBalls = true;

    [Tooltip("Play a sound trigger every Nth *placed* solution ball.")]
    [Min(1)]
    public int soundTriggerEveryNthBall = 3;

    [Tooltip("Sound clips assigned in order to each trigger ball.")]
    public List<AudioClip> pathSoundClips = new List<AudioClip>();

    [Tooltip("If true, after the last clip it wraps back to the first.")]
    public bool loopSoundClipList = false;

    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Tooltip("Radius (meters) of the trigger zone around the ball.")]
    public float soundTriggerRadius = 0.6f;

    [Tooltip("If true, the trigger plays only once per orb.")]
    public bool soundTriggerOneShot = true;

    [Tooltip("Cooldown (seconds) to prevent rapid re-trigger near edges.")]
    public float soundTriggerCooldownSeconds = 1.5f;

    [Tooltip("Main Camera / HMD. If null, will try Camera.main at runtime.")]
    public Transform hmd;

    [Header("Old marker option (primitive spheres)")]
    [Tooltip("Optional: drop small spheres along the solution path for debugging.")]
    public bool spawnSolutionMarkers = false;
    public float markerHeight = 0.1f;

    [Header("Build Options")]
    public bool clearBeforeBuild = true;
    public bool buildOnStart = false;

    // Walls bitmask: 1=N, 2=E, 4=S, 8=W. Bit set => wall present.
    private int[,] walls;
    private bool[,] visited;

    private int cellsX, cellsY;
    private Vector3 origin; // bottom-left corner of maze (world space)
    private List<Vector3> solutionWorldPoints = new List<Vector3>();

    // Container for solution balls so we can easily clear them
    private Transform solutionBallRoot;

    void Start()
    {
        if (!Application.isPlaying) return;
        if (buildOnStart) Build();
    }

    [ContextMenu("Clear WorldRoot")]
    public void ClearWorldRoot()
    {
        if (worldRoot == null) return;

        for (int i = worldRoot.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(worldRoot.GetChild(i).gameObject);
            else Destroy(worldRoot.GetChild(i).gameObject);
#else
            Destroy(worldRoot.GetChild(i).gameObject);
#endif
        }
    }

    [ContextMenu("Build Grid Maze (Hedges)")]
    public void Build()
    {
        if (gymCenter == null || worldRoot == null || hedgePrefab == null)
        {
            Debug.LogError("GridMazeHedgeBuilder: Assign gymCenter, worldRoot, hedgePrefab.");
            return;
        }

        if (clearBeforeBuild) ClearWorldRoot();

        // Compute how many cells fit
        float usableW = Mathf.Max(0.1f, footprintWidth - outerMargin * 2f);
        float usableL = Mathf.Max(0.1f, footprintLength - outerMargin * 2f);

        cellsX = Mathf.Max(2, Mathf.FloorToInt(usableW / cellSize));
        cellsY = Mathf.Max(2, Mathf.FloorToInt(usableL / cellSize));

        // Center the maze within the footprint
        float mazeW = cellsX * cellSize;
        float mazeL = cellsY * cellSize;

        origin = gymCenter.position + new Vector3(-mazeW * 0.5f, 0f, -mazeL * 0.5f);

        GeneratePerfectMaze(cellsX, cellsY);

        // IMPORTANT: If you want only one solution, keep braidFactor = 0
        if (braidFactor > 0f) BraidMaze(cellsX, cellsY, braidFactor);

        // Choose entrance/exit
        int ex, xx;
        if (randomizeEntranceExit)
        {
            ex = Random.Range(0, cellsX);
            xx = Random.Range(0, cellsX);
        }
        else
        {
            ex = Mathf.Clamp(entranceX, 0, cellsX - 1);
            xx = Mathf.Clamp(exitX, 0, cellsX - 1);
        }

        // Ensure exactly one entrance and one exit by carving boundary openings:
        CarveEntranceExit(ex, xx);

        // Build hedges for all walls
        BuildWallsAsHedges(cellsX, cellsY, origin);

        // Compute solution path from entrance cell to exit cell
        ComputeSolutionPath(new Vector2Int(ex, 0), new Vector2Int(xx, cellsY - 1));

        // In-game visual solution balls (prefab)
        SpawnSolutionBallsIfNeeded();

        // Optional: old debug spheres
        if (spawnSolutionMarkers) SpawnSolutionMarkers();

        Debug.Log($"Grid maze built: {cellsX} x {cellsY} cells. EntranceX={ex}, ExitX={xx}");
    }

    // ---------------- Maze generation ----------------
    void GeneratePerfectMaze(int w, int h)
    {
        walls = new int[w, h];
        visited = new bool[w, h];

        // start with all walls present
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            walls[x, y] = 1 | 2 | 4 | 8;

        var stack = new Stack<Vector2Int>();
        Vector2Int start = new Vector2Int(Random.Range(0, w), Random.Range(0, h));
        visited[start.x, start.y] = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            var cur = stack.Peek();
            var neighbors = UnvisitedNeighbors(cur, w, h);

            if (neighbors.Count == 0)
            {
                stack.Pop();
                continue;
            }

            var next = neighbors[Random.Range(0, neighbors.Count)];
            RemoveWallBetween(cur, next);
            visited[next.x, next.y] = true;
            stack.Push(next);
        }
    }

    List<Vector2Int> UnvisitedNeighbors(Vector2Int c, int w, int h)
    {
        var list = new List<Vector2Int>(4);

        TryAdd(c.x, c.y + 1);
        TryAdd(c.x + 1, c.y);
        TryAdd(c.x, c.y - 1);
        TryAdd(c.x - 1, c.y);

        return list;

        void TryAdd(int x, int y)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            if (!visited[x, y]) list.Add(new Vector2Int(x, y));
        }
    }

    void RemoveWallBetween(Vector2Int a, Vector2Int b)
    {
        int dx = b.x - a.x;
        int dy = b.y - a.y;

        if (dx == 1) { walls[a.x, a.y] &= ~2; walls[b.x, b.y] &= ~8; }        // a east open
        else if (dx == -1) { walls[a.x, a.y] &= ~8; walls[b.x, b.y] &= ~2; }  // a west open
        else if (dy == 1) { walls[a.x, a.y] &= ~1; walls[b.x, b.y] &= ~4; }   // a north open
        else if (dy == -1) { walls[a.x, a.y] &= ~4; walls[b.x, b.y] &= ~1; }  // a south open
    }

    // ---------------- Ensure only one entrance and one exit ----------------
    void CarveEntranceExit(int entranceXCell, int exitXCell)
    {
        for (int x = 0; x < cellsX; x++)
        {
            walls[x, 0] |= 4;                 // bottom row must have South walls
            walls[x, cellsY - 1] |= 1;        // top row must have North walls
        }
        for (int y = 0; y < cellsY; y++)
        {
            walls[0, y] |= 8;                 // left col must have West walls
            walls[cellsX - 1, y] |= 2;        // right col must have East walls
        }

        walls[entranceXCell, 0] &= ~4;            // remove South wall bit
        walls[exitXCell, cellsY - 1] &= ~1;       // remove North wall bit
    }

    // ---------------- Optional braiding (creates loops; can create multiple solutions) ----------------
    void BraidMaze(int w, int h, float factor)
    {
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int openings = 0;
            int m = walls[x, y];
            if ((m & 1) == 0) openings++;
            if ((m & 2) == 0) openings++;
            if ((m & 4) == 0) openings++;
            if ((m & 8) == 0) openings++;

            if (openings == 1 && Random.value < factor)
            {
                var candidates = new List<Vector2Int>();

                if (y + 1 < h && (m & 1) != 0) candidates.Add(new Vector2Int(x, y + 1));
                if (x + 1 < w && (m & 2) != 0) candidates.Add(new Vector2Int(x + 1, y));
                if (y - 1 >= 0 && (m & 4) != 0) candidates.Add(new Vector2Int(x, y - 1));
                if (x - 1 >= 0 && (m & 8) != 0) candidates.Add(new Vector2Int(x - 1, y));

                if (candidates.Count > 0)
                {
                    var n = candidates[Random.Range(0, candidates.Count)];
                    RemoveWallBetween(new Vector2Int(x, y), n);
                }
            }
        }
    }

    // ---------------- Build walls as hedges (no overlaps) ----------------
    void BuildWallsAsHedges(int w, int h, Vector3 originWorld)
    {
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int m = walls[x, y];

            Vector3 cellCenter = originWorld + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
            float half = cellSize * 0.5f;

            if ((m & 1) != 0) SpawnWall(cellCenter + new Vector3(0f, 0f, half), Quaternion.identity);                 // N
            if ((m & 2) != 0) SpawnWall(cellCenter + new Vector3(half, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));       // E
            if (y == 0 && (m & 4) != 0) SpawnWall(cellCenter + new Vector3(0f, 0f, -half), Quaternion.identity);      // S (outer)
            if (x == 0 && (m & 8) != 0) SpawnWall(cellCenter + new Vector3(-half, 0f, 0f), Quaternion.Euler(0f, 90f, 0f)); // W (outer)
        }
    }

    void SpawnWall(Vector3 pos, Quaternion rot)
    {
        if (hedgeLengthAlongLocalX)
            rot *= Quaternion.Euler(0f, 90f, 0f);

#if UNITY_EDITOR
        GameObject h = (!Application.isPlaying)
            ? (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(hedgePrefab, worldRoot)
            : Instantiate(hedgePrefab, worldRoot);
#else
        GameObject h = Instantiate(hedgePrefab, worldRoot);
#endif
        h.transform.SetPositionAndRotation(pos, rot);
    }

    // ---------------- Solution path (unique) ----------------
    void ComputeSolutionPath(Vector2Int start, Vector2Int goal)
    {
        solutionWorldPoints.Clear();

        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var q = new Queue<Vector2Int>();
        var seen = new HashSet<Vector2Int>();

        q.Enqueue(start);
        seen.Add(start);

        bool found = false;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == goal) { found = true; break; }

            foreach (var n in OpenNeighbors(cur))
            {
                if (seen.Contains(n)) continue;
                seen.Add(n);
                parent[n] = cur;
                q.Enqueue(n);
            }
        }

        if (!found)
        {
            Debug.LogWarning("No solution path found (unexpected for a perfect maze).");
            return;
        }

        var pathCells = new List<Vector2Int>();
        var p = goal;
        pathCells.Add(p);
        while (p != start)
        {
            p = parent[p];
            pathCells.Add(p);
        }
        pathCells.Reverse();

        foreach (var c in pathCells)
            solutionWorldPoints.Add(CellCenterWorld(c.x, c.y));
    }

    IEnumerable<Vector2Int> OpenNeighbors(Vector2Int c)
    {
        int x = c.x;
        int y = c.y;
        int m = walls[x, y];

        if ((m & 1) == 0 && y + 1 < cellsY) yield return new Vector2Int(x, y + 1); // N
        if ((m & 2) == 0 && x + 1 < cellsX) yield return new Vector2Int(x + 1, y); // E
        if ((m & 4) == 0 && y - 1 >= 0) yield return new Vector2Int(x, y - 1);     // S
        if ((m & 8) == 0 && x - 1 >= 0) yield return new Vector2Int(x - 1, y);     // W
    }

    Vector3 CellCenterWorld(int x, int y)
    {
        return origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
    }

    // ---------------- In-game solution balls ----------------
    void SpawnSolutionBallsIfNeeded()
    {
        if (solutionBallPrefab == null) return;
        if (solutionWorldPoints == null || solutionWorldPoints.Count == 0) return;

        // Create a container so they’re easy to find/clean
        solutionBallRoot = new GameObject("SolutionBalls").transform;
        solutionBallRoot.SetParent(worldRoot, true);

        // Resolve HMD reference (runtime)
        if (hmd == null && Camera.main != null)
            hmd = Camera.main.transform;

        int n = Mathf.Max(1, solutionBallEveryNthCell);

        int placedBallCount = 0;  // counts only balls actually spawned
        int soundClipIndex = 0;   // steps through your sound list in order

        for (int i = 0; i < solutionWorldPoints.Count; i++)
        {
            bool place =
                (i % n == 0) ||
                (alwaysPlaceBallAtStartAndEnd && (i == 0 || i == solutionWorldPoints.Count - 1));

            if (!place) continue;

            Vector3 pos = solutionWorldPoints[i] + Vector3.up * solutionBallHeight;

#if UNITY_EDITOR
            GameObject orb = (!Application.isPlaying)
                ? (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(solutionBallPrefab, solutionBallRoot)
                : Instantiate(solutionBallPrefab, solutionBallRoot);
#else
            GameObject orb = Instantiate(solutionBallPrefab, solutionBallRoot);
#endif
            orb.transform.position = pos;

            placedBallCount++;

            // Every Nth *placed* ball gets a sound trigger
            if (enableSoundTriggersOnSolutionBalls &&
                soundTriggerEveryNthBall > 0 &&
                (placedBallCount % soundTriggerEveryNthBall == 0))
            {
                // Do we have a clip to assign?
                if (pathSoundClips != null && pathSoundClips.Count > 0)
                {
                    AudioClip chosen = null;

                    if (soundClipIndex < pathSoundClips.Count)
                    {
                        chosen = pathSoundClips[soundClipIndex];
                        soundClipIndex++;
                    }
                    else if (loopSoundClipList)
                    {
                        chosen = pathSoundClips[soundClipIndex % pathSoundClips.Count];
                        soundClipIndex++;
                    }

                    if (chosen != null)
                    {
                        AddSoundTriggerToOrb(orb, chosen);
                    }
                }
            }
        }
    }

    void AddSoundTriggerToOrb(GameObject orb, AudioClip clip)
    {
        // Create a child trigger zone
        GameObject zoneObj = new GameObject("OrbSoundZone");
        zoneObj.transform.SetParent(orb.transform, false);
        zoneObj.transform.localPosition = Vector3.zero;

        // Sphere trigger
        SphereCollider sc = zoneObj.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = soundTriggerRadius;

        // Audio source on the zone (3D sound centered on orb)
        AudioSource a = zoneObj.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.spatialBlend = 1f;

        // HMD-based trigger script
        HmdSoundZone hmdZone = zoneObj.AddComponent<HmdSoundZone>();
        hmdZone.Init(hmd, clip, soundVolume, soundTriggerOneShot, soundTriggerCooldownSeconds);
    }

    // ---------------- Old primitive markers (optional) ----------------
    void SpawnSolutionMarkers()
    {
        for (int i = 0; i < solutionWorldPoints.Count; i++)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = $"SolutionMarker_{i}";
            s.transform.SetParent(worldRoot, true);
            s.transform.position = solutionWorldPoints[i] + Vector3.up * markerHeight;
            s.transform.localScale = Vector3.one * 0.15f;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var col = s.GetComponent<Collider>();
                if (col) DestroyImmediate(col);
            }
            else
            {
                var col = s.GetComponent<Collider>();
                if (col) Destroy(col);
            }
#else
            var col2 = s.GetComponent<Collider>();
            if (col2) Destroy(col2);
#endif
        }
    }

    // ---------------- Debug drawing ----------------
    void OnDrawGizmosSelected()
    {
        if (!drawSolutionPath) return;
        if (solutionWorldPoints == null || solutionWorldPoints.Count < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < solutionWorldPoints.Count - 1; i++)
        {
            Vector3 a = solutionWorldPoints[i] + Vector3.up * 0.05f;
            Vector3 b = solutionWorldPoints[i + 1] + Vector3.up * 0.05f;
            Gizmos.DrawLine(a, b);
        }

        if (drawCellCenters)
        {
            Gizmos.color = Color.yellow;
            foreach (var p in solutionWorldPoints)
                Gizmos.DrawSphere(p + Vector3.up * 0.05f, 0.08f);
        }
    }
}
