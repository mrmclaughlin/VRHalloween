using UnityEngine;

namespace CodyDreams.Solutions.FakeFog
{
    [CreateAssetMenu(fileName = "SSFogData", menuName = "Cody Dream/Fake Fog/SS Fog Data")]
    public class SSFogData : ScriptableObject
    {
        [Tooltip("The main fog color that will be applied to the material.")]
        public Color color = Color.gray;

        [Tooltip("Controls the overall density of the depth fog. Applied in an exponential squared manner.")]
        public float Density = 0.5f;

        [Tooltip("If enabled, the fog color will be taken from Unity's lighting settings instead of this asset.")]
        public bool UseSceneFogColor = false;

        [Tooltip("The height at which the fog starts to take effect.")]
        public float FogHighet = 10f;

        [Tooltip("Determines how quickly the fog fades out with height.")]
        public float FogFallOff = 1f;

        [Tooltip("A multiplier that adjusts the overall strength of the fog effect. (0 = No Fog, 1 = Full Strength)")]
        [Range(0f, 1f)]
        public float OverAllIntensity = 1f;

        [Header("Light Influence")] [Tooltip("Directional light to be considered for fog color calculation.")]
        public Light directionalLight;


        /// <summary>
        /// Calculates the fog color based on the assigned directional light.
        /// This function should be called from the Editor UI.
        /// </summary>
        public void CalculateFogColorFromLight()
        {
            if (directionalLight == null)
            {
                Debug.LogWarning("No directional light assigned! Please assign a light first.");
                return;
            }

            // Blend fog color with light color (keeping original fog intensity)
            color = color * directionalLight.color;
            Debug.Log($"Fog color updated based on light: {directionalLight.name}");
        }
    }

}