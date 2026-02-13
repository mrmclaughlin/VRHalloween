using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridMazeHedgeBuilder : MonoBehaviour
{
    [Header("References")]
    public Transform gymCenter;
    public Transform worldRoot;
    public GameObject hedgePrefab;

    [Header("Prefab orientation")]
    [Tooltip("Enable if hedge LENGTH runs along local X. Disable if along local Z.")]
    public bool hedgeLengthAlongLocalX = true;

    [Header("Geometry (meters)")]
    public float cellSize = 1.5f;
    public float footprintWidth = 24f;
    public float footprintLength = 15f;

    [Header("Maze shape")]
    public float outerMargin = 0.3f;

    [Range(0f, 0.5f)]
    public float braidFactor = 0.0f;

    [Header("Endpoints (for the FINAL exit only)")]
    public bool randomizeEntranceExit = false;

    [Tooltip("Final entrance is SOUTH edge x cell (first segment only).")]
    public int entranceX = 0;

    [Tooltip("Final exit is NORTH edge x cell (final segment only).")]
    public int exitX = 0;

    [Header("Segment chaining")]
    [Tooltip("How many times to rebuild BEFORE the final open exit. Example: 5 means 5 rebuilds, then final maze with real exit.")]
    [Min(0)]
    public int rebuildsBeforeFinalEnd = 5;

[Header("Debug: Segment Trigger Marker")]
public bool showSegmentTriggerMarker = true;

[Tooltip("Height of the debug cone marker.")]
public float triggerMarkerHeight = 0.6f;

[Tooltip("Base width of the debug cone marker.")]
public float triggerMarkerBaseRadius = 0.25f;

private GameObject triggerMarkerObj;



    [Tooltip("Place the rebuild trigger this many CELLS before the goal (keeps it inside the maze).")]
    [Min(1)]
    public int triggerCellsBeforeGoal = 3;

    [Tooltip("Try up to this many maze generations to match direction continuity.")]
    [Min(1)]
    public int maxDirectionMatchAttempts = 40;

    [Tooltip("If true, we regenerate the maze until the next segment's first step matches the previous segment direction.")]
    public bool enforceDirectionContinuity = true;

    [Header("Blocking outer openings")]
    [Tooltip("If true, only the very first entrance is open, and only the final exit is open. During segments, boundaries stay sealed.")]
    public bool blockOuterOpeningsUntilFinal = true;

    [Tooltip("If true, the entrance is open only on the first segment so players can enter from outside.")]
    public bool openEntranceOnFirstBuild = true;

    [Header("Rebuild timing")]
    public float rebuildCooldownSeconds = 1.0f;
    public float rebuildDelaySeconds = 0.05f;

    [Header("Rebuild Trigger (HMD-based)")]
    public Vector3 segmentTriggerSize = new Vector3(1.2f, 2.2f, 1.2f);

    [Tooltip("Main Camera / HMD. If null, will try Camera.main at runtime.")]
    public Transform hmd;

    [Header("Solution Path Debug (Scene Gizmos)")]
    public bool drawSolutionPath = true;
    public bool drawCellCenters = false;

    [Header("Solution Path Visual (In-Game)")]
    public GameObject solutionBallPrefab;
    public float solutionBallHeight = 0.25f;

    [Min(1)]
    public int solutionBallEveryNthCell = 2;

    public bool alwaysPlaceBallAtStartAndEnd = true;

    [Header("Solution Path Sound Triggers")]
    public bool enableSoundTriggersOnSolutionBalls = true;

    [Min(1)]
    public int soundTriggerEveryNthBall = 3;

    public List<AudioClip> pathSoundClips = new List<AudioClip>();
    public bool loopSoundClipList = false;

    [Range(0f, 1f)]
    public float soundVolume = 1f;

    public float soundTriggerRadius = 0.6f;
    public bool soundTriggerOneShot = true;
    public float soundTriggerCooldownSeconds = 1.5f;

    [Header("Old marker option (primitive spheres)")]
    public bool spawnSolutionMarkers = false;
    public float markerHeight = 0.1f;
    public GameObject oldMarker;

    [Header("Build Options")]
    public bool clearBeforeBuild = true;
    public bool buildOnStart = false;

    // Walls bitmask: 1=N, 2=E, 4=S, 8=W
    private int[,] walls;
    private bool[,] visited;

    private int cellsX, cellsY;
    private Vector3 origin; // bottom-left in world
    private List<Vector3> solutionWorldPoints = new List<Vector3>();

    // Current path in cells (for trigger placement + direction)
    private List<Vector2Int> solutionPathCells = new List<Vector2Int>();

    // Container roots
    private Transform solutionBallRoot;

    // Trigger object
    private GameObject segmentTriggerObj;

    // Endpoints locked
    private int endpointA_SouthX;
    private int endpointB_NorthX;

    // Segment state
    private int rebuildsDone = 0;
    private float nextAllowedRebuildTime = -999f;

    // Next build constraints
    private bool useForcedStartCell = false;
    private Vector2Int forcedStartCell;

    private bool useDesiredFirstStep = false;
    private Vector2Int desiredFirstStepDir; // one of (0,1),(1,0),(0,-1),(-1,0)

    void Start()
    {
        if (!Application.isPlaying) return;
        if (buildOnStart)
        {
            InitializeEndpointsIfNeeded();
            Build();
        }
    }

    void InitializeEndpointsIfNeeded()
    {
        endpointA_SouthX = entranceX;
        endpointB_NorthX = exitX;
    }



private Transform runtimeRoot;

private Transform GetOrCreateRuntimeRoot()
{
    if (runtimeRoot != null) return runtimeRoot;
    var t = worldRoot.Find("__Runtime");
    if (t != null) runtimeRoot = t;
    else
    {
        runtimeRoot = new GameObject("__Runtime").transform;
        runtimeRoot.SetParent(worldRoot, true);
    }
    return runtimeRoot;
}





[ContextMenu("Clear WorldRoot")]
public void ClearWorldRoot()
{
    if (worldRoot == null) return;

    for (int i = worldRoot.childCount - 1; i >= 0; i--)
    {
        Transform child = worldRoot.GetChild(i);
        if (child.name == "__Runtime") continue;

#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(child.gameObject);
        else Destroy(child.gameObject);
#else
        Destroy(child.gameObject);
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

        if (hmd == null && Camera.main != null)
            hmd = Camera.main.transform;

        // Compute cells
        float usableW = Mathf.Max(0.1f, footprintWidth - outerMargin * 2f);
        float usableL = Mathf.Max(0.1f, footprintLength - outerMargin * 2f);

        cellsX = Mathf.Max(2, Mathf.FloorToInt(usableW / cellSize));
        cellsY = Mathf.Max(2, Mathf.FloorToInt(usableL / cellSize));

        float mazeW = cellsX * cellSize;
        float mazeL = cellsY * cellSize;

        origin = gymCenter.position + new Vector3(-mazeW * 0.5f, 0f, -mazeL * 0.5f);

        // Randomize endpoints once
        if (randomizeEntranceExit && rebuildsDone == 0 && !useForcedStartCell)
        {
            endpointA_SouthX = Random.Range(0, cellsX);
            endpointB_NorthX = Random.Range(0, cellsX);
        }

        endpointA_SouthX = Mathf.Clamp(endpointA_SouthX, 0, cellsX - 1);
        endpointB_NorthX = Mathf.Clamp(endpointB_NorthX, 0, cellsX - 1);

        bool isFinalSegment = (rebuildsDone >= rebuildsBeforeFinalEnd);

        // Decide start / goal
        Vector2Int startCell;
        if (useForcedStartCell)
        {
            startCell = ClampCell(forcedStartCell);
        }
        else
        {
            // First segment start is entrance on SOUTH edge
            startCell = new Vector2Int(endpointA_SouthX, 0);
        }

        Vector2Int goalCell;
        if (isFinalSegment)
        {
            // Final goal is the real exit on NORTH edge
            goalCell = new Vector2Int(endpointB_NorthX, cellsY - 1);
        }
        else
        {
            // Intermediate goal: choose a far-ish boundary based on desired direction if we have one,
            // otherwise default to NORTH edge to keep flow generally forward.
            goalCell = PickIntermediateGoal(startCell);
        }

        // Generate maze with retries until direction continuity matches (if requested)
        int attempts = Mathf.Max(1, maxDirectionMatchAttempts);
        bool built = false;

        for (int a = 0; a < attempts; a++)
        {
            GeneratePerfectMaze(cellsX, cellsY);
            if (braidFactor > 0f) BraidMaze(cellsX, cellsY, braidFactor);

            // Boundary openings rules
            bool openEntrance =
                !blockOuterOpeningsUntilFinal ||
                (!useForcedStartCell && rebuildsDone == 0 && openEntranceOnFirstBuild);

            bool openExit =
                !blockOuterOpeningsUntilFinal ||
                isFinalSegment;

            CarveSouthNorthOpenings(endpointA_SouthX, endpointB_NorthX, openEntrance, openExit);

            // Compute solution
            if (!ComputeSolutionPathCells(startCell, goalCell))
                continue;

            if (enforceDirectionContinuity && useDesiredFirstStep)
            {
                if (!DoesFirstStepMatchDesired(startCell))
                    continue;
            }

            built = true;
            break;
        }

        if (!built)
        {
            Debug.LogWarning("Failed to build a direction-matching maze within attempts. Building last attempt anyway.");
            // Build at least something (one more time)
            GeneratePerfectMaze(cellsX, cellsY);
            if (braidFactor > 0f) BraidMaze(cellsX, cellsY, braidFactor);

            bool openEntrance =
                !blockOuterOpeningsUntilFinal ||
                (!useForcedStartCell && rebuildsDone == 0 && openEntranceOnFirstBuild);

            bool openExit =
                !blockOuterOpeningsUntilFinal ||
                isFinalSegment;

            CarveSouthNorthOpenings(endpointA_SouthX, endpointB_NorthX, openEntrance, openExit);

            ComputeSolutionPathCells(startCell, goalCell);
        }

        // Build walls
        BuildWallsAsHedges(cellsX, cellsY);

        // Convert solution cells -> world points
        BuildSolutionWorldPointsFromCells();

        // Balls + sounds
        SpawnSolutionBallsIfNeeded();

        // Optional markers
        if (spawnSolutionMarkers) SpawnSolutionMarkers();

        // Place trigger:
        // - If final segment: trigger is at the GOAL cell (acts like “real exit reached”)
        // - Else: trigger is placed triggerCellsBeforeGoal cells before goal (interior)
        PlaceSegmentTrigger(isFinalSegment);

        // After a successful Build, consume one-time constraints:
        // Start cell is now “where you are” for next segment, so we always force start after first trigger.
        // Desired direction is set when trigger is hit.
        useDesiredFirstStep = false;

        Debug.Log($"Maze built. segment={rebuildsDone}/{rebuildsBeforeFinalEnd} final={isFinalSegment} start={startCell} goal={goalCell} pathLen={solutionPathCells.Count}");
    }

    // Called by trigger when player reaches the segment trigger
    public void NotifyReachedSegmentTrigger()
    {
        if (!Application.isPlaying) return;

        if (Time.time < nextAllowedRebuildTime) return;
        nextAllowedRebuildTime = Time.time + rebuildCooldownSeconds;

        bool isFinalSegment = (rebuildsDone >= rebuildsBeforeFinalEnd);
        if (isFinalSegment)
        {
            Debug.Log("FINAL END reached (no rebuild).");
            if (segmentTriggerObj != null) segmentTriggerObj.SetActive(false);
            return;
        }

        // Capture: start cell for next segment = current trigger cell
        Vector2Int triggerCell = GetCurrentTriggerCell();
        forcedStartCell = triggerCell;
        useForcedStartCell = true;

        // Capture: desired first step direction = direction of the solution path AT the trigger
        if (enforceDirectionContinuity)
        {
            Vector2Int dir = GetDirectionAtTriggerCell(triggerCell);
            if (dir != Vector2Int.zero)
            {
                desiredFirstStepDir = dir;
                useDesiredFirstStep = true;
            }
        }

        rebuildsDone++;

        StartCoroutine(RebuildAfterDelay());
    }

    private IEnumerator RebuildAfterDelay()
    {
        if (rebuildDelaySeconds > 0f)
            yield return new WaitForSeconds(rebuildDelaySeconds);

        Build();
    }

    // ---------------- Goal picking ----------------
    Vector2Int PickIntermediateGoal(Vector2Int startCell)
    {
        // If we have a desired direction, pick a boundary in that direction.
        // Otherwise default to NORTH edge.
        Vector2Int dir = desiredFirstStepDir;
        if (!useDesiredFirstStep) dir = Vector2Int.up;

        // Prefer far boundaries to keep “length” feeling
        if (dir == Vector2Int.up)
        {
            int x = Random.Range(0, cellsX);
            return new Vector2Int(x, cellsY - 1);
        }
        if (dir == Vector2Int.down)
        {
            int x = Random.Range(0, cellsX);
            return new Vector2Int(x, 0);
        }
        if (dir == Vector2Int.right)
        {
            int y = Random.Range(0, cellsY);
            return new Vector2Int(cellsX - 1, y);
        }
        // left
        {
            int y = Random.Range(0, cellsY);
            return new Vector2Int(0, y);
        }
    }

    // ---------------- Maze generation ----------------
    void GeneratePerfectMaze(int w, int h)
    {
        walls = new int[w, h];
        visited = new bool[w, h];

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

        if (dx == 1) { walls[a.x, a.y] &= ~2; walls[b.x, b.y] &= ~8; }
        else if (dx == -1) { walls[a.x, a.y] &= ~8; walls[b.x, b.y] &= ~2; }
        else if (dy == 1) { walls[a.x, a.y] &= ~1; walls[b.x, b.y] &= ~4; }
        else if (dy == -1) { walls[a.x, a.y] &= ~4; walls[b.x, b.y] &= ~1; }
    }

    void CarveSouthNorthOpenings(int entranceXCell, int exitXCell, bool openEntrance, bool openExit)
    {
        for (int x = 0; x < cellsX; x++)
        {
            walls[x, 0] |= 4;
            walls[x, cellsY - 1] |= 1;
        }
        for (int y = 0; y < cellsY; y++)
        {
            walls[0, y] |= 8;
            walls[cellsX - 1, y] |= 2;
        }

        entranceXCell = Mathf.Clamp(entranceXCell, 0, cellsX - 1);
        exitXCell = Mathf.Clamp(exitXCell, 0, cellsX - 1);

        if (openEntrance) walls[entranceXCell, 0] &= ~4;
        if (openExit) walls[exitXCell, cellsY - 1] &= ~1;
    }

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

    // ---------------- Build walls as hedges ----------------
    void BuildWallsAsHedges(int w, int h)
    {
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int m = walls[x, y];

            Vector3 cellCenter = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
            float half = cellSize * 0.5f;

            if ((m & 1) != 0) SpawnWall(cellCenter + new Vector3(0f, 0f, half), Quaternion.identity);
            if ((m & 2) != 0) SpawnWall(cellCenter + new Vector3(half, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            if (y == 0 && (m & 4) != 0) SpawnWall(cellCenter + new Vector3(0f, 0f, -half), Quaternion.identity);
            if (x == 0 && (m & 8) != 0) SpawnWall(cellCenter + new Vector3(-half, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
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

    // ---------------- Solution path cells + world points ----------------
    bool ComputeSolutionPathCells(Vector2Int start, Vector2Int goal)
    {
        solutionPathCells.Clear();
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

        if (!found) return false;

        var p = goal;
        solutionPathCells.Add(p);
        while (p != start)
        {
            p = parent[p];
            solutionPathCells.Add(p);
        }
        solutionPathCells.Reverse();
        return (solutionPathCells.Count >= 2);
    }

    void BuildSolutionWorldPointsFromCells()
    {
        solutionWorldPoints.Clear();
        foreach (var c in solutionPathCells)
            solutionWorldPoints.Add(CellCenterWorld(c.x, c.y));
    }

    IEnumerable<Vector2Int> OpenNeighbors(Vector2Int c)
    {
        int x = c.x;
        int y = c.y;
        int m = walls[x, y];

        if ((m & 1) == 0 && y + 1 < cellsY) yield return new Vector2Int(x, y + 1);
        if ((m & 2) == 0 && x + 1 < cellsX) yield return new Vector2Int(x + 1, y);
        if ((m & 4) == 0 && y - 1 >= 0) yield return new Vector2Int(x, y - 1);
        if ((m & 8) == 0 && x - 1 >= 0) yield return new Vector2Int(x - 1, y);
    }

    Vector3 CellCenterWorld(int x, int y)
    {
        return origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
    }

    // ---------------- Direction continuity helpers ----------------
    bool DoesFirstStepMatchDesired(Vector2Int startCell)
    {
        if (solutionPathCells.Count < 2) return false;
        if (solutionPathCells[0] != startCell) return false;

        Vector2Int step = solutionPathCells[1] - solutionPathCells[0];
        return step == desiredFirstStepDir;
    }

    Vector2Int GetDirectionAtTriggerCell(Vector2Int triggerCell)
    {
        // Find triggerCell in path and return next step direction if possible
        for (int i = 0; i < solutionPathCells.Count - 1; i++)
        {
            if (solutionPathCells[i] == triggerCell)
                return solutionPathCells[i + 1] - solutionPathCells[i];
        }
        return Vector2Int.zero;
    }

    Vector2Int GetCurrentTriggerCell()
    {
        if (segmentTriggerObj == null) return forcedStartCell;
        // We store it on the trigger component too (most reliable)
        var t = segmentTriggerObj.GetComponent<MazeEndTrigger>();
        if (t != null) return t.triggerCell;
        return forcedStartCell;
    }

    Vector2Int ClampCell(Vector2Int c)
    {
        return new Vector2Int(Mathf.Clamp(c.x, 0, cellsX - 1), Mathf.Clamp(c.y, 0, cellsY - 1));
    }

   void PlaceSegmentTrigger(bool isFinalSegment)
{
    if (solutionPathCells == null || solutionPathCells.Count < 2) return;

    int idx;
    if (isFinalSegment)
        idx = solutionPathCells.Count - 1; // at goal
    else
        idx = Mathf.Clamp(solutionPathCells.Count - 1 - triggerCellsBeforeGoal, 1, solutionPathCells.Count - 2);

    Vector2Int triggerCell = solutionPathCells[idx];

    // Put the trigger at the cell center, raised so the box is centered at head height
    Vector3 triggerPos = CellCenterWorld(triggerCell.x, triggerCell.y) + Vector3.up * (segmentTriggerSize.y * 0.5f);

    Transform rt = GetOrCreateRuntimeRoot();

    // --- Create/Update Trigger ---
    if (segmentTriggerObj == null)
    {
        segmentTriggerObj = new GameObject("MazeSegmentTrigger");
        segmentTriggerObj.transform.SetParent(rt, true);

        var bc = segmentTriggerObj.AddComponent<BoxCollider>();
        bc.isTrigger = true;

        var trig = segmentTriggerObj.AddComponent<MazeEndTrigger>();
        trig.builder = this;
    }

    var box = segmentTriggerObj.GetComponent<BoxCollider>();
    box.size = segmentTriggerSize;

    var t = segmentTriggerObj.GetComponent<MazeEndTrigger>();
    t.hmd = hmd;
    t.cooldownSeconds = rebuildCooldownSeconds;
    t.triggerCell = triggerCell;
    t.ForceOutside(); // SUPER IMPORTANT after each move

    segmentTriggerObj.transform.position = triggerPos;
    segmentTriggerObj.SetActive(true);

    // --- Create/Update Debug Cone Marker ---
    if (showSegmentTriggerMarker)
    {
        if (triggerMarkerObj == null)
        {
            // Unity primitive "cone" doesn't exist, so we fake it:
            // Use a Cylinder scaled to look like a cone-ish spike.
            triggerMarkerObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            triggerMarkerObj.name = "SegmentTriggerMarker_CONE";
            triggerMarkerObj.transform.SetParent(rt, true);

            // Remove collider so it doesn't interfere with anything
            var c = triggerMarkerObj.GetComponent<Collider>();
            if (c) Destroy(c);
        }

        // Place it at floor with tip pointing up:
        // Cylinder pivot is center, so we lift by half-height.
        float h = Mathf.Max(0.1f, triggerMarkerHeight);
        float r = Mathf.Max(0.05f, triggerMarkerBaseRadius);

        triggerMarkerObj.transform.position = new Vector3(triggerPos.x, origin.y + (h * 0.5f), triggerPos.z);

        // Fake "cone": tiny top radius by flattening X/Z heavily and leaving Y tall doesn't actually taper.
        // Best cheap illusion is: make it a thin tall spike. You’ll still see it clearly.
        triggerMarkerObj.transform.localScale = new Vector3(r * 2f, h * 0.5f, r * 2f);

        triggerMarkerObj.SetActive(true);
    }
    else
    {
        if (triggerMarkerObj != null) triggerMarkerObj.SetActive(false);
    }

    Debug.Log($"SegmentTrigger placed at cell {triggerCell} (idx {idx}/{solutionPathCells.Count - 1}) pos={triggerPos}");
}

    // ---------------- In-game solution balls ----------------
    void SpawnSolutionBallsIfNeeded()
    {
        if (solutionBallPrefab == null) return;
        if (solutionWorldPoints == null || solutionWorldPoints.Count == 0) return;

        solutionBallRoot = new GameObject("SolutionBalls").transform;
        solutionBallRoot.SetParent(worldRoot, true);

        if (hmd == null && Camera.main != null)
            hmd = Camera.main.transform;

        int n = Mathf.Max(1, solutionBallEveryNthCell);

        int placedBallCount = 0;
        int soundClipIndex = 0;

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

            if (enableSoundTriggersOnSolutionBalls &&
                soundTriggerEveryNthBall > 0 &&
                (placedBallCount % soundTriggerEveryNthBall == 0))
            {
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

                    if (chosen != null) AddSoundTriggerToOrb(orb, chosen);
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
    a.clip = clip;                 // ✅ make sure a clip is assigned
    a.volume = soundVolume;        // ✅ make sure volume is assigned
    a.rolloffMode = AudioRolloffMode.Logarithmic;
    a.minDistance = 0.25f;
    a.maxDistance = 12f;

    // Ensure we have an HMD reference every build
    if (hmd == null && Camera.main != null)
        hmd = Camera.main.transform;

    // HMD-based trigger script
    HmdSoundZone hmdZone = zoneObj.AddComponent<HmdSoundZone>();
    hmdZone.Init(hmd, clip, soundVolume, soundTriggerOneShot, soundTriggerCooldownSeconds);
}


    // ---------------- Old primitive markers (optional) ----------------
    void SpawnSolutionMarkers()
    {
        for (int i = 0; i < solutionWorldPoints.Count; i++)
        {
            GameObject s = Instantiate(oldMarker);
            s.name = $"SolutionMarker_{i}";
            s.transform.SetParent(worldRoot, true);
            s.transform.position = solutionWorldPoints[i] + Vector3.up * markerHeight;
            s.transform.localScale = Vector3.one * 0.15f;

            var col = s.GetComponent<Collider>();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (col) DestroyImmediate(col);
            }
            else
            {
                if (col) Destroy(col);
            }
#else
            if (col) Destroy(col);
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
