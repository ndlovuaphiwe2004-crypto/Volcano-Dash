using UnityEngine;

public class CanvasShrinker : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    void Update()
    {
        if (mainCamera == null) return;

        // When camera is 7.5, scale becomes 0.66 (smaller)
        float targetScale = 5f / mainCamera.orthographicSize;

        transform.localScale = new Vector3(targetScale, targetScale, 1f);
    }
}