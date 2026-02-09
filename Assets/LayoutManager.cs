using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutManager : MonoBehaviour
{
    public static LayoutManager Instance { get; private set; }

    [Header("Layouts (roots)")]
    [Tooltip("Add ALL layout root GameObjects here.")]
    [SerializeField] private List<GameObject> layouts = new List<GameObject>();

    [Header("Start Layout")]
    [SerializeField] private int startLayoutIndex = 0;

    [Header("Safety")]
    [SerializeField] private float switchCooldownSeconds = 0.35f;

    private int currentIndex = -1;
    private bool switching;
    private float nextAllowedTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Ensure only one layout is active at start (prevents hidden tug-of-war)
        SetActiveLayout(startLayoutIndex, immediate: true);
    }

    public void RequestSwitchTo(GameObject targetLayoutRoot)
    {
        if (targetLayoutRoot == null) return;

        int idx = layouts.IndexOf(targetLayoutRoot);
        if (idx < 0)
        {
            Debug.LogWarning($"LayoutManager: Target layout not found in list: {targetLayoutRoot.name}");
            return;
        }

        RequestSwitchToIndex(idx);
    }

    public void RequestSwitchToIndex(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= layouts.Count) return;
        if (switching) return;
        if (Time.time < nextAllowedTime) return;
        if (targetIndex == currentIndex) return;

        StartCoroutine(SwitchRoutine(targetIndex));
    }

    private IEnumerator SwitchRoutine(int targetIndex)
    {
        switching = true;
        nextAllowedTime = Time.time + switchCooldownSeconds;

        // Turn off all layouts except target (keeps things deterministic)
        for (int i = 0; i < layouts.Count; i++)
        {
            if (layouts[i] == null) continue;
            bool shouldBeOn = (i == targetIndex);
            if (layouts[i].activeSelf != shouldBeOn)
                layouts[i].SetActive(shouldBeOn);
        }

        currentIndex = targetIndex;

        // Let OnEnable/Start settle for one frame
        yield return null;

        switching = false;
    }

    private void SetActiveLayout(int idx, bool immediate)
    {
        if (idx < 0 || idx >= layouts.Count) idx = 0;

        for (int i = 0; i < layouts.Count; i++)
        {
            if (layouts[i] == null) continue;
            bool shouldBeOn = (i == idx);
            layouts[i].SetActive(shouldBeOn);
        }
        currentIndex = idx;
        nextAllowedTime = Time.time + (immediate ? 0.05f : switchCooldownSeconds);
    }

    public int GetCurrentIndex() => currentIndex;
    public GameObject GetCurrentLayout() => (currentIndex >= 0 && currentIndex < layouts.Count) ? layouts[currentIndex] : null;
}
