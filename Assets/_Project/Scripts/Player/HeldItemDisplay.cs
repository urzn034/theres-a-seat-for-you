using UnityEngine;
using System.Collections.Generic;

public class HeldItemDisplay : MonoBehaviour
{
    [System.Serializable]
    public class ItemIcon { public string itemName; public Sprite icon; }

    public PlayerHand hand;
    public SpriteRenderer iconRenderer;
    public List<ItemIcon> icons;


    public static HeldItemDisplay Instance;
    void Awake() { Instance = this; }

    public Sprite GetIcon(string itemName)
    {
        foreach (var i in icons)
            if (i.itemName == itemName) return i.icon;
        return null;
    }

    void Update()
    {
        string held = hand.GetHeldItem();
        iconRenderer.sprite = null;
        if (string.IsNullOrEmpty(held)) return;
        foreach (var i in icons)
            if (i.itemName == held) { iconRenderer.sprite = i.icon; return; }
    }
}