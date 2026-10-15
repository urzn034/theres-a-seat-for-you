using System;
using UnityEngine;

public class Trash : MonoBehaviour, IInteractable
{
    public static event Action OnTrashCleaned;

    public string Prompt => "Ambil sampah";

    public void Interact()
    {
        OnTrashCleaned?.Invoke();
        Destroy(gameObject);
    }
}