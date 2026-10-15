using UnityEngine;

public class FinchInteract : MonoBehaviour, IInteractable
{
    public DialogueManager dialogue;
    public DialogueData outside2;
    [SerializeField] private string promptMessage = "Bicara";

    bool played;

    public string Prompt => promptMessage;

    public void Interact()
    {
        if (played || dialogue == null || dialogue.IsRunning || outside2 == null) return;
        played = true;
        dialogue.StartDialogue(outside2.lines);
    }
}