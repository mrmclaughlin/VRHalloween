using UnityEngine;
using UnityEngine.SceneManagement;

public class HandResetButton : MonoBehaviour
{
    [SerializeField] float cooldownSeconds = 1.0f;
    float nextAllowedTime = 0f;

    void Awake()
    {
        Debug.Log("[ResetButton] Awake on " + gameObject.name);
    }

    void OnEnable()
    {
        Debug.Log("[ResetButton] Enabled");
    }

    public void ResetScene()
    {
        Debug.Log("[ResetButton] ResetScene() CALLED");

        if (Time.time < nextAllowedTime)
        {
            Debug.Log("[ResetButton] Cooldown active, ignoring press");
            return;
        }

        nextAllowedTime = Time.time + cooldownSeconds;

        var scene = SceneManager.GetActiveScene();
        Debug.Log("[ResetButton] Reloading scene: " + scene.name);

        SceneManager.LoadScene(scene.buildIndex);
    }
}
