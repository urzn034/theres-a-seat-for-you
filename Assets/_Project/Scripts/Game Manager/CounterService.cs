using UnityEngine;

public class CounterService : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptMessage = "Tanya Pesanan (Tekan E)";
    public string Prompt => promptMessage;

    public void Interact()
    {
        var dm = FindObjectOfType<DialogueManager>();
        if (dm != null && dm.IsRunning) return;

        foreach (var npc in FindObjectsOfType<NPCController>())
        {
            if (npc.currentState != NPCController.NPCState.WaitingForInteraction) continue;

            if (npc.orderDialogue != null && dm != null)
            {
                var target = npc;
                dm.StartDialogue(npc.orderDialogue.lines, () => target.InteractWithCustomer());
            }
            else
            {
                npc.InteractWithCustomer();
            }
            return;
        }

        Debug.Log("Belum ada pelanggan yang mengantre di kasir.");
    }
}