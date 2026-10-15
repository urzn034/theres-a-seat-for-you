using UnityEngine;

public class IngredientStation : MonoBehaviour, IInteractable
{
    [SerializeField] private string ingredientName = "Coffee Beans";
    [SerializeField] private string promptMessage = "Ambil Biji Kopi";

    public string Prompt => promptMessage;

    [SerializeField] private FridgePopup popup;

    public void Interact()
    {
        // Cari komponen PlayerHand pada objek Player di scene
        PlayerHand playerHand = FindObjectOfType<PlayerHand>();

        if (popup != null && playerHand != null)
        {
            if (!playerHand.HasItem) popup.Open(playerHand);
            else Debug.Log("Tangan Eve sedang memegang item lain!");
            return;
        }

        if (playerHand != null)
        {
            if (!playerHand.HasItem)
            {
                playerHand.PickUpItem(ingredientName);
                Debug.Log("Berhasil mengambil " + ingredientName);
            }
            else
            {
                Debug.Log("Tangan Eve sedang memegang item lain!");
            }
        }
        else
        {
            Debug.LogWarning("Komponen PlayerHand tidak ditemukan di scene!");
        }
    }
}