using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NextSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip nextClip;

    [SerializeField]
    private Button uiButton;

    [SerializeField]
    private bool insertButton = false;

    [SerializeField]
    private string buttonText = "Next";

    private AudioSource audioSource;

    void Start()
    {
        // Ensure an AudioSource is present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // If a button is assigned in inspector, wire it up
        if (uiButton != null)
        {
            uiButton.onClick.AddListener(PlayNextSound);
            return;
        }

        // If requested, create and insert a button at runtime
        if (insertButton)
        {
            CreateButton();
        }
    }

    public void PlayNextSound()
    {
        if (nextClip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(nextClip);
    }   

    private void CreateButton()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Create Button GameObject
        GameObject buttonGO = new GameObject("NextButton");
        buttonGO.transform.SetParent(canvas.transform, false);

        // Add Image (required for Button visuals) and Button
        Image image = buttonGO.AddComponent<Image>();
        // Use default sprite if available (Unity will fallback to UI sprite)
        image.color = new Color(1f, 1f, 1f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        // Configure RectTransform size and position (bottom-right-ish)
        RectTransform rt = buttonGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160f, 40f);
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-10f, 10f);

        // Create Text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        Text txt = textGO.AddComponent<Text>();
        txt.text = buttonText;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        // Assign and hook up the button
        uiButton = button;
        uiButton.onClick.AddListener(PlayNextSound);
    }

    void OnDestroy()
    {
        if (uiButton != null)
        {
            uiButton.onClick.RemoveListener(PlayNextSound);
        }
    }
}
