using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private string menuScene = "MainMenu";

    private GameObject panel;
    private bool paused;

    private void Start()
    {
        var co = new GameObject("PauseCanvas");
        var canvas = co.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var sc = co.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        co.AddComponent<GraphicRaycaster>();

        panel = new GameObject("PausePanel");
        panel.transform.SetParent(co.transform, false);
        var pr = panel.AddComponent<RectTransform>();
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one; pr.sizeDelta = Vector2.zero;


        MakeText("PAUSED", new Vector2(0, 200), 96);
        MakeButton("Resume", new Vector2(0, 40), Resume);
        MakeButton("Main Menu", new Vector2(0, -80), ToMenu);
        panel.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (paused) { Resume(); return; }
        foreach (var f in FindObjectsOfType<FridgePopup>())
            if (f.panel != null && f.panel.activeSelf) return;
        paused = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        paused = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ToMenu()
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.SaveSession();
        SceneManager.LoadScene(menuScene);
    }

    private void OnDestroy() { Time.timeScale = 1f; }

    private void MakeText(string s, Vector2 pos, float size)
    {
        var o = new GameObject("Text");
        o.transform.SetParent(panel.transform, false);
        var r = o.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(1200, 140);
        r.anchoredPosition = pos;
        var t = o.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = s; t.fontSize = size;
        t.color = new Color(1f, 0.95f, 0.85f);
        t.alignment = TextAlignmentOptions.Center;
    }

    private void MakeButton(string label, Vector2 pos, UnityEngine.Events.UnityAction act)
    {
        var o = new GameObject("Btn_" + label);
        o.transform.SetParent(panel.transform, false);
        var r = o.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(480, 90);
        r.anchoredPosition = pos;
        var img = o.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        var b = o.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(act);

        var t = new GameObject("Label");
        t.transform.SetParent(o.transform, false);
        var tr = t.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
        var tmp = t.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label; tmp.fontSize = 44;
        tmp.color = new Color(1f, 0.95f, 0.85f);
        tmp.alignment = TextAlignmentOptions.Center;
    }
}