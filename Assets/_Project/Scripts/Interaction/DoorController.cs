using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptMessage = "Masuk";
    [SerializeField] private string targetSceneName = "Interior";
    private Animator anim;
    private bool isOpen = false;
    public static bool unlocked;
    public string Prompt => promptMessage;

    private void Awake()
    {
        unlocked = false;
        anim = GetComponent<Animator>();
    }
    
    public void Interact()
    {
        if (!unlocked) return;
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
        var o = new GameObject("DoorFader");
        DontDestroyOnLoad(o);
        var c = o.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 1000;
        var go = new GameObject("Img");
        go.transform.SetParent(o.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;
        var r = img.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.sizeDelta = Vector2.zero;
        o.AddComponent<Runner>().Run(img, targetSceneName, 1f);
    }

    private class Runner : MonoBehaviour
    {
        public void Run(Image img, string scene, float dur)
        {
            StartCoroutine(Go(img, scene, dur));
        }

        IEnumerator Go(Image img, string scene, float dur)
        {
            yield return Fade(img, 0f, 1f, dur);
            yield return new WaitForSecondsRealtime(0.3f);
            var op = SceneManager.LoadSceneAsync(scene);
            while (!op.isDone) yield return null;
            yield return null;
            yield return Fade(img, 1f, 0f, dur);
            Destroy(gameObject);
        }

        IEnumerator Fade(Image img, float a, float b, float d)
        {
            var col = img.color;
            for (float t = 0f; t < d; t += Time.unscaledDeltaTime)
            {
                col.a = Mathf.Lerp(a, b, t / d);
                img.color = col;
                yield return null;
            }
            col.a = b; img.color = col;
        }
    }
}