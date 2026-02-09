using UnityEngine;
using UnityEngine.UI;

public class HeadClipFader : MonoBehaviour
{
    [Header("What counts as a wall/floor/ceiling")]
    [Tooltip("Layers that should trigger head clipping fade (ex: Environment).")]
    public LayerMask environmentLayers;

    [Header("Fade Behavior")]
    [Tooltip("Distance (meters) where fading begins.")]
    [Range(0.01f, 1.0f)]
    public float fadeStartDistance = 0.25f;

    [Tooltip("Distance (meters) where fade reaches maximum from proximity alone.")]
    [Range(0.01f, 0.5f)]
    public float fadeFullDistance = 0.08f;

    [Tooltip("How quickly the fade responds.")]
    [Range(1f, 30f)]
    public float fadeSpeed = 10f;

    [Tooltip("Maximum fade alpha when simply too close (0-1).")]
    [Range(0f, 1f)]
    public float maxProximityAlpha = 0.85f;

    [Tooltip("Fade alpha when the head is inside geometry (0-1).")]
    [Range(0f, 1f)]
    public float insideGeometryAlpha = 1.0f;

    [Header("Inside-Geometry Detection")]
    [Tooltip("Radius (meters) around the head used to detect if inside colliders.")]
    [Range(0.01f, 0.5f)]
    public float insideCheckRadius = 0.12f;

    [Tooltip("If true, uses a few raycasts to estimate proximity to nearby geometry.")]
    public bool useProximityRays = true;

    [Tooltip("Raycast length used for proximity checks (meters). Should be >= fadeStartDistance.")]
    [Range(0.05f, 2.0f)]
    public float proximityRayLength = 0.5f;

    [Header("UI Fade Target")]
    [Tooltip("Preferred: assign a CanvasGroup on your full-screen black Image.")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("Fallback: assign the full-screen Image if not using CanvasGroup.")]
    public Image fadeImage;

    [Header("Debug")]
    public bool drawDebugGizmos = false;

    float _currentAlpha = 0f;

    void Reset()
    {
        proximityRayLength = Mathf.Max(proximityRayLength, fadeStartDistance);
    }

    void Awake()
    {
        if (fadeCanvasGroup == null && fadeImage == null)
        {
            Debug.LogWarning("[HeadClipFader] No CanvasGroup or Image assigned. Fade will not display.");
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
        else if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }

        proximityRayLength = Mathf.Max(proximityRayLength, fadeStartDistance);
    }

    void Update()
    {
        float targetAlpha = 0f;

        // 1) Inside-geometry check (strongest)
        if (IsHeadInsideGeometry())
        {
            targetAlpha = insideGeometryAlpha;
        }
        else
        {
            // 2) Proximity check (soft fade)
            float proximityAlpha = useProximityRays ? ComputeProximityAlpha() : 0f;
            targetAlpha = Mathf.Max(targetAlpha, proximityAlpha);
        }

        // Smooth fade
        _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime));
        ApplyAlpha(_currentAlpha);
    }

    bool IsHeadInsideGeometry()
    {
        // Overlap sphere around camera position. If anything solid is intersecting, we consider it clipping.
        Vector3 p = transform.position;
        Collider[] hits = Physics.OverlapSphere(p, insideCheckRadius, environmentLayers, QueryTriggerInteraction.Ignore);
        return hits != null && hits.Length > 0;
    }

    float ComputeProximityAlpha()
    {
        Vector3 origin = transform.position;

        // We sample a few directions to get a reasonable "distance to nearest wall"
        // Forward/back/left/right/up plus a downward ray helps with ceilings and floors.
        Vector3[] dirs = new Vector3[]
        {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right,
            transform.up,
            -transform.up
        };

        float nearest = float.PositiveInfinity;

        for (int i = 0; i < dirs.Length; i++)
        {
            if (Physics.Raycast(origin, dirs[i], out RaycastHit hit, proximityRayLength, environmentLayers, QueryTriggerInteraction.Ignore))
            {
                nearest = Mathf.Min(nearest, hit.distance);
            }
        }

        if (float.IsInfinity(nearest))
            return 0f;

        // Map nearest distance to alpha.
        // At fadeStartDistance -> alpha 0
        // At fadeFullDistance -> alpha maxProximityAlpha
        if (nearest >= fadeStartDistance)
            return 0f;

        float t = Mathf.InverseLerp(fadeStartDistance, fadeFullDistance, nearest); // when nearest gets smaller, t approaches 1
        t = Mathf.Clamp01(t);

        return t * maxProximityAlpha;
    }

    void ApplyAlpha(float a)
    {
        a = Mathf.Clamp01(a);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = a;
            return;
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos) return;

        Gizmos.color = new Color(1f, 0f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, insideCheckRadius);

        if (useProximityRays)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
            Vector3 o = transform.position;

            Gizmos.DrawLine(o, o + transform.forward * proximityRayLength);
            Gizmos.DrawLine(o, o - transform.forward * proximityRayLength);
            Gizmos.DrawLine(o, o + transform.right * proximityRayLength);
            Gizmos.DrawLine(o, o - transform.right * proximityRayLength);
            Gizmos.DrawLine(o, o + transform.up * proximityRayLength);
            Gizmos.DrawLine(o, o - transform.up * proximityRayLength);
        }
    }
}
