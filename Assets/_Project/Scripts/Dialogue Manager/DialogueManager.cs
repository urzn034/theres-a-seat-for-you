using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea] public string text;
    [TextArea] public string textID;
}

public class DialogueManager : MonoBehaviour
{
    public static int Language = 0; // 0 = English, 1 = Indonesia

    public GameObject panel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;
    public Sprite evePortrait;
    public float charDelay = 0.03f;
    public DialogueLine[] testLines;

    public bool IsRunning { get; private set; }

    DialogueLine[] lines;
    int index;
    bool typing;
    Coroutine routine;
    PlayerController player;
    Action onFinished;

    void Awake() { panel.SetActive(false); }

    void Start()
    {
        if (testLines != null && testLines.Length > 0) StartDialogue(testLines);
    }

    public void StartDialogue(DialogueLine[] newLines, Action finished = null)
    {
        if (newLines == null || newLines.Length == 0) return;
        lines = newLines;
        onFinished = finished;
        index = 0;
        IsRunning = true;
        player = FindObjectOfType<PlayerController>();
        if (player != null) player.canMove = false;
        panel.SetActive(true);
        routine = StartCoroutine(Type(lines[index]));
    }

    string GetText(DialogueLine line)
    {
        if (Language == 1 && !string.IsNullOrEmpty(line.textID)) return line.textID;
        return line.text;
    }

    IEnumerator Type(DialogueLine line)
    {
        typing = true;
        if (nameText != null)
        {
            nameText.text = line.speaker;
            nameText.gameObject.SetActive(!string.IsNullOrEmpty(line.speaker));
        }

        if (portraitImage != null)
        {
            bool show = evePortrait != null && line.speaker == "Eve";
            portraitImage.sprite = evePortrait;
            portraitImage.gameObject.SetActive(show);
        }
        string t = GetText(line);
        dialogueText.text = t;
        for (int i = 0; i <= t.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }
        typing = false;
    }

    void Update()
    {
        if (!panel.activeSelf || !Input.GetMouseButtonDown(0)) return;

        if (typing)
        {
            StopCoroutine(routine);
            dialogueText.maxVisibleCharacters = 9999;
            typing = false;
        }
        else
        {
            index++;
            if (index >= lines.Length) End();
            else routine = StartCoroutine(Type(lines[index]));
        }
    }

    void End()
    {
        panel.SetActive(false);
        IsRunning = false;
        if (player != null) player.canMove = true;
        var cb = onFinished;
        onFinished = null;
        cb?.Invoke();
    }
}