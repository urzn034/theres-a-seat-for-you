using UnityEngine;

public class FridgePopup : MonoBehaviour
{
    public GameObject panel;
    PlayerHand hand;

    void Awake() { panel.SetActive(false); }

    public void Open(PlayerHand h)
    {
        hand = h;
        panel.SetActive(true);
    }

    public void Pick(string ingredient)
    {
        if (hand != null && !hand.HasItem) hand.PickUpItem(ingredient);
        Close();
    }

    public void Close() { panel.SetActive(false); }

    void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Close();
    }
}