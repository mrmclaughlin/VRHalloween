using UnityEngine;

public class PokeIndentVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cap;           // the visible moving part (child)
    [SerializeField] Transform pokePoint;     // fingertip / poke attach transform

    [Header("Press Axis")]
    [Tooltip("Local direction the cap moves when pressed (e.g., (0,0,-1) for -Z).")]
    [SerializeField] Vector3 localPressAxis = new Vector3(0f, 0f, -1f);

    [Header("Depth -> Travel")]
    [Tooltip("How much fingertip penetration corresponds to a full press (meters).")]
    [SerializeField] float maxPokeDepth = 0.02f;   // 2 cm
    [Tooltip("How far the cap moves at full press (meters).")]
    [SerializeField] float capTravel = 0.006f;     // 6 mm

    [Header("Smoothing")]
    [SerializeField] float returnSpeed = 25f;
[SerializeField] Transform surface; // add this
    Vector3 capStartLocalPos;
    float current01; // 0..1 press amount

    void Awake()
    {
        if (cap != null) capStartLocalPos = cap.localPosition;
        localPressAxis = localPressAxis.normalized;
    }

    void Update()
    {
      

    if (cap == null || pokePoint == null || surface == null) return;

    Vector3 localPress = localPressAxis.normalized;

    // fingertip position in surface local space
    Vector3 localFinger = surface.InverseTransformPoint(pokePoint.position);

    // depth into button along press axis (pressing inward means positive depth)
    float depth = Vector3.Dot(localFinger, -localPress);

    float target01 = Mathf.Clamp01(depth / Mathf.Max(0.0001f, maxPokeDepth));
    current01 = Mathf.MoveTowards(current01, target01, Time.deltaTime * returnSpeed);

    cap.localPosition = capStartLocalPos + (localPress * (capTravel * current01));

        
    }

    // Optional: call this if you disable the button or reload UI, etc.
    public void ResetVisual()
    {
        current01 = 0f;
        if (cap != null) cap.localPosition = capStartLocalPos;
    }
}
