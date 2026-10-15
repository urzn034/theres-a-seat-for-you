using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecipeBook : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text bookText;
    public GameManager gameManager;
    public DialogueManager dialogue;

    void Awake() { panel.SetActive(false); }

    void Update()
    {
        if (dialogue != null && dialogue.IsRunning) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (panel.activeSelf) Close();
            else Open();
        }
        else if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    void Open()
    {
        string t = "RECIPE BOOK\n\n";
        List<Recipe> list = gameManager != null ? gameManager.GetTodayRecipes() : null;
        if (list != null)
            foreach (var r in list)
                if (r != null) t += r.drinkName + ": " + string.Join(" + ", r.ingredients) + "\n";

        t += "\nCONTROLS\nWASD: Move\nE: Interact\nQ: Drop item\nR: Reset machine\nSpace: Brew\nB: Recipe book";
        bookText.text = t;
        panel.SetActive(true);
    }

    void Close() { panel.SetActive(false); }
}