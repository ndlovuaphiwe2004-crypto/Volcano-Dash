using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider loadingSlider;

    [Header("Scene to Load")]
    [SerializeField] private string sceneToLoad;

    [Header("Loading Settings")]
    [Tooltip("Time in seconds for the slider to fill.")]
    [SerializeField] private float loadDuration = 10f;

    private void Start()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("LoadingScene: 'sceneToLoad' is not set. Assign a scene name or build index in the inspector.");
            return;
        }

        if (loadingSlider == null)
        {
            Debug.LogError("LoadingScene: 'loadingSlider' reference is missing. Assign the UI Slider in the inspector.");
            return;
        }

        if (loadDuration <= 0f)
        {
            Debug.LogWarning("LoadingScene: 'loadDuration' must be > 0. Defaulting to 10 seconds.");
            loadDuration = 10f;
        }

        loadingSlider.value = 0f;
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        int targetBuildIndex = -1;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (int.TryParse(sceneToLoad, out int parsedIndex))
        {
            if (parsedIndex < 0 || parsedIndex >= totalScenes)
            {
                Debug.LogError($"LoadingScene: Scene build index '{parsedIndex}' is out of range (0..{totalScenes - 1}). Add the scene to Build Settings.");
                yield break;
            }

            targetBuildIndex = parsedIndex;
        }
        else
        {
            for (int i = 0; i < totalScenes; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, sceneToLoad, System.StringComparison.OrdinalIgnoreCase))
                {
                    targetBuildIndex = i;
                    break;
                }
            }

            if (targetBuildIndex == -1)
            {
                Debug.LogError($"LoadingScene: Scene '{sceneToLoad}' was not found in Build Settings. Open File > Build Settings and add the scene (use the scene's file name, not its path).");
                yield break;
            }
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetBuildIndex);
        if (asyncLoad == null)
        {
            Debug.LogError($"LoadingScene: Failed to start async load for scene '{sceneToLoad}' (build index {targetBuildIndex}). Ensure the scene is included in Build Settings.");
            yield break;
        }

        asyncLoad.allowSceneActivation = false;

        float elapsed = 0f;

        while (!asyncLoad.isDone)
        {
            elapsed += Time.deltaTime;

            float timeProgress = Mathf.Clamp01(elapsed / loadDuration);
            float asyncProgressNormalized = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingSlider != null)
            {
                loadingSlider.value = timeProgress;
            }

            // Only activate when both the timer has finished (slider reached 1) and the async load has reached 0.9 (normalized == 1)
            if (timeProgress >= 1f && asyncProgressNormalized >= 1f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
