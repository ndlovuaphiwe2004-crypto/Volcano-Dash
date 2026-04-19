using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
<<<<<<< Updated upstream
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;
    public TMP_Text nameBox;
    public Animator animator;
    private Queue<string> sentences;
    public static DialogueManager Instance;

    public float typingSpeed = 0.05f; // Time delay between each character
    public bool IsTyping  = false;
    public bool dialogueFinished = true  ;

=======
    public static DialogueManager Instance { get; private set; }
     void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public TMP_Text dialogueText;
    public Animator animator;
    public GameObject dialogueGameObject;
    private Queue<string> sentences;
    public bool isDialogueActive => dialogueGameObject.activeSelf;
    public float typingSpeed = 0.05f;
>>>>>>> Stashed changes
    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    // Start a new dialogue from a collection of sentences
    public void StartDialogue(IEnumerable<string> newSentences)
    {
        if (animator != null)
            animator.SetBool("IsOpen", true);

        sentences.Clear();
        foreach (string sentence in newSentences)
<<<<<<< Updated upstream
        {
            sentences.Enqueue(sentence);
        }
=======
            sentences.Enqueue(sentence);
>>>>>>> Stashed changes

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    private void EndDialogue()
    {
        if (animator != null)
            animator.SetBool("IsOpen", false);
    }

    // Update is called once per frame (kept for future use)
    void Update()
    {
    }
}