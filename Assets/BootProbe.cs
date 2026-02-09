using UnityEngine;
using UnityEngine.SceneManagement;

public class BootProbe : MonoBehaviour
{
    [SerializeField] string mainSceneName = "YourMainSceneName";

    void Awake()
    {
        Debug.Log("BOOTPROBE: Awake reached ✅");
    }

    void Start()
    {
        Debug.Log("BOOTPROBE: Start reached ✅");
        StartCoroutine(LoadMain());
    }

    System.Collections.IEnumerator LoadMain()
    {
        Debug.Log("BOOTPROBE: Loading main scene async...");
        var op = SceneManager.LoadSceneAsync(mainSceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            Debug.Log($"BOOTPROBE: progress={op.progress}");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("BOOTPROBE: Main scene loaded ✅");
    }
}
