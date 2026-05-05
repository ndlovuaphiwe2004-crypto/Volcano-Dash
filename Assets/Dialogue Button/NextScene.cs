using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    private GameObject nextButton;

    [SerializeField]
    private float delaySeconds = 2f;

    [Header("Button Scale Feedback")]
    [SerializeField]
    private float hoverScaleMultiplier = 1.15f;

    [SerializeField]
    private float pressedScaleMultiplier = 1.25f;

    [SerializeField]
    private float scaleDuration = 0.12f;

    private Vector3 normalScale = Vector3.one;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        if (nextButton == null)
        {
            Button localButton = GetComponent<Button>();
            if (localButton != null)
            {
                nextButton = localButton.gameObject;
            }
            else
            {
                GameObject found = GameObject.Find("NextButton");
                if (found != null)
                    nextButton = found;
            }
        }

        if (nextButton != null)
        {
            // Capture the initial scale
            normalScale = nextButton.transform.localScale;

            // Ensure button is initially hidden
            nextButton.SetActive(false);

            // Attach pointer event handlers for scale feedback
            AttachPointerHandlers(nextButton);
        }

        StartCoroutine(ShowButtonAfterDelay());
    }

    private IEnumerator ShowButtonAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        if (nextButton != null)
            nextButton.SetActive(true);
    }

    private void AttachPointerHandlers(GameObject target)
    {
        // Ensure there's an EventTrigger on the target
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = target.AddComponent<EventTrigger>();

        // Remove existing entries for the types we will manage to avoid duplicates
        if (trigger.triggers != null && trigger.triggers.Count > 0)
        {
            trigger.triggers.RemoveAll(entry =>
                entry.eventID == EventTriggerType.PointerEnter ||
                entry.eventID == EventTriggerType.PointerExit ||
                entry.eventID == EventTriggerType.PointerDown ||
                entry.eventID == EventTriggerType.PointerUp);
        }

        // Helper to create and add an entry
        void AddEntry(EventTriggerType eventType, UnityAction<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener(new UnityAction<BaseEventData>(callback));
            trigger.triggers.Add(entry);
        }

        AddEntry(EventTriggerType.PointerEnter, OnPointerEnter);
        AddEntry(EventTriggerType.PointerExit, OnPointerExit);
        AddEntry(EventTriggerType.PointerDown, OnPointerDown);
        AddEntry(EventTriggerType.PointerUp, OnPointerUp);
    }

    private void OnPointerEnter(BaseEventData data)
    {
        Vector3 targetScale = normalScale * hoverScaleMultiplier;
        StartScaleAnimation(targetScale);
    }

    private void OnPointerExit(BaseEventData data)
    {
        StartScaleAnimation(normalScale);
    }

    private void OnPointerDown(BaseEventData data)
    {
        Vector3 targetScale = normalScale * pressedScaleMultiplier;
        StartScaleAnimation(targetScale);
    }

    private void OnPointerUp(BaseEventData data)
    {
        // Return to hover size if pointer still over button, otherwise normal
        Vector3 targetScale = normalScale * hoverScaleMultiplier;
        StartScaleAnimation(targetScale);
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (nextButton == null)
            return;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(AnimateScale(nextButton.transform, targetScale, scaleDuration));
    }

    private IEnumerator AnimateScale(Transform t, Vector3 target, float duration)
    {
        Vector3 start = t.localScale;
        if (duration <= 0f)
        {
            t.localScale = target;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float factor = Mathf.Clamp01(elapsed / duration);
            // Smooth interpolation
            factor = Mathf.SmoothStep(0f, 1f, factor);
            t.localScale = Vector3.Lerp(start, target, factor);
            yield return null;
        }

        t.localScale = target;
    }

    public void LoadNextScene()
    {
                // Load the next scene (immediate)
        SceneManager.LoadScene("Main_Menu");
    }
}
