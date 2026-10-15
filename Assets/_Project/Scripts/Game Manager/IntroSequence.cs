using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    public DialogueManager dialogue;
    public DialogueData outside1;

    void Start()
    {
        if (dialogue != null && outside1 != null)
            dialogue.StartDialogue(outside1.lines);
    }
}