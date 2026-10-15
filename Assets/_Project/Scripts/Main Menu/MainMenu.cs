using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject menuUI;
    public string nextScene = "Intro";

    bool loading;

    void Update()
    {
        if (loading || menuUI == null || !menuUI.activeInHierarchy) return;

        if (Input.anyKeyDown)
        {
            loading = true;
            SceneManager.LoadScene(nextScene);
        }
    }
}