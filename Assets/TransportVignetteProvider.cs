using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Simple vignette provider you can trigger manually for "transport" effects.
    /// </summary>
    public class TransportVignetteProvider : MonoBehaviour, ITunnelingVignetteProvider
    {
        [SerializeField]
        VignetteParameters m_Parameters = new VignetteParameters();

        public VignetteParameters vignetteParameters => m_Parameters;
 
        public VignetteParameters parameters => m_Parameters; // handy accessor
    }
}
