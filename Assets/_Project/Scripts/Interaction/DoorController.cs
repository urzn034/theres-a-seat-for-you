using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptMessage = "Masuk";
    [SerializeField] private string targetSceneName = "Interior";
    private Animator anim;
    private bool isOpen = false;

    public string Prompt => promptMessage;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (isOpen) return;

        isOpen = true;
        if (anim != null) anim.SetBool("IsOpen", true);

        var player = FindObjectOfType<PlayerController>();
        if (player != null) player.canMove = false;

        // Panggil fungsi transisi scene
        Invoke("LoadInteriorScene", 0.5f);
    }

    private void LoadInteriorScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}