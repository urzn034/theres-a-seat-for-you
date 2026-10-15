using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField] private string heldItem = "";
    public bool heldPerfect;

    public bool HasItem => !string.IsNullOrEmpty(heldItem);

    public void PickUpItem(string itemName)
    {
        heldItem = itemName;
        heldPerfect = false;
        Debug.Log("Eve memegang: " + heldItem);
    }

    public void ClearHand()
    {
        Debug.Log("Item " + heldItem + " digunakan/dibuang.");
        heldItem = "";
    }

    public string GetHeldItem()
    {
        return heldItem;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && HasItem)
            ClearHand();
    }
}