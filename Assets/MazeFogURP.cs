using UnityEngine;

public class MazeFogURP : MonoBehaviour
{
    [Header("Fog Settings (match your Lighting window)")]
    public FogMode mode = FogMode.Linear;
    public Color fogColor = new Color(0.75f, 0.78f, 0.82f, 1f);

    [Header("Linear fog")]
    public float linearStart = 8f;
    public float linearEnd = 45f;

    [Header("Exponential fog")]
    public float expDensity = 0.02f;

    void OnEnable()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = mode;
        RenderSettings.fogColor = fogColor;

        ApplyOnValues();
    }

    void OnDisable()
    {
        // Clear fog
        if (RenderSettings.fogMode == FogMode.Linear)
        {
            // Push it so far out it effectively disappears
            RenderSettings.fogStartDistance = 100000f;
            RenderSettings.fogEndDistance = 100000f;
        }
        else
        {
            RenderSettings.fogDensity = 0f;
        }

        RenderSettings.fog = false;
    }

    void ApplyOnValues()
    {
        if (mode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = linearStart;
            RenderSettings.fogEndDistance = linearEnd;
        }
        else
        {
            RenderSettings.fogDensity = expDensity;
        }
    }
}
