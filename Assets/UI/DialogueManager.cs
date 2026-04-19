using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    // Singleton
    public static DialogueManager Instance { get; private set; }

    // UI references (assign in inspector)
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;
    public TMP_Text nameBox;
    public Animator animator;

    // Typing behavior
    public float typingSpeed = 0.05f;

    // State
    public bool IsTyping { get; private set; }
    public bool DialogueFinished { get; private set; } = true;

    // Internal
    private Queue<string> sentences;
    private Coroutine typingCoroutine;
    private string currentSentence;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        sentences = new Queue<string>();
    }

    // Overload: start with speaker name
    public void StartDialogue(string speakerName, IEnumerable<string> newSentences)
    {
        if (nameBox != null)
            nameBox.text = speakerName ?? string.Empty;

        StartDialogue(newSentences);
    }

    public void StartDialogue(IEnumerable<string> newSentences)
    {
        if (animator != null)
            animator.SetBool("IsOpen", true);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

                                sentences.Clear();
        if (newSentences != null)
        {
            foreach (string sentence in newSentences)
            {
                if (sentence != null)
                    sentences.Enqueue(sentence);
            }
        }

        DialogueFinished = false;
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // If currently typing, finish the current sentence immediately
        if (IsTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (dialogueText != null)
                dialogueText.text = currentSentence ?? string.Empty;

            IsTyping = false;
            return;
        }

        if (sentences == null || sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        IsTyping = true;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (string.IsNullOrEmpty(sentence))
        {
            // Small pause to show empty line if necessary
            yield return null;
            IsTyping = false;
            yield break;
        }

        foreach (char letter in sentence.ToCharArray())
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            if (typingSpeed > 0f)
                yield return new WaitForSeconds(typingSpeed);
            else
                yield return null;
        }

        IsTyping = false;
        typingCoroutine = null;
    }

    private void EndDialogue()
    {
        if (animator != null)
            animator.SetBool("IsOpen", false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        DialogueFinished = true;
        currentSentence = null;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        IsTyping = false;
    }

    // Reserved for input handling or future behavior
    void Update()
    {
    }
}