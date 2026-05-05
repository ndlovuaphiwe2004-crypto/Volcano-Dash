using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class LoadingScene : MonoBehaviour
{
    [Tooltip("Name of your main game scene")]
    public string sceneToLoad;

    [Tooltip("Optional: UI Slider to drive the fill. Preferred when using Unity Slider component.")]
    public Slider progressSlider;

    [Tooltip("Optional: UI Image used for the loading bar (fallback).")]
    public Image progressBar;

    [Tooltip("Minimum time to show the loading screen")]
    public float minDisplayTime = 2.0f;

    void Start()
    {
        // Trim to avoid accidental whitespace issues from inspector copy/paste.
        sceneToLoad = sceneToLoad?.Trim();

        Debug.Log($"[LoadingScene] Start. sceneToLoad='{sceneToLoad}'");

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError("[LoadingScene] sceneToLoad is empty. Set it in the inspector or load a valid scene.");
            return;
        }

        // Validate the scene name exists in Build Settings
        int buildSceneCount = SceneManager.sceneCountInBuildSettings;
        int foundBuildIndex = -1;

        for (int i = 0; i < buildSceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneToLoad, StringComparison.OrdinalIgnoreCase))
            {
                foundBuildIndex = i;
                break;
            }
        }

        if (foundBuildIndex < 0)
        {
            // Build a helpful message listing available scenes
            var sb = new StringBuilder();
            sb.AppendLine("[LoadingScene] Scene not found in Build Settings:");
            sb.AppendLine($"  Requested: '{sceneToLoad}'");
            sb.AppendLine("  Scenes in Build Settings:");

            for (int i = 0; i < buildSceneCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                sb.AppendLine($"    - {name} (build index {i})");
            }

            sb.AppendLine("Action: Add the scene to File > Build Settings... or correct the name in the inspector.");
            Debug.LogError(sb.ToString());
            return;
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.wholeNumbers = false;
            progressSlider.value = 0f;
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
        }

        // Start the background loading process using the validated scene name
        StartCoroutine(LoadSceneAsync());
    }
                
    IEnumerator LoadSceneAsync()
    {
        float startTime = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Prevent the scene from activating immediately
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // operation.progress goes from 0 to 0.9
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Update UI: prefer slider if assigned, otherwise use image fill
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }
            else if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }

            // Wait until both the scene is loaded and the minimum time has passed
            if (operation.progress >= 0.9f && (Time.time - startTime) >= minDisplayTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
