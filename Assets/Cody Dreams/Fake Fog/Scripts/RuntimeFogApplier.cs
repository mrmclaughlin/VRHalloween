using UnityEngine;

namespace CodyDreams.Solutions.FakeFog
{
    /// <summary>
    /// Applies fog settings from a list of SSFogData ScriptableObjects to a corresponding
    /// list of target Materials at runtime when the component awakens.
    /// Also provides editor methods to apply settings immediately via Context Menu or Custom Inspector.
    /// Assumes a 1:1 mapping based on index (Material[i] uses FogData[i]).
    /// </summary>
    public class RuntimeFogApplier : MonoBehaviour
    {
        [Header("Required Assets")] [Tooltip("The list of Materials whose fog properties will be set. Order matters!")]
        public Material[] targetMaterials; // You can change this to List<Material> if you prefer

        [Tooltip(
            "The list of SSFogData assets containing the fog settings. Must match the order and count of Target Materials.")]
        public SSFogData[] fogDatas; // You can change this to List<SSFogData> if you prefer

        // Runtime execution
        void Awake()
        {
            ApplyAllFogSettings();
        }

        // Private method containing the core logic
        private void ApplyAllFogSettings() // Changed back to private, logic called via public method
        {
            // --- Basic Array Checks ---
            if (targetMaterials == null || targetMaterials.Length == 0)
            {
                Debug.LogWarning(
                    $"[{nameof(RuntimeFogApplier)}] Warning: Target Materials array is null or empty on GameObject '{this.gameObject.name}'. No fog settings applied.",
                    this);
                return;
            }

            if (fogDatas == null || fogDatas.Length == 0)
            {
                Debug.LogWarning(
                    $"[{nameof(RuntimeFogApplier)}] Warning: Fog Datas array is null or empty on GameObject '{this.gameObject.name}'. No fog settings applied.",
                    this);
                return;
            }

            // --- Length Mismatch Check ---
            if (targetMaterials.Length != fogDatas.Length)
            {
                Debug.LogError(
                    $"[{nameof(RuntimeFogApplier)}] Error: The number of Target Materials ({targetMaterials.Length}) does not match the number of Fog Datas ({fogDatas.Length}) on GameObject '{this.gameObject.name}'. Fog application aborted. Please ensure both arrays have the same size and corresponding entries.",
                    this);
                return;
            }

            // --- Apply Fog Properties Loop ---
            int count = targetMaterials.Length;
            int appliedCount = 0;
            bool errorsOccurred = false; // Flag to track if any error happened during loop

            for (int i = 0; i < count; i++)
            {
                Material currentMaterial = targetMaterials[i];
                SSFogData currentFogData = fogDatas[i];

                // --- Individual Element Checks ---
                if (currentMaterial == null)
                {
                    Debug.LogWarning(
                        $"[{nameof(RuntimeFogApplier)}] Warning: Target Material at index {i} is null on GameObject '{this.gameObject.name}'. Skipping this entry.",
                        this);
                    errorsOccurred = true;
                    continue;
                }

                if (currentFogData == null)
                {
                    Debug.LogWarning(
                        $"[{nameof(RuntimeFogApplier)}] Warning: Fog Data (SSFogData) at index {i} is null on GameObject '{this.gameObject.name}'. Skipping fog application for material '{currentMaterial.name}'.",
                        this);
                    errorsOccurred = true;
                    continue;
                }

                // --- Apply Properties ---
                try // Add try-catch for safety when applying properties
                {
                    currentMaterial.SetColor("_Fog_Color", currentFogData.color);
                    currentMaterial.SetFloat("_Depth_Fog_Density", currentFogData.Density);
                    currentMaterial.SetInteger("_Use_Unity_Fog_color", currentFogData.UseSceneFogColor ? 1 : 0);
                    currentMaterial.SetFloat("_Depth_Fog_height", currentFogData.FogHighet);
                    currentMaterial.SetFloat("_DFH_Falloff", currentFogData.FogFallOff);
                    currentMaterial.SetFloat("_depth_Over_All", currentFogData.OverAllIntensity);
                    appliedCount++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(
                        $"[{nameof(RuntimeFogApplier)}] Error applying fog properties to material '{currentMaterial.name}' at index {i}: {ex.Message}",
                        currentMaterial);
                    errorsOccurred = true;
                }
            }

            // --- Log Summary ---
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string context = Application.isPlaying ? "Runtime" : "Editor"; // Differentiate log source
            if (appliedCount > 0 && !errorsOccurred)
            {
                Debug.Log(
                    $"[{nameof(RuntimeFogApplier)} - {context}] Successfully applied fog settings to all {appliedCount}/{count} material(s) on GameObject '{this.gameObject.name}'.",
                    this);
            }
            else if (appliedCount > 0)
            {
                Debug.LogWarning(
                    $"[{nameof(RuntimeFogApplier)} - {context}] Applied fog settings to {appliedCount}/{count} material(s) on GameObject '{this.gameObject.name}', but some errors or skips occurred. Check previous logs.",
                    this);
            }
            else if (errorsOccurred)
            {
                Debug.LogError(
                    $"[{nameof(RuntimeFogApplier)} - {context}] Failed to apply any fog settings due to errors or skips on GameObject '{this.gameObject.name}'. Check previous logs.",
                    this);
            }
            // No message needed if count is 0 and no errors, handled by initial checks.
#endif
        }

        /// <summary>
        /// Public method to trigger the fog settings application.
        /// Can be called from other scripts or editor tools.
        /// </summary>
        [ContextMenu("Apply Fog Settings Now (Context Menu)")]
        public void UpdateAllFogSettings()
        {
            Debug.Log(
                $"[{nameof(RuntimeFogApplier)} - Editor] Applying settings via {(Event.current != null ? "Inspector Button" : "Context Menu")}...",
                this); // Context hint
            ApplyAllFogSettings();
            // Note: Changes made in the editor via this method might only persist
            // if the affected materials are saved or the scene is saved.
            // Runtime Awake will re-apply based on assigned data regardless.
        }
    }
}