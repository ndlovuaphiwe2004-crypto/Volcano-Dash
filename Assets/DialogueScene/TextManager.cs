using System.Collections;
using UnityEngine;
using TMPro;

public class MultiTMPAnimator : MonoBehaviour
{
    [Header("Assign 7 TMP Texts")]
    public TextMeshProUGUI[] tmpTexts = new TextMeshProUGUI[7];

    [Header("Animation Settings")]
    public float charDelay = 0.05f;   // typing speed
    public float holdTime = 2f;       // how long text stays visible
    public float delayBetween = 0.5f; // pause before next TMP

    // Sentences to display
    private string[] sentences = new string[]
    {
        "My grandfather used to tell me stories about this mountain. He said Gugurang watches everything. Every step. Every breath. Every offering thrown into his fire. I thought they were just stories. Old tales to scare children into being good. So I walked up here with empty hands. No sacrifice. No prayer. Just... curiosity..",
        "Now the mountain is crying. As it explodes, every offering my people have given for a thousand years is all coming back. Jeweled daggers passed down through generations. Masks worn by elders I never got to meet.",
        "I can see them all. The offerings. The prayers. The sacrifices. They're all here. And they're all crying out to me. To you. To everyone who has ever climbed this mountain. To everyone who has ever sought Gugurang's favor.",
        "I need to run for my life!!!!!",
        "I wasn't ready when I climbed this mountain. But maybe... maybe I can be ready when I come back down.",
        "However,I can’t leave these coins and gems behind."
    };

    private void Start()
    {
        StartCoroutine(AnimateAllTexts());
    }

    private IEnumerator AnimateAllTexts()
    {
        for (int i = 0; i < tmpTexts.Length && i < sentences.Length; i++)
        {
            var tmp = tmpTexts[i];
            if (tmp == null) continue;

            // Hide all TMPs first
            foreach (var other in tmpTexts)
            {
                if (other != null)
                {
                    other.text = "";
                    other.alpha = 0f;
                    other.transform.localScale = Vector3.zero;
                }
            }

            string sentence = sentences[i];

            // Fade + scale in
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 1f;
                tmp.alpha = Mathf.Lerp(0f, 1f, t);
                tmp.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }

            // Typewriter effect
            tmp.text = "";
            for (int j = 0; j < sentence.Length; j++)
            {
                tmp.text += sentence[j];
                yield return new WaitForSeconds(charDelay);
            }

            // Hold text for a while
            yield return new WaitForSeconds(holdTime);

            // Clear text before moving on
            tmp.text = "";

            // Wait before animating the next TMP
            yield return new WaitForSeconds(delayBetween);
        }
    }
}
