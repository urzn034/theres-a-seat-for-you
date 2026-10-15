using System.Collections.Generic;
using UnityEngine;

public class TableService : MonoBehaviour, IInteractable
{
    public List<Transform> assignedSeatPoints;

    private NPCController assignedNpc;

    public string Prompt
    {
        get
        {
            CheckAssignedNpc();

            if (assignedNpc != null && assignedNpc.currentState == NPCController.NPCState.SittingWaitingForCoffee)
            {
                Recipe recipe = assignedNpc.GetAssignedRecipe();
                string drinkName = recipe != null ? recipe.drinkName : "Pesanan";
                return "Antar " + drinkName + " (Tekan E)";
            }
            return "";
        }
    }

    public void Interact()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        PlayerHand playerHand = FindObjectOfType<PlayerHand>();

        if (gm == null || playerHand == null) return;

        if (!playerHand.HasItem)
        {
            Debug.Log("Tangan kosong! Ambil kopi dulu.");
            return;
        }

        CheckAssignedNpc();

        if (assignedNpc == null || assignedNpc.currentState != NPCController.NPCState.SittingWaitingForCoffee)
        {
            Debug.Log("Tidak ada pelanggan yang menunggu pesanan di meja ini.");
            return;
        }

        Recipe recipe = assignedNpc.GetAssignedRecipe();
        if (recipe == null) return;

        string heldItem = playerHand.GetHeldItem();

        if (!string.Equals(heldItem, recipe.drinkName, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Kopi di tangan (" + heldItem + ") tidak sesuai dengan pesanan (" + recipe.drinkName + ")!");
            return;
        }

        if (gm.CheckAndCompleteOrder(recipe.drinkName, assignedNpc))
        {
            assignedNpc.gotPerfect = playerHand.heldPerfect;
            playerHand.ClearHand();
            assignedNpc = null;
            Debug.Log("Kopi sukses diantar!");
        }
    }

    private void CheckAssignedNpc()
    {
        if (assignedSeatPoints == null || assignedSeatPoints.Count == 0) return;

        PlayerHand hand = FindObjectOfType<PlayerHand>();
        string held = hand != null ? hand.GetHeldItem() : "";
        NPCController fallback = null;

        foreach (NPCController npc in FindObjectsOfType<NPCController>())
        {
            if (npc.currentState != NPCController.NPCState.SittingWaitingForCoffee) continue;

            foreach (Transform seat in assignedSeatPoints)
            {
                if (seat == null) continue;
                if (Vector3.Distance(seat.position, npc.transform.position) >= 0.5f) continue;

                Recipe r = npc.GetAssignedRecipe();
                if (r != null && string.Equals(r.drinkName, held, System.StringComparison.OrdinalIgnoreCase))
                {
                    assignedNpc = npc;
                    return;
                }
                if (fallback == null) fallback = npc;
            }
        }

        if (fallback != null)
        {
            assignedNpc = fallback;
            return;
        }

        if (assignedNpc != null
            && assignedNpc.currentState != NPCController.NPCState.SittingWaitingForCoffee
            && assignedNpc.currentState != NPCController.NPCState.EnjoyingDrink)
        {
            assignedNpc = null;
        }
    }
}