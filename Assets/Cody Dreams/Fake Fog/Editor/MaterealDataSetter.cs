using UnityEngine;
using UnityEditor;
using System.IO;

namespace CodyDreams.Solutions.FakeFog
{
    public class SimpleMaterialFogApplier : EditorWindow
    {
        public Material[] materials = new Material[0];
        public FogDataSet[] fogDatas = new FogDataSet[0];
        private int selectedFogIndex = 0;
        private string assetPath = "Assets/FogSettings.asset"; // Default asset path

        private SSFogSettings assetData;

        [MenuItem("Tools/Cody Dream/Fake Fog/Fog Applier")]
        public static void ShowWindow()
        {
            GetWindow<SimpleMaterialFogApplier>("Fog Applier");
        }

        private void OnEnable()
        {
            LoadSettingsFromAsset();
        }

        private void OnGUI()
        {
            GUILayout.Label("Fog Applier", EditorStyles.boldLabel);

            // Materials Field
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty materialsProperty = serializedObject.FindProperty("materials");

            EditorGUILayout.PropertyField(materialsProperty, new GUIContent("Materials"), true);

            // Fog Data Field
            SerializedProperty fogDatasProperty = serializedObject.FindProperty("fogDatas");
            EditorGUILayout.PropertyField(fogDatasProperty, new GUIContent("Fog Datasets"), true);

            serializedObject.ApplyModifiedProperties();

            // Selected Fog Index
            if (fogDatas.Length > 0)
            {
                selectedFogIndex =
                    EditorGUILayout.IntSlider("Selected Fog Dataset", selectedFogIndex, 0, fogDatas.Length - 1);
            }
            else
            {
                EditorGUILayout.HelpBox("No fog datasets available!", MessageType.Warning);
            }

            // Asset Path Field
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset Path", GUILayout.Width(100));
            assetPath = EditorGUILayout.TextField(assetPath);

            if (GUILayout.Button("Browse", GUILayout.Width(100)))
            {
                string selectedPath =
                    EditorUtility.SaveFilePanel("Save Fog Settings", "Assets", "FogSettings", "asset");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    assetPath = "Assets" +
                                selectedPath.Substring(Application.dataPath.Length); // Convert to relative path
                }
                LoadSettingsFromAsset();
            }

            GUILayout.EndHorizontal();

            // Apply Button
            if (GUILayout.Button("Apply"))
            {
                ApplyFogData();
            }

            // Save Settings Button
            if (GUILayout.Button("Save Settings"))
            {
                SaveSettingsToAsset();
            }

            GUILayout.Label("<b>Fog Applier Tool</b>\n\n" +
                            "This tool allows you to apply screen-space fog settings to materials efficiently.\n\n" +

                            "<b>How It Works:</b>\n" +
                            "1. Assign your fog materials to the 'Materials' field.\n" +
                            "2. Create a <b>Fog Dataset</b>, which acts as a collection of multiple <b>SS Fog Data</b> entries.\n" +
                            "3. Each <b>Fog Dataset</b> contains an array of <b>SS Fog Data</b>, which holds the actual fog settings.\n" +
                            "4. The <b>Selected Fog Dataset</b> slider allows you to switch between different sets of fog settings.\n" +
                            "5. Click <b>Apply</b> to update the fog properties of all assigned materials.\n\n" +

                            "<b>Important Notes:</b>\n" +
                            "- Each material is assigned a fog setting based on its index in the list.\n" +
                            "- <b>SS Fog Data</b> is a <i>ScriptableObject</i>. You must create it via <b>Menu → Create → Cody Dream → Fake Fog → SS Fog Data</b>.\n" +
                            "- Keep fog datasets organized to match your scene structure for easy scene-based fog switching.\n\n" +

                            "<b>Best Practices:</b>\n" +
                            "- Ensure the number of materials matches the number of SS Fog Data entries in the selected dataset.\n" +
                            "- Organize fog datasets to correspond with Unity Scene Management indexes for easier automation.\n" +
                            "- Avoid unnecessary dataset duplication by reusing SS Fog Data where applicable.\n" +
                            "- Regularly check the fog parameters to ensure they align with the desired atmospheric effect.",

                new GUIStyle(EditorStyles.helpBox) { richText = true });
        }

        private void ApplyFogData()
        {
            if (fogDatas.Length == 0 || selectedFogIndex < 0 || selectedFogIndex >= fogDatas.Length)
            {
                Debug.LogWarning("Invalid fog data index.");
                return;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                SSFogData data = fogDatas[selectedFogIndex].FogDatas[i];
                materials[i].SetColor("_Fog_Color", data.color);
                materials[i].SetFloat("_Depth_Fog_Density", data.Density);
                materials[i].SetInteger("_Use_Unity_Fog_color", data.UseSceneFogColor ? 1 : 0);
                materials[i].SetFloat("_Depth_Fog_height", data.FogHighet);
                materials[i].SetFloat("_DFH_Falloff", data.FogFallOff);
                materials[i].SetFloat("_depth_Over_All", data.OverAllIntensity);
            }

            Debug.Log($"Applied fog settings from dataset {selectedFogIndex} to materials.");
        }

        private void SaveSettingsToAsset()
        {
            // Create the asset if it doesn't exist already
            if (assetData == null)
            {
                assetData = ScriptableObject.CreateInstance<SSFogSettings>();
                AssetDatabase.CreateAsset(assetData, assetPath);
                AssetDatabase.SaveAssets();
            }

            // Save the current settings to the asset
            assetData.selectedFogIndex = selectedFogIndex;
            assetData.materials = materials;
            assetData.fogDatas = fogDatas;

            EditorUtility.SetDirty(assetData);
            AssetDatabase.SaveAssets();

            Debug.Log($"Fog settings saved to asset at {assetPath}");
        }

        private void LoadSettingsFromAsset()
        {
            // Load the settings from the asset if it exists
            if (File.Exists(assetPath))
            {
                assetData = AssetDatabase.LoadAssetAtPath<SSFogSettings>(assetPath);

                if (assetData != null)
                {
                    selectedFogIndex = assetData.selectedFogIndex;
                    materials = assetData.materials;
                    fogDatas = assetData.fogDatas;

                    Debug.Log($"Fog settings loaded from asset at {assetPath}");
                }
            }
        }

    }
}

