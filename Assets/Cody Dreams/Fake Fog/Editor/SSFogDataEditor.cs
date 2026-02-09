using UnityEngine;
using UnityEditor;

namespace CodyDreams.Solutions.FakeFog
{
    [CustomEditor(typeof(SSFogData))]
    public class SSFogDataEditor : Editor
    {
        private Material extractmat;

        public override void OnInspectorGUI()
        {
            var fogData = (SSFogData)target;

            GUILayout.Label("<b>Screen-Space Fog Data</b>\n" +
                            "Use this asset to define fog settings for materials.\n\n" +
                            "<b>Note:</b> Some fields will be ignored depending on the material type applied.\n",
                new GUIStyle(EditorStyles.helpBox) { richText = true });

            // Fog Color
            fogData.color = EditorGUILayout.ColorField(
                new GUIContent("Fog Color", "The main fog color that will be applied to the material."), fogData.color);

            // Density
            fogData.Density = EditorGUILayout.FloatField(
                new GUIContent("Density",
                    "Controls the overall density of the depth fog. Applied in an exponential squared manner."),
                fogData.Density);

            // Use Unity Fog Color Toggle
            fogData.UseSceneFogColor =
                EditorGUILayout.Toggle(
                    new GUIContent("Use Unity Fog Color",
                        "If enabled, the fog color will be taken from Unity's lighting settings instead."),
                    fogData.UseSceneFogColor);

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Height & Falloff Settings", EditorStyles.boldLabel);
            fogData.FogHighet = EditorGUILayout.FloatField(
                new GUIContent("Fog Height", "The height at which the fog starts to take effect."), fogData.FogHighet);
            fogData.FogFallOff = EditorGUILayout.FloatField(
                new GUIContent("Fog Falloff", "Determines how quickly the fog fades out with height."),
                fogData.FogFallOff);

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Intensity Settings", EditorStyles.boldLabel);
            fogData.OverAllIntensity =
                EditorGUILayout.Slider(
                    new GUIContent("Overall Intensity",
                        "A multiplier that adjusts the overall strength of the fog effect. (0 = No Fog, 1 = Full Strength)"),
                    fogData.OverAllIntensity, 0f, 1f);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Light Influence", EditorStyles.boldLabel);
            fogData.directionalLight = (Light)EditorGUILayout.ObjectField(
                new GUIContent("Directional Light", "Directional light to be considered for fog color calculation."),
                fogData.directionalLight, typeof(Light), true);

            if (GUILayout.Button("Calculate Fog Color from Light"))
            {
                fogData.CalculateFogColorFromLight();
                EditorUtility.SetDirty(fogData);
            }

            extractmat = (Material)EditorGUILayout.ObjectField(
                new GUIContent("Extract Material", "Using this matereal , data for this object will be extracted"),
                extractmat, typeof(Material), true);
            if (GUILayout.Button("Get Data From Mat"))
            {
                fogData.color = extractmat.GetColor("_Fog_Color");
                if (extractmat.HasFloat("_Depth_Fog_Density"))
                {
                    fogData.Density = extractmat.GetFloat("_Depth_Fog_Density");
                }

                fogData.UseSceneFogColor = extractmat.GetFloat("_Use_Unity_Fog_color") == 1 ? true : false;
                fogData.FogHighet = extractmat.GetFloat("_Depth_Fog_height");
                fogData.FogFallOff = extractmat.GetFloat("_DFH_Falloff");
                fogData.OverAllIntensity = extractmat.GetFloat("_depth_Over_All");
            }

            // Apply changes
            if (GUI.changed) EditorUtility.SetDirty(fogData);
        }
    }

}