using TMPro;
using UnityEngine;

public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private PlayerInteractor player;
    private TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void Update()
    {
        var current = player.Current;
        label.text = current != null ? "[E] " + current.Prompt : "";
    }
}