using UnityEngine;
<<<<<<< Updated upstream
using TMPro;

public class DialogueNPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text nameBox;
    public TMP_Text dialogueText;      // Was missing in original code — declaration added
    public string[] dialogueLines;
    public float typingSpeed = 0.05f;

    private bool playerInRange = false;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;

    void Update()
    {
        if (!playerInRange)
            return;

        if (Input.anyKeyDown && !dialogueActive)
        {
            StartDialogue();
        }
        else if (Input.anyKeyDown && dialogueActive && !isTyping)
        {
            NextDialogueLine();
        }
        else if (Input.anyKeyDown && dialogueActive && isTyping)
        {
            // Finish the current line immediately
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (dialogueLines != null && dialogueLines.Length > 0)
            {
                int idx = Mathf.Clamp(currentLineIndex, 0, dialogueLines.Length - 1);
                if (dialogueText != null)
                    dialogueText.text = dialogueLines[idx];
            }

            isTyping = false;
        }
    }

    void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return;

        dialogueActive = true;
        currentLineIndex = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        ShowLine();
    }

    void ShowLine()
    {
        if (dialogueText == null || dialogueLines == null)
        {
            EndDialogue();
            return;
        }

        if (currentLineIndex < dialogueLines.Length)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            typingCoroutine = StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    System.Collections.IEnumerator TypeLine(string line)
    {
        isTyping = true;
        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char letter in line)
        {
            if (dialogueText != null)
                dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    void NextDialogueLine()
    {
        currentLineIndex++;
        ShowLine();
    }

    void EndDialogue()
    {
        dialogueActive = false;
        isTyping = false;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
=======

public class NPCDialogue : MonoBehaviour
{
    public Dialogue dialogue;

    private bool playerInRange;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TriggerDialogue();
        }
    }

    void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue.sentences);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
>>>>>>> Stashed changes
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
<<<<<<< Updated upstream
            if (dialogueActive)
                EndDialogue();
        }
    }
}       
=======
        }
    }
}
>>>>>>> Stashed changes
