using UnityEngine;

namespace CodyDreams.Solutions.FakeFog
{
    [System.Serializable]
    public class SSFogSettings : ScriptableObject
    {
        public Material[] materials;
        public FogDataSet[] fogDatas;
        public int selectedFogIndex;
    }

    [System.Serializable]
    public struct FogDataSet
    {
        public SSFogData[] FogDatas;
    }
}